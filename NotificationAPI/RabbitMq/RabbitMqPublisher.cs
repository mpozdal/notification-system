using System.Text;
using System.Text.Json;
using NotificationService.Models;
using RabbitMQ.Client;

namespace NotificationService.RabbitMq;

public class RabbitMqPublisher
{
    private readonly IModel _channel;
    private readonly string _queueName = "notifications";

    public RabbitMqPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMq:Host"],
            UserName = "user",
            Password = "password"
        };

        var connection = factory.CreateConnection("notification-service-rabbitmq");
        _channel = connection.CreateModel();

        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void PublishNotification(Notification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(json);
        
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        
        _channel.BasicPublish(
            exchange: "",
            routingKey: _queueName,
            basicProperties: props,
            body: body);

        Console.WriteLine($"[x] Published notification to queue '{_queueName}': {json}");
    }
}