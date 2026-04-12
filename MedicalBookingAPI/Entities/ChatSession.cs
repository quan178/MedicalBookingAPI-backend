using MedicalBookingAPI.Enums;

namespace MedicalBookingAPI.Entities;

public class ChatSession
{
    public string ChatSessionId { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Active;
    public DateTime? EndedAt { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
