using System.Security.Claims;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public NotificationsController(
        INotificationService notificationService,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _notificationService = notificationService;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.SuccessResponse(notifications));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<UnreadCountDto>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<UnreadCountDto>.SuccessResponse(new UnreadCountDto { Count = count }));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAsReadAsync(id, userId);
        return Ok(ApiResponse.SuccessResponse("Đã đánh dấu là đã đọc"));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse.SuccessResponse("Đã đánh dấu tất cả là đã đọc"));
    }
}
