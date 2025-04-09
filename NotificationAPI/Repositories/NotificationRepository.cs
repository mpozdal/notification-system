using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Repositories;

public class NotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task Create(Notification entity)
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

    public async Task<List<Notification>> GetDueNotificationsAsync()
    {
        return await  _context.Notifications
            .Where(n => n.Status == "Scheduled" && n.SendAtUtc <= DateTime.UtcNow)
            .ToListAsync();
    }
}