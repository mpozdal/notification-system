using System.Text;
using System.Text.Json;
using NotificationService.Interfaces;
using NotificationService.Repositories;
using NotificationService.Services;
using NotificationShared.Enums;
using NotificationShared.Events;
using NotificationShared.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.RabbitMq;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IRabbitEventProcesser _processor;
    private const string QueueName = "notification.queue";
    private const string ExchangeName = "notification.events";

    public RabbitMqConsumer(IConfiguration config, ILogger<RabbitMqConsumer> logger, IRabbitEventProcesser processor)
    {
        _logger = logger;
        _processor = processor;

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

        _channel.QueueBind(QueueName, ExchangeName, "notification.created");
        _channel.QueueBind(QueueName, ExchangeName, "notification.canceled");
        _channel.QueueBind(QueueName, ExchangeName, "notification.updated");
        _channel.QueueBind(QueueName, ExchangeName, "notification.forced");

        _logger.LogInformation("Connected to RabbitMQ");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                await _processor.ProcessMessageAsync(ea.RoutingKey, message);

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
