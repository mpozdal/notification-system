using NotificationShared.Models;

namespace NotificationService.Interfaces;

public interface IRabbitMqPublisher
{
    public void PublishEmail(Notification notification);
    public void PublishPush(Notification notification);
}