using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data;

public class AppDbContext: DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Notification> Notifications { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Recipient).IsRequired();
            entity.Property(n => n.Channel).IsRequired();
            entity.Property(n => n.Message).IsRequired();
            entity.Property(n => n.SendAtUtc).IsRequired();
            entity.Property(n => n.TimeZone).IsRequired();
            entity.Property(n => n.Status).IsRequired();
        });
    }
}