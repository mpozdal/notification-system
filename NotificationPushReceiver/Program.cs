﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationPushReceiver.Rabbitmq;
using RabbitMQ.Client;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

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


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();

app.UseRouting();
#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});
#pragma warning restore ASP0014


app.Run();