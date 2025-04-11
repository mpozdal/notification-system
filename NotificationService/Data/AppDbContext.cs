using Microsoft.EntityFrameworkCore;
using NotificationShared.Models;

namespace NotificationService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<NotificationScheduled?> NotificationScheduled { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationScheduled>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Status)
                .HasConversion<string>();
        });
    }
}