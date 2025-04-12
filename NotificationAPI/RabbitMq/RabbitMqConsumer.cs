using System.Text;
using System.Text.Json;
using NotificationAPI.DTOs;
using NotificationAPI.Services;
using NotificationShared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationAPI.RabbitMq;

public class RabbitMqConsumer : BackgroundService
{

    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private const string QueueName = "notification.status";
    private const string ExchangeName = "notification.events";
    private readonly IServiceProvider _serviceProvider;


    public RabbitMqConsumer(IConfiguration config, ILogger<RabbitMqConsumer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:HostName"],
            UserName = config["RabbitMQ:UserName"],
            Password = config["RabbitMQ:Password"],
            DispatchConsumersAsync = true
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

        _channel.QueueBind(QueueName, ExchangeName, "notification.status");

        _logger.LogInformation("Connected to RabbitMQ");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        var scope = _serviceProvider.CreateScope();
        var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<NotificationChangedStatusEvent>(message);
                if (notification == null)
                    return;
                await notificationSender.UpdateStatus(notification.Id, notification.NewStatus);
                _logger.LogInformation(message);
                _channel.BasicAck(ea.DeliveryTag, multiple: false);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(QueueName, autoAck: false, consumer);

        return Task.CompletedTask;
    }
    public override void Dispose()
    {
        if (_channel.IsOpen) _channel.Close();
        if (_connection.IsOpen) _connection.Close();
        _logger.LogInformation("Disconnected from RabbitMQ");
        base.Dispose();
    }
}