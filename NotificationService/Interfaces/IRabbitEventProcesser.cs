namespace NotificationService.Interfaces;

public interface IRabbitEventProcesser
{
    public Task ProcessMessageAsync(string routingKey, string message);
}