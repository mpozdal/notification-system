namespace NotificationShared.Events;

public class NotificationUpdatedEvent
{
    public Guid Id { get; set; }
    public DateTime NewScheduledAt { get; set; }
    public string NewTimezone { get; set; }
}