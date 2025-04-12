using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NotificationAPI.Data;
using NotificationShared.Models;
using NotificationShared.Enums;

namespace NotificationAPI.Repositories;

public class NotificationRepository: INotificationRepository
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction;
    private INotificationRepository _notificationRepositoryImplementation;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Notification> AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task CancelAsync(Guid id)
    {
        var toCancel = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

        if (toCancel == null)
        {
            return;
        }
        toCancel.Status = NotificationStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
           
        }
        finally
        {
            await _transaction.DisposeAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }
}