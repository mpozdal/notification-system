using System.Text;
using System.Text.Json;
using NotificationService.Interfaces;
using NotificationShared.Models;
using RabbitMQ.Client;
namespace NotificationService.RabbitMq;

public class RabbitMqPublisher: IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private const string ExchangeName = "notification";

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
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

    public void PublishEmail(Notification notification)
    {
        Publish(notification, "notification.email.send");
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

    public void PublishPush(Notification notification)
    {
        Publish(notification, "notification.push.send");
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

    private void Publish(object message, string routingKey)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Published message to {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to {RoutingKey}", routingKey);
            throw;
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