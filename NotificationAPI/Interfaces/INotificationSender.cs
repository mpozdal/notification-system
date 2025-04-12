using NotificationAPI.DTOs;
using NotificationShared.Enums;
using NotificationShared.Events;
using NotificationShared.Models;

namespace NotificationAPI.Services;

public interface INotificationSender
{
    public  Task<Notification> SendNotification(NotificationCreateDto notification);
    public  Task<Notification> GetByIdAsync(Guid id);
    public Task UpdateStatus(Guid id, NotificationStatus status);
    public Task UpdateNotification(NotificationUpdateDto notification);
}