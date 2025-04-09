namespace NotificationService.Models;

public class NotificationReadyEvent
{
    public Guid Id { get; set; }
    public string Channel { get; set; }
    public string Recipient { get; set; }
    public string Message { get; set; }
    
}