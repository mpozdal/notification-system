using NotificationAPI.DTOs;
using NotificationAPI.Models;

namespace NotificationAPI.Services;

public interface INotificationSender
{
    public  Task SendNotification(NotificationCreateDto notification);
}