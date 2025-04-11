namespace NotificationService.Models;

public class NotificationScheduledEvent
{
    public Guid NotificationId { get; set; }
    public string Channel { get; set; } // "push" lub "email"
    public DateTime ScheduledAt { get; set; } // Czas w UTC
    public string Timezone { get; set; } // np. "Europe/Warsaw"
}