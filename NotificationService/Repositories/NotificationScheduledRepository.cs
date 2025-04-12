using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NotificationService.Data;
using NotificationShared.Models;
using NotificationShared.Enums;

namespace NotificationService.Repositories;

public class NotificationScheduledRepository
{
    private readonly AppDbContext _context;
    private  IDbContextTransaction _transaction;

    public NotificationScheduledRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationScheduled? notification)
    {
        await _context.NotificationScheduled.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(NotificationScheduled? notification)
    {
        _context.NotificationScheduled.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<NotificationScheduled?> GetAsync(string notificationId)
    {
        return await _context.NotificationScheduled.FindAsync(notificationId);
    }
    public async Task UpdateStatusAsync(Guid notificationId, NotificationStatus status)
    {
        var scheduled = await _context.NotificationScheduled
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

        if (scheduled == null)
        {
            return;
        }

        scheduled.Status = status;
        await _context.SaveChangesAsync();
    }
    public async Task UpdateTimeAsync(Guid notificationId, DateTime time)
    {
        var scheduled = await _context.NotificationScheduled
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

        if (scheduled == null)
        {
            return;
        }

        scheduled.ScheduledAtUtc = time;
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationScheduled>> GetReadyNotificationsAsync(DateTime nowUtc)
    {
        return await _context.NotificationScheduled.Where(n =>
            n.ScheduledAtUtc < nowUtc && n.Status == NotificationStatus.Scheduled).ToListAsync();
    }

    public async Task IncrementAttemptAsync(Guid notificationId)
    {
        var n = await _context.NotificationScheduled
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        n.RetryCount += 1;
        
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