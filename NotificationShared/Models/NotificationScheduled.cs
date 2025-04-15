using NotificationShared.Enums;

namespace NotificationShared.Models;

public class NotificationScheduled
{
   

    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid NotificationId { get; set; }
    public string Recipient { get; init; }
    public string Channel { get; init; }
    public DateTime ScheduledAtUtc { get; set; }
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; } = 0;
    public bool HighPriority { get; init; } = false;
    public bool ForceSend { get; set; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}