using System.Net.Http.Json;
using NotificationService.Http;
using NotificationShared.Models;
using NotificationService.Interfaces;
using NotificationService.RabbitMq;
using NotificationService.Repositories;
using NotificationShared.Enums;
using NotificationShared.Events;


namespace NotificationService.Services;


public class NotificationDispatcher : BackgroundService
{
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly IServiceProvider _services;
    private readonly NotificationApiClient _httpClientFactory;
    private readonly RabbitMqPublisher _rabbitMqPublisher;
    private readonly Random _random = new();

    private const int MaxAttempts = 3;

    public NotificationDispatcher(
        ILogger<NotificationDispatcher> logger,
        IServiceProvider services,
        NotificationApiClient httpClientFactory,
        RabbitMqPublisher rabbitMqPublisher)
    {
        _logger = logger;
        _services = services;
        _httpClientFactory = httpClientFactory;
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dispatcher service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _services.CreateAsyncScope();
                var repo = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();
                var nowUtc = DateTime.UtcNow;

                var readyToSend = await repo.GetReadyNotificationsAsync(nowUtc);
                var attempts = 0;
                foreach (var notification in readyToSend)
                {
                    bool success = await TryDispatchAsync(notification);

                    await repo.BeginTransactionAsync();

                    if (success)
                    {
                        await repo.UpdateStatusAsync(notification.NotificationId, NotificationStatus.Sent);
                        _rabbitMqPublisher.PublishStatus(new NotificationChangedStatusEvent()
                        {
                            Id = notification.NotificationId,
                            NewStatus = NotificationStatus.Sent
                        });
                        _logger.LogInformation("Notification {Id} dispatched successfully", notification.NotificationId);
                    }
                    else
                    {
                        if (attempts + 1 >= MaxAttempts)
                        {
                            
                            await repo.UpdateStatusAsync(notification.NotificationId, NotificationStatus.Failed);
                            _rabbitMqPublisher.PublishStatus(new NotificationChangedStatusEvent()
                            {
                                Id = notification.NotificationId,
                                NewStatus = NotificationStatus.Failed
                            });
                            _logger.LogWarning("Notification {Id} marked as Failed after 3 attempts", notification.NotificationId);
                        }
                        else
                        {
                            _logger.LogWarning("Notification {Id} dispatch attempt failed", notification.NotificationId);
                        }
                    }

                    await repo.CommitTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during dispatch loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task<bool> TryDispatchAsync(NotificationScheduled scheduled)
    {
        var chance = 0.5;
        if (scheduled.HighPriority)
        {
            chance = 0.75;
        }
        if (_random.NextDouble() > chance)
        {
            _logger.LogWarning("Random failure simulation for notification {Id}", scheduled.NotificationId);
            return false;
        }

        try
        {
            var dto = await _httpClientFactory.GetNotificationDetailsAsync(scheduled.NotificationId);
            
            if (dto == null)
            {
                _logger.LogError("Deserialization failed for notification {Id}", scheduled.Id);
                return false;
            }

            switch (dto.Channel.ToLower())
            {
                case "email":
                    _rabbitMqPublisher.PublishEmail(dto);
                    break;
                case "push":
                    _rabbitMqPublisher.PublishPush(dto);
                    break;
                default:
                    _logger.LogWarning("Unsupported channel: {Channel}", dto.Channel);
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while dispatching notification {Id}", scheduled.Id);
            return false;
        }
    }
    
}

