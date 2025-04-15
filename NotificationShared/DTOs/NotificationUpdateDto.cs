using NotificationShared.Enums;

namespace NotificationAPI.DTOs;

public class NotificationUpdateDto
{
    public Guid Id { get; init; }
    public DateTime? NewDatetime { get; init; }
    public NotificationStatus? NewStatus { get; init; }
    public Boolean? ForceToSend { get; init; }
}