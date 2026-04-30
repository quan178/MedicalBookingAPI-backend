namespace MedicalBookingAPI.DTOs;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? RelatedId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
