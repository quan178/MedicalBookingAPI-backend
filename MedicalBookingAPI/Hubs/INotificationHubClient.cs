using MedicalBookingAPI.DTOs;

namespace MedicalBookingAPI.Hubs;

public interface INotificationHubClient
{
    Task ReceiveNotification(NotificationDto notification);
}
