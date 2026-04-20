namespace MedicalBookingAPI.Entities;

using System.Text.Json.Serialization;
using MedicalBookingAPI.Helpers;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageSender
{
    User,
    Assistant
}

public class ChatMessage
{
    public int ChatMessageId { get; set; }
    public string ChatSessionId { get; set; } = string.Empty;
    public MessageSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; } = false;
    public string? SuggestedSpecialty { get; set; }
    public double? ConfidenceScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now;

    // Navigation property
    public ChatSession ChatSession { get; set; } = null!;
}
