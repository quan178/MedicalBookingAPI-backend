using MedicalBookingAPI.Helpers;
using System.Text.Json.Serialization;

namespace MedicalBookingAPI.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    AppointmentCreated,
    AppointmentConfirmed,
    AppointmentCancelled,
    AppointmentAutoCancelled,
    AppointmentCompleted
}

public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int? RelatedId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now;

    public User User { get; set; } = null!;
}
