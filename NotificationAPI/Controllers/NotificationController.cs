using Microsoft.AspNetCore.Mvc;
using NotificationAPI.DTOs;
using NotificationShared.Models;
using NotificationAPI.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController: Controller
{
    private readonly INotificationSender _service;

    public NotificationController(INotificationSender service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto notification)
    {
        var result = await _service.SendNotification(notification);

        return CreatedAtAction(nameof(GetById),new { id = result.Id }, result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var notification = await _service.GetByIdAsync(id);
        if (notification == null)
            return NotFound();

        return Ok(notification);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelNotification(Guid id)
    {
        await _service.CancelNotification(id);
        
        return NoContent();
    }
}