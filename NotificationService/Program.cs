using Microsoft.EntityFrameworkCore;
using NotificationService;
using NotificationService.Data;
using NotificationService.Http;
using NotificationService.Interfaces;
using NotificationService.RabbitMq;
using NotificationService.Repositories;
using NotificationService.Services;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IConnection>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = config["RabbitMQ:HostName"],
        UserName = config["RabbitMQ:UserName"],
        Password = config["RabbitMQ:Password"]
    };
    return factory.CreateConnection();
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddHttpClient();

builder.Services.AddSingleton<TimeConverter>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<NotificationApiClient>();
builder.Services.AddSingleton<IRabbitEventProcesser, RabbitEventProcessor>();

builder.Services.AddScoped<NotificationScheduledRepository>();

builder.Services.AddHostedService<NotificationDispatcher>();
builder.Services.AddHostedService<RabbitMqConsumer>();

var host = builder.Build();
host.Run();