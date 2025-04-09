using Microsoft.AspNetCore.Http.HttpResults;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Services;

public class NotificationScheduler :  INotificationScheduler
{
    private readonly NotificationRepository _repository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationScheduler(IServiceScopeFactory scopeFactory, NotificationRepository repository)
    {
        _serviceScopeFactory = scopeFactory;
        _repository = repository;
    }
    
    public async Task ScheduleNotification(NotificationCreateDto notification)
    {
        var notificationToAdd = new Notification()
        {
            Id = new Guid(),
            Recipient = notification.Recipient,
            Channel = notification.Channel,
            Message = notification.Message,
            SendAtUtc = notification.SendAtUtc,
            Status = "Scheduled",
            TimeZone = notification.TimeZone
        };
        await _repository.Create(notificationToAdd);
    }

}