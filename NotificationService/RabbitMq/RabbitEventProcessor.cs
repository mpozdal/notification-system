using System.Text.Json;
using NotificationService.Interfaces;
using NotificationService.Repositories;
using NotificationService.Services;
using NotificationShared.Enums;
using NotificationShared.Events;
using NotificationShared.Models;

namespace NotificationService.RabbitMq;

public class RabbitEventProcessor : IRabbitEventProcesser
{
    private readonly IServiceProvider _services;
    private readonly TimeConverter _converter;
    private readonly ILogger<RabbitEventProcessor> _logger;
    private readonly RabbitMqPublisher _publisher;

    public RabbitEventProcessor(
        IServiceProvider services, 
        TimeConverter converter,
        ILogger<RabbitEventProcessor> logger, RabbitMqPublisher publisher)
    {
        _services = services;
        _converter = converter;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task ProcessMessageAsync(string routingKey, string message)
    {
        _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

        try
        {
            object? @event = null;

            switch (routingKey)
            {
                case "notification.created":
                    @event = JsonSerializer.Deserialize<NotificationCreatedEvent>(message);
                    if (@event != null)
                    {
                        _logger.LogInformation("Processing scheduled notification: {Notification}", message);
                        await HandleScheduledAsync((NotificationCreatedEvent)@event);
                    }
                    break;
                case "notification.canceled":
                    @event = JsonSerializer.Deserialize<NotificationCanceledEvent>(message);
                    if (@event != null)
                    {
                        _logger.LogInformation("Processing canceled notification: {Notification}", message);
                        await HandleCanceledAsync((NotificationCanceledEvent)@event);
                    }
                    break;
                case "notification.updated":
                    @event = JsonSerializer.Deserialize<NotificationUpdatedEvent>(message);
                    if (@event != null)
                    {
                        _logger.LogInformation("Processing updated notification: {Notification}", message);
                        await HandleUpdatedAsync((NotificationUpdatedEvent)@event);
                    }
                    break;
                case "notification.forced":
                    @event = JsonSerializer.Deserialize<NotificationForcedEvent>(message);
                    if (@event != null)
                    {
                        _logger.LogInformation("Processing updated notification: {Notification}", message);
                        await HandleForcedToSendAsync((NotificationForcedEvent)@event);
                    }
                    break;
                default:
                    _logger.LogWarning("Unknown routing key received: {RoutingKey}", routingKey);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
            throw;
        }
    }

    private async Task HandleScheduledAsync(NotificationCreatedEvent @event)
    {
        await using var scope = _services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();

        try
        {
            await repository.BeginTransactionAsync();

            var scheduled = new NotificationScheduled
            {
                NotificationId = @event.NotificationId,
                Recipient = @event.Recipient,
                Channel = @event.Channel,
                ScheduledAtUtc = _converter.ConvertToUtc(@event.ScheduledAt, @event.TimeZone),
                Status = NotificationStatus.Scheduled,
                ForceSend = @event.ForceSend,
                HighPriority = @event.HighPriority,
            };

            await repository.AddAsync(scheduled);
            _publisher.PublishStatus(new NotificationChangedStatusEvent()
            {
                Id = @event.NotificationId,
                NewStatus = NotificationStatus.Scheduled
            });
            await repository.CommitTransactionAsync();
            _logger.LogInformation("Adding scheduled notification to database: {NotificationId}", @event.NotificationId);
        }
        catch (Exception ex)
        {
            await repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error while handling scheduled notification: {NotificationId}", @event.NotificationId);
            throw;
        }
    }

    private async Task HandleCanceledAsync(NotificationCanceledEvent @event)
    {
        await using var scope = _services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();

        try
        {
            await repository.BeginTransactionAsync();
            await repository.UpdateStatusAsync(@event.Id, NotificationStatus.Cancelled);
            _publisher.PublishStatus(new NotificationChangedStatusEvent()
            {
                Id = @event.Id,
                NewStatus = NotificationStatus.Cancelled
            });
            await repository.CommitTransactionAsync();
            _logger.LogInformation("Updating notification to cancelled: {NotificationId}", @event.Id);
        }
        catch (Exception ex)
        {
            await repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error while handling canceled notification: {NotificationId}", @event.Id);
            throw;
        }
    }

    private async Task HandleUpdatedAsync(NotificationUpdatedEvent @event)
    {
        await using var scope = _services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();

        try
        {
            await repository.BeginTransactionAsync();
            await repository.UpdateTimeAsync(@event.Id, _converter.ConvertToUtc(@event.NewScheduledAt, @event.NewTimezone));
            await repository.CommitTransactionAsync();
            _publisher.PublishStatus(new NotificationChangedStatusEvent()
            {
                Id = @event.Id,
                NewStatus = NotificationStatus.Scheduled
            });
            _logger.LogInformation("Updating notification time: {NotificationId}", @event.Id);
        }
        catch (Exception ex)
        {
            await repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error while handling updated notification: {NotificationId}", @event.Id);
            throw;
        }
    }

    private async Task HandleForcedToSendAsync(NotificationForcedEvent @event)
    {
        await using var scope = _services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();
        try
        {
            await repository.BeginTransactionAsync();
            await repository.ForceToSend(@event.Id);
            await repository.CommitTransactionAsync();
            _logger.LogInformation("Adding forced notification to send: {NotificationId}", @event.Id);
        }
        catch (Exception ex)
        {
            await repository.RollbackTransactionAsync();
            throw;
        }
    }
}

