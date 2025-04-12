using NotificationShared.Enums;

namespace NotificationShared.Events;

public class NotificationChangedStatusEvent
{
    public Guid Id { get; set; }
    public NotificationStatus NewStatus { get; set; }
}