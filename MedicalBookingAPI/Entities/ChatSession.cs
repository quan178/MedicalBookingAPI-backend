using MedicalBookingAPI.Enums;
using MedicalBookingAPI.Helpers;

namespace MedicalBookingAPI.Entities;

public class ChatSession
{
    public string ChatSessionId { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now;
    public DateTime UpdatedAt { get; set; } = DateTimeHelper.Now;
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Active;
    public DateTime? EndedAt { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
