using System.Security.Claims;
using ImpensaCore.DTOs.Notifications;
using ImpensaCore.Models;
using ImpensaCore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImpensaCore.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext context;

    public NotificationsController(AppDbContext context)
    {
        this.context = context;
    }

    private Guid GetUserIdFromJwt()
    {
        var guid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(guid)) throw new ArgumentException("User id not found in JWT token");

        return Guid.Parse(guid);
    }

    private static NotificationResponseDto MapNotificationToResponseDto(Notification n)
    {
        return new NotificationResponseDto
        {
            Id = n.Id,
            Title = n.Title,
            Description = n.Description,
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead 
        };
    }

    private static Notification MapNotificationRequestDtoToNotification(NotificationRequestDto notificationDto, User user, DateTime createdAt)
    {
        return new Notification
        {
            Title = notificationDto.Title,
            Description = notificationDto.Description,
            User = user,
            CreatedAt = createdAt,
            IsRead = false 
        };
    }

    [HttpPost]
    public async Task<ActionResult<NotificationResponseDto>> CreateNotification(NotificationRequestDto dto)
    {
        var userId = GetUserIdFromJwt();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new { Message = "User not found" });

        var notification = MapNotificationRequestDtoToNotification(dto, user, DateTime.UtcNow);

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        return MapNotificationToResponseDto(notification);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetAllNotifications(
        bool? isRead, DateTime? startDate, DateTime? endDate, string searchQuery = null, bool recentOnly = false)
    {
        var userId = GetUserIdFromJwt();
        IQueryable<Notification> query = context.Notifications.Where(n => n.UserId == userId);

        // Filter by read/unread status
        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        // Filter by date range
        if (startDate.HasValue && endDate.HasValue)
        {
            query = query.Where(n => n.CreatedAt >= startDate.Value && n.CreatedAt <= endDate.Value);
        }

        // Search by query
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(n => n.Title.Contains(searchQuery) || n.Description.Contains(searchQuery));
        }

        // Get only recent notifications
        if (recentOnly)
        {
            query = query.OrderByDescending(n => n.CreatedAt).Take(10);
        }
        else
        {
            query = query.OrderByDescending(n => n.CreatedAt);
        }

        var notifications = await query.Select(n => MapNotificationToResponseDto(n)).ToListAsync();

        return Ok(notifications);
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var userId = GetUserIdFromJwt();

        var notification = await context.Notifications.FirstOrDefaultAsync(e => e.Id == id && e.User.Id == userId);
        if (notification == null) return NotFound();

        context.Notifications.Remove(notification);
        await context.SaveChangesAsync();

        return Ok();
    }


    [HttpPut("{id:guid}/toggle-read")]
    public async Task<IActionResult> ToggleNotificationReadStatus(Guid id)
    {
        var userId = GetUserIdFromJwt();
        var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null) return NotFound();

        notification.IsRead = !notification.IsRead;
        await context.SaveChangesAsync();

        return Ok();
    }


    [HttpPut("read/all")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        var userId = GetUserIdFromJwt();
        var notifications = await context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await context.SaveChangesAsync();

        return Ok();
    }
}

