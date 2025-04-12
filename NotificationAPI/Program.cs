using Microsoft.EntityFrameworkCore;
using NotificationAPI.Data;
using NotificationAPI.RabbitMq;
using NotificationAPI.Repositories;
using NotificationAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<RabbitMQPublisher>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationSender, NotificationSender>();

builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
