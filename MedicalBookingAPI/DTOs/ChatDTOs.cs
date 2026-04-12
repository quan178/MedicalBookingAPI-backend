using System.ComponentModel.DataAnnotations;

namespace MedicalBookingAPI.DTOs;

// ==================== Chat Session DTOs ====================

public class ChatSessionDto
{
    public string ChatSessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? EndedAt { get; set; }
}

public class ChatHistoryDto
{
    public string ChatSessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

// ==================== Chat Message DTOs ====================

public class ChatMessageDto
{
    public int ChatMessageId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? SuggestedSpecialty { get; set; }
    public double? ConfidenceScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ==================== Request DTOs ====================

public class CreateChatSessionRequest
{
    public int? PatientId { get; set; }
}

public class SendMessageRequest
{
    [Required(ErrorMessage = "Nội dung tin nhắn không được để trống")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Tin nhắn phải có độ dài từ 1-2000 ký tự")]
    public string Content { get; set; } = string.Empty;

    public string? SessionToken { get; set; }
}

public class ChatQueryRequest
{
    public int? PatientId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ==================== Response DTOs ====================

public class ChatSessionResponse
{
    public ChatSessionDto Session { get; set; } = null!;
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class SendMessageResponse
{
    public ChatMessageDto UserMessage { get; set; } = null!;
    public ChatMessageDto AssistantMessage { get; set; } = null!;
    public ChatSessionDto Session { get; set; } = null!;
}
