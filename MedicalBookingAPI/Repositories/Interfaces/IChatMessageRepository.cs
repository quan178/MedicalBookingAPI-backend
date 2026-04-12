using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetMessagesBySessionIdAsync(string chatSessionId);
    Task<int> GetMessageCountBySessionIdAsync(string chatSessionId);
}
