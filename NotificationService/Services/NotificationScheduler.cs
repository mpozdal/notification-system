using NotificationShared.Models;
using System.Text;
using System.Text.Json;
using NotificationService.Repositories;
using NotificationShared.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Services
{
    public class NotificationScheduler : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<NotificationScheduler> _logger;
        private readonly IServiceProvider _services;
        private const string QueueName = "scheduler_queue";
        
        public NotificationScheduler(
            IConfiguration config, 
            ILogger<NotificationScheduler> logger,
            IServiceProvider services)
        {
            _logger = logger;
            _services = services;

            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:HostName"],
                UserName = config["RabbitMQ:UserName"],
                Password = config["RabbitMQ:Password"],
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(
                exchange: "notification_events", 
                type: ExchangeType.Topic,
                durable: true);
            
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
            
            _channel.QueueBind(
                queue: QueueName,
                exchange: "notification_events",
                routingKey: "notification.scheduled");
            
            _channel.QueueBind(
                queue: QueueName,
                exchange: "notification_events",
                routingKey: "notification.canceled");

            _channel.QueueBind(
                queue: QueueName,
                exchange: "notification_events",
                routingKey: "notification.updated");
            
            _logger.LogInformation("Scheduler connected to RabbitMQ");
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
                    var @event = JsonSerializer.Deserialize<Notification>(message);
                    
                    _logger.LogInformation("Received event for notification {NotificationId} with routingKey {RoutingKey}",
                        @event.Id, ea.RoutingKey);
                    
                    if (ea.RoutingKey == "notification.scheduled")
                    {
                        await ProcessScheduling(@event);
                    }
                    else if (ea.RoutingKey == "notification.canceled")
                    {
                        await ProcessCancellation(@event);
                    }
                    else if (ea.RoutingKey == "notification.updated")
                    {
                        await ProcessUpdate(@event);
                    }
                    
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Message acknowledged by RabbitMQ");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };
            
            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);
            
            return Task.CompletedTask;
        }
        
        private async Task ProcessScheduling(Notification @event)
        {
            await using var scope = _services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();

            try
            {
                await repository.BeginTransactionAsync();
                var scheduledTask = new NotificationScheduled()
                {
                    NotificationId = @event.Id,
                    Recipient = @event.Recipient,
                    Channel = @event.Channel,
                    ScheduledAtUtc = ConvertToUtc(@event.ScheduledAt, @event.TimeZone),
                    Status = NotificationStatus.Scheduled,
                    ForceSend = @event.ForceSend,
                    HighPriority =  @event.HighPriority,
                };
                await repository.AddAsync(scheduledTask);
                await repository.CommitTransactionAsync();
                _logger.LogInformation("Scheduled notification {NotificationId} for {ScheduledAt}", @event.Id, @event.ScheduledAt);
            }
            catch (Exception ex)
            {
                await repository.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing notification");
                throw;
            }
        }
        private async Task ProcessCancellation(Notification @event)
        {
            await using var scope = _services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();

            try
            {
                await repository.BeginTransactionAsync();
                await repository.UpdateStatusAsync(@event.Id, NotificationStatus.Cancelled);
                await repository.CommitTransactionAsync();
                _logger.LogInformation("Canceled notification {NotificationId}", @event.Id);
                
            }
            catch (Exception e)
            {
                await repository.RollbackTransactionAsync();
                throw;
            }

            
        }

        // Procesowanie zmiany terminu powiadomienia
        private async Task ProcessUpdate(Notification @event)
        {
            await using var scope = _services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<NotificationScheduledRepository>();

            try
            {
                await repository.BeginTransactionAsync();
                await repository.UpdateTimeAsync(@event.Id, ConvertToUtc(@event.ScheduledAt, @event.TimeZone));
                await repository.CommitTransactionAsync();
                _logger.LogInformation("Rescheduled notification {NotificationId}", @event.Id);
                
            }
            catch (Exception e)
            {
                await repository.RollbackTransactionAsync();
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
        private DateTime ConvertToUtc(DateTime localTime, string timeZoneId)
        {
            try
            {
                _logger.LogInformation("Converting {Time} to UTC using TimeZone: {TimeZone}", localTime, timeZoneId);
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                DateTime unspecified = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
                DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);

                return utcTime;
            }
            catch (TimeZoneNotFoundException)
            {
                _logger.LogError("TimeZoneNotFound: {TimeZone}", timeZoneId);
                throw new ArgumentException($"Nie znaleziono strefy czasowej: {timeZoneId}");
            }
            catch (InvalidTimeZoneException)
            {
                _logger.LogError("InvalidTimeZone: {TimeZone}", timeZoneId);
                throw new ArgumentException($"Strefa czasowa jest nieprawidłowa lub uszkodzona: {timeZoneId}");
            }
        }

        
    }
}
