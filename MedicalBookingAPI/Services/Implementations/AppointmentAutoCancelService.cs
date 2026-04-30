using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;
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
        _logger.LogInformation("Appointment Auto-Cancel Service started. Auto-cancelling pending appointments {Hours} hours after creation",
            _settings.AutoCancelHoursAfterCreation);

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
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var expired = await repository.GetExpiredPendingAppointmentsAsync(_settings.AutoCancelHoursAfterCreation);
        var expiredList = expired.ToList();

        if (expiredList.Count == 0) return;

        _logger.LogInformation("Found {Count} expired pending appointments to auto-cancel", expiredList.Count);

        foreach (var apt in expiredList)
        {
            apt.Status = AppointmentStatus.Cancelled;
            await repository.UpdateAsync(apt);

            var appointmentTimeStr = apt.AppointmentTime.ToString("HH:mm dd/MM/yyyy");
            await notificationService.SendToUserAsync(
                apt.Patient!.UserId,
                "Lịch hẹn tự động bị hủy",
                $"Lịch khám vào {appointmentTimeStr} đã tự động bị hủy do chưa được xác nhận trong vòng 2 giờ.",
                NotificationType.AppointmentAutoCancelled,
                apt.AppointmentId);

            _logger.LogInformation(
                "Appointment {AppointmentId} auto-cancelled. Created at {CreatedAt}, was scheduled for {AppointmentTime}",
                apt.AppointmentId, apt.CreatedAt, apt.AppointmentTime);
        }
    }
}
