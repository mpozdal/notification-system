using NotificationShared.Enums;
namespace NotificationShared.Models;


public class Notification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Recipient { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public int RetryCount { get; init; } = 0;
    public bool HighPriority { get; init; } = false;
    public bool ForceSend { get; init; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

