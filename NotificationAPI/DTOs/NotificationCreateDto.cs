namespace NotificationService.DTOs;

public class NotificationCreateDto
{
 
        public Guid Id { get; set; }
        public string Recipient { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
        public DateTime SendAtUtc { get; set; }
        public string TimeZone { get; set; }
        public string Status { get; set; }
}
