using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Enums;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class ChatSessionRepository : GenericRepository<ChatSession>, IChatSessionRepository
{
    public ChatSessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChatSession>> GetSessionsByPatientAsync(int patientId)
    {
        return await _dbSet
            .Where(s => s.PatientId == patientId)
            .Include(s => s.Messages)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatSession>> GetRecentSessionsAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Include(s => s.Messages)
            .OrderByDescending(s => s.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> SessionExistsAsync(string sessionId)
    {
        return await _dbSet.AnyAsync(s => s.ChatSessionId == sessionId);
    }

    public async Task<ChatSession?> GetByIdWithMessagesAsync(string sessionId)
    {
        return await _dbSet
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId);
    }

    public override async Task<ChatSession> AddAsync(ChatSession entity)
    {
        if (string.IsNullOrEmpty(entity.ChatSessionId))
        {
            entity.ChatSessionId = GenerateSessionId();
        }

        await base.AddAsync(entity);
        return entity;
    }

    public override async Task UpdateAsync(ChatSession entity)
    {
        entity.UpdatedAt = DateTimeHelper.Now;
        await base.UpdateAsync(entity);
    }

    private static string GenerateSessionId()
    {
        return $"chat_{Guid.NewGuid():N}";
    }

    public async Task<IEnumerable<ChatSession>> GetExpiredSessionsAsync(int timeoutMinutes)
    {
        var threshold = DateTimeHelper.Now.AddMinutes(-timeoutMinutes);
        return await _dbSet
            .Where(s => s.Status == ChatSessionStatus.Active && s.UpdatedAt < threshold)
            .ToListAsync();
    }
}
