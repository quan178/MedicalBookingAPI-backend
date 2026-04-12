using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IChatSessionRepository : IGenericRepository<ChatSession>
{
    Task<IEnumerable<ChatSession>> GetSessionsByPatientAsync(int patientId);
    Task<IEnumerable<ChatSession>> GetRecentSessionsAsync(int pageNumber, int pageSize);
    Task<bool> SessionExistsAsync(string sessionId);
    Task<ChatSession?> GetByIdWithMessagesAsync(string sessionId);
    Task<IEnumerable<ChatSession>> GetExpiredSessionsAsync(int timeoutMinutes);
}
