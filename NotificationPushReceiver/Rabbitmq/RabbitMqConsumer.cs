using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationShared.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationPushReceiver.Rabbitmq;

public class RabbitMqConsumer : BackgroundService
{

    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private const string QueueName = "notification.push.send";
    private const string ExchangeName = "notification";


    public RabbitMqConsumer( ILogger<RabbitMqConsumer> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "user",
            Password = "password",
            DispatchConsumersAsync = true
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

        _channel.QueueBind(QueueName, ExchangeName, QueueName);

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
                var notification = JsonSerializer.Deserialize<Notification>(message);

                _logger.LogInformation("PUSH RECEIVED\nID: {Id}\nUser {User} recevied message: {Message} at {Date}", notification.Id, notification.Recipient, notification.Message, notification.ScheduledAt);
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