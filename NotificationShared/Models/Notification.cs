using NotificationService.Enums;


namespace NotificationAPI.Models;


public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Recipient { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public int RetryCount { get; set; } = 0;
    public bool HighPriority { get; set; } = false;
    public bool ForceSend { get; set; } = false;
    public bool IsCanceled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

