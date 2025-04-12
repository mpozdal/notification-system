using NotificationShared.Models;
using System.Text;
using System.Text.Json;
using NotificationService.Repositories;
using NotificationShared.Enums;
using NotificationShared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Services
{
    public class NotificationScheduler
    {
        private readonly ILogger<NotificationScheduler> _logger;
        private readonly NotificationScheduledRepository _repository;

        public NotificationScheduler(ILogger<NotificationScheduler> logger, NotificationScheduledRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }
        
    }

}
