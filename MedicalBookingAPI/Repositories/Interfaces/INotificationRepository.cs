using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}
