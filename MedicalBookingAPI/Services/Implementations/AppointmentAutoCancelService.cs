using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MedicalBookingAPI.Settings;

namespace MedicalBookingAPI.Services.Implementations;

public class AppointmentAutoCancelService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentAutoCancelService> _logger;
    private readonly AppointmentSettings _settings;

    public AppointmentAutoCancelService(
        IServiceProvider serviceProvider,
        ILogger<AppointmentAutoCancelService> logger,
        IOptions<AppointmentSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Auto-Cancel Service started. Grace period: {GracePeriod} minutes",
            _settings.AutoCancelGracePeriodMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await CancelExpiredAppointmentsAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "Error auto-cancelling expired appointments"); }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CancelExpiredAppointmentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();

        var expired = await repository.GetExpiredPendingAppointmentsAsync(_settings.AutoCancelGracePeriodMinutes);
        var expiredList = expired.ToList();

        if (expiredList.Count == 0) return;

        _logger.LogInformation("Found {Count} expired pending appointments to auto-cancel", expiredList.Count);

        foreach (var apt in expiredList)
        {
            apt.Status = AppointmentStatus.Cancelled;
            await repository.UpdateAsync(apt);
            _logger.LogInformation(
                "Appointment {AppointmentId} auto-cancelled. Was scheduled for {AppointmentTime}",
                apt.AppointmentId, apt.AppointmentTime);
        }
    }
}
