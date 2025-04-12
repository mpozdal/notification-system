using NotificationShared.Enums;

namespace NotificationShared.Events;

public class NotificationCreatedEvent
{
    public Guid NotificationId { get; set; }
    public string Recipient { get; set; }
    public string Channel { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string TimeZone { get; set; } 
    public bool HighPriority { get; set; }
    public bool ForceSend { get; set; }
}