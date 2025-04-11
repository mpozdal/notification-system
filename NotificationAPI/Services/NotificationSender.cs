using Microsoft.AspNetCore.Http.HttpResults;
using NotificationAPI.Data;
using NotificationAPI.DTOs;
using NotificationAPI.Models;
using NotificationAPI.RabbitMq;
using NotificationAPI.Repositories;

namespace NotificationAPI.Services;

public class NotificationSender :  INotificationSender
{
    private readonly INotificationRepository _repository;
    private readonly RabbitMQPublisher _publisher;
    private readonly ILogger<NotificationSender> _logger;
    public NotificationSender(INotificationRepository repository, RabbitMQPublisher publisher, ILogger<NotificationSender> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }
    
    public async Task SendNotification(NotificationCreateDto notification)
    {
        try
        {
            await _repository.BeginTransactionAsync();


            var notificationToAdd = new Notification()
            {
                Id = new Guid(),
                Recipient = notification.Recipient,
                Channel = notification.Channel,
                Message = notification.Message,
                SendAtUtc = notification.SendAtUtc,
                Status = "Pending",
                TimeZone = notification.TimeZone
            };
            await _repository.AddAsync(notificationToAdd);
            
            _publisher.PublishNotificationScheduled(notificationToAdd);
            
            await _repository.CommitTransactionAsync();
            _logger.LogInformation("Notification {Id} created and published", notification.Id);
        }
        catch (Exception ex)
        {
            await _repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error sending notification");
            throw;

        }
    }

}