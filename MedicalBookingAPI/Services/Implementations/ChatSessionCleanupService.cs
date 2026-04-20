using MedicalBookingAPI.Enums;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MedicalBookingAPI.Services.Implementations;

public class ChatSessionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatSessionCleanupService> _logger;
    private readonly ChatSettings _chatSettings;

    public ChatSessionCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ChatSessionCleanupService> logger,
        IOptions<ChatSettings> chatSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _chatSettings = chatSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Chat Session Cleanup Service started. Timeout: {Timeout} minutes",
            _chatSettings.InactivityTimeoutMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired chat sessions");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CleanupExpiredSessionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IChatSessionRepository>();

        var expiredSessions = await repository.GetExpiredSessionsAsync(_chatSettings.InactivityTimeoutMinutes);
        var expiredList = expiredSessions.ToList();

        if (expiredList.Count == 0)
            return;

        _logger.LogInformation("Found {Count} expired chat sessions to end", expiredList.Count);

        foreach (var session in expiredList)
        {
            session.Status = ChatSessionStatus.Ended;
            session.EndedAt = DateTimeHelper.Now;
            await repository.UpdateAsync(session);

            _logger.LogInformation(
                "Session {SessionId} auto-ended due to {Timeout} minutes of inactivity. Last activity: {LastActivity}",
                session.ChatSessionId,
                _chatSettings.InactivityTimeoutMinutes,
                session.UpdatedAt);
        }
    }
}
