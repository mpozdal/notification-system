using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Services;

public interface INotificationScheduler
{
    public  Task ScheduleNotification(NotificationCreateDto notification);
}