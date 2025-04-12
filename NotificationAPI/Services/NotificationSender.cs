using Microsoft.AspNetCore.Http.HttpResults;
using NotificationAPI.Data;
using NotificationAPI.DTOs;
using NotificationShared.Models;
using NotificationAPI.RabbitMq;
using NotificationAPI.Repositories;
using NotificationShared.Enums;
using NotificationShared.Events;

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
    
    public async Task<Notification> SendNotification(NotificationCreateDto dto)
    {
        try
        {
            await _repository.BeginTransactionAsync();
            
            var notificationToAdd = new Notification()
            {
                Recipient = dto.Recipient,
                Channel = dto.Channel,
                Message = dto.Message,
                ScheduledAt = dto.ScheduledAt,
                TimeZone = dto.TimeZone,
                HighPriority = dto.HighPriority,
                ForceSend = dto.ForceSend
            };
            var createdNotification = await _repository.AddAsync(notificationToAdd);

            var notificationToPublish = new NotificationCreatedEvent()
            {
                NotificationId = createdNotification.Id,
                Channel = dto.Channel,
                ScheduledAt = dto.ScheduledAt,
                TimeZone = dto.TimeZone,
                HighPriority = dto.HighPriority,
                ForceSend = dto.ForceSend,
                Recipient = dto.Recipient,
            };
            _publisher.PublishNotificationScheduled(notificationToPublish);
            
            await _repository.CommitTransactionAsync();
            _logger.LogInformation("Notification {Id} created and published", notificationToAdd.Id);
            return createdNotification;
        }
        catch (Exception ex)
        {
            await _repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error sending notification");
            throw;

        }
    }

    public async Task UpdateStatus(Guid id, NotificationStatus status)
    {
        try
        {
            await _repository.BeginTransactionAsync();
            await _repository.UpdateStatus(id, status);
            await _repository.CommitTransactionAsync();
            _logger.LogInformation("Notification {Id} with status {Status}", id, status);
        }
        catch (Exception ex)
        {
            await _repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error sending notification");
        }
    }
    public async Task UpdateNotification(NotificationUpdateDto notification)
    {
        try
        {
            await _repository.BeginTransactionAsync();
            await _repository.UpdateNotification(notification);
            
            var toCancel = new NotificationCanceledEvent()
            {
                Id = notification.Id,
            };
            _publisher.PublishNotificationCanceled(toCancel);
            await _repository.CommitTransactionAsync();
            
        }
        catch (Exception ex)
        {
            await _repository.RollbackTransactionAsync();
            _logger.LogError(ex, "Error cancelling notification");
            throw;
        }
    }
    
    public async Task<Notification> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
        
    }
    
    
    

}