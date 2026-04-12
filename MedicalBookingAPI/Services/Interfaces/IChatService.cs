using MedicalBookingAPI.DTOs;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IChatService
{
    Task<ChatSessionResponse> CreateSessionAsync(int? patientId);
    Task<ChatSessionResponse?> GetSessionByTokenAsync(string sessionId, int? patientId);
    Task<ChatHistoryDto?> GetChatHistoryAsync(string sessionId, int? patientId);
    Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, int? patientId);
    Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(int? patientId);
    Task<bool> DeleteSessionAsync(string sessionId, int? patientId);
    Task<bool> EndSessionAsync(string sessionId, int? patientId);
}
