namespace NotificationAPI.DTOs;
using NotificationShared.Enums;
public class NotificationCreateDto
{
    public string Recipient { get; set; }
    public string Channel { get; set; }
    public string Message { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string TimeZone { get; init; }
    public bool HighPriority { get; init; }
    public bool ForceSend { get; init; }
}
