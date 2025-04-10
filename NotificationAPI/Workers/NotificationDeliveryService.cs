using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.RabbitMq;
using NotificationService.Repositories;

namespace NotificationService.Workers;

public class NotificationDeliveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public NotificationDeliveryService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

             using var scope = _serviceProvider.CreateScope();
            
             var repository = scope.ServiceProvider.GetRequiredService<NotificationRepository>();
             var producer = scope.ServiceProvider.GetRequiredService<RabbitMqPublisher>();
            
             var notifications = await repository.GetDueNotificationsAsync();
            
             foreach (var notification in notifications)
             {
                 producer.PublishNotification(notification);
                 await repository.MarkNotificationAsSent(notification);
             }


            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }
}