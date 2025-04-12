
using NotificationShared.Events;
using NotificationShared.Models;

namespace NotificationAPI.RabbitMq;

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMQPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private const string ExchangeName = "notification.events";

    public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"],
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ConfirmSelect();
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _logger.LogInformation("Connected to RabbitMQ");
    }

    public void PublishNotificationScheduled(NotificationCreatedEvent notification)
    {

        PublishMessage(notification, "notification.created");
        if (_channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
        {
            _logger.LogInformation("Message confirmed by RabbitMQ");
        }
        else
        {
            _logger.LogWarning("Message not confirmed by RabbitMQ");
            throw new Exception("Broker confirmation timeout");
        }
    }

    private void PublishMessage(object message, string routingKey)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published message with routing key {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ");
            throw;
        }
    }
    public  void PublishNotificationCanceled(NotificationCanceledEvent toCancel)
    {
        PublishMessage(toCancel, "notification.canceled");

        if (_channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
        {
            _logger.LogInformation("Cancellation event confirmed by RabbitMQ");
        }
        else
        {
            _logger.LogWarning("Cancellation event not confirmed by RabbitMQ");
            throw new Exception("Broker confirmation timeout");
        }
    }

    public void PublishNotificationUpdated(NotificationUpdatedEvent notification)
    {
        PublishMessage(notification, "notification.updated");

        if (_channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
        {
            _logger.LogInformation("Update event confirmed by RabbitMQ");
        }
        else
        {
            _logger.LogWarning("Update event not confirmed by RabbitMQ");
            throw new Exception("Broker confirmation timeout");
        }
    }


    public void Dispose()
    {
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
            _logger.LogInformation("Channel closed.");
        }

        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
            _logger.LogInformation("Connection closed.");
        }

        _logger.LogInformation("Disconnected from RabbitMQ");
    }
}