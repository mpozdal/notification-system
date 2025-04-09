using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController: Controller
{
    private readonly INotificationScheduler _notificationScheduler;

    public NotificationController(INotificationScheduler notificationScheduler)
    {
        _notificationScheduler = notificationScheduler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto notification)
    {
        await _notificationScheduler.ScheduleNotification(notification);
        
        return Ok(notification);
    }
}