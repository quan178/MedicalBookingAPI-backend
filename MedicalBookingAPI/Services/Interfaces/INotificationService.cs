using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);

    Task SendToUserAsync(int userId, string title, string message, NotificationType type, int? relatedId = null);
}
