using NotificationAPI.DTOs;
using NotificationShared.Enums;
using NotificationShared.Models;

namespace NotificationAPI.Repositories;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task UpdateStatus(Guid id, NotificationStatus status);
    Task UpdateNotification(NotificationUpdateDto notification);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}