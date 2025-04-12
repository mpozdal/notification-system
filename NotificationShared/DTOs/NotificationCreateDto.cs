namespace NotificationAPI.DTOs;
using NotificationShared.Enums;
public class NotificationCreateDto
{
    public string Recipient { get; init; }
    public string Channel { get; init; }
    public string Message { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string TimeZone { get; init; }
    public bool HighPriority { get; init; }
    public bool ForceSend { get; init; }
}
