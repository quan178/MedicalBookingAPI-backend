using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Hubs;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MedicalBookingAPI.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<NotificationHub, INotificationHubClient> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepository,
        IHubContext<NotificationHub, INotificationHubClient> hubContext)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId, userId);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task SendToUserAsync(int userId, string title, string message, NotificationType type, int? relatedId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedId = relatedId,
            IsRead = false,
            CreatedAt = DateTimeHelper.Now
        };

        await _notificationRepository.AddAsync(notification);

        var dto = MapToDto(notification);
        await _hubContext.Clients.Group(userId.ToString()).ReceiveNotification(dto);
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            NotificationId = notification.NotificationId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            RelatedId = notification.RelatedId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}
