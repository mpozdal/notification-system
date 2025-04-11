using Microsoft.AspNetCore.Mvc;
using NotificationAPI.DTOs;
using NotificationAPI.Models;
using NotificationAPI.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController: Controller
{
    private readonly INotificationSender _notificationSender;

    public NotificationController(INotificationSender notificationSender)
    {
        _notificationSender = notificationSender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto notification)
    {
        await _notificationSender.SendNotification(notification);
        
        return Ok(notification);
    }
}