using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NotificationAPI.Data;
using NotificationAPI.Models;

namespace NotificationAPI.Repositories;

public class NotificationRepository: INotificationRepository
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _transaction;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(Notification entity)
    {
        await _context.Notifications.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task MarkNotificationAsSent(Notification notification)
    {
        notification.Status = "Sent";
        _context.Notifications.Update(notification);
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