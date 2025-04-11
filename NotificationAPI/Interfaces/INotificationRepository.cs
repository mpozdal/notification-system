using NotificationAPI.Models;

namespace NotificationAPI.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task MarkNotificationAsSent(Notification notification);
    
    
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}