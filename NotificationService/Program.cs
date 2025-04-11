using Microsoft.EntityFrameworkCore;
using NotificationService;
using NotificationService.Data;
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

builder.Services.AddHostedService<NotificationScheduler>();
builder.Services.AddScoped<NotificationScheduledRepository>();


var host = builder.Build();
host.Run();