using NotificationAPI.DTOs;
using NotificationShared.Models;

namespace NotificationAPI.Services;

public interface INotificationSender
{
    public  Task<Notification> SendNotification(NotificationCreateDto notification);
    public  Task<Notification> GetByIdAsync(Guid id);
    public Task CancelNotification(Guid id);
}