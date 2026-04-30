using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly INotificationService _notificationService;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        INotificationService notificationService)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByPatientAsync(patientId);
        return appointments.Select(MapToDto);
    }

    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByDoctorAsync(doctorId);
        return appointments.Select(MapToDto);
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment == null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(int patientId, CreateAppointmentRequest request)
    {
        if (request.AppointmentTime <= DateTimeHelper.Now)
        {
            throw new InvalidOperationException("Thời gian hẹn phải là thời gian trong tương lai");
        }

        var doctor = await _doctorRepository.GetDoctorWithDetailsAsync(request.DoctorId);
        if (doctor == null)
        {
            throw new InvalidOperationException("Bác sĩ không tồn tại");
        }

        var existingInDept = await _appointmentRepository.GetActiveAppointmentsByPatientDepartmentDateAsync(
            patientId, doctor.DepartmentId, request.AppointmentTime.Date);
        if (existingInDept.Any())
        {
            var existing = existingInDept.First();
            throw new InvalidOperationException(
                $"Bạn đã có lịch hẹn tại bộ phận '{existing.Doctor?.Department?.DepartmentName}' vào lúc {existing.AppointmentTime:HH:mm} "
                + $"ngày {existing.AppointmentTime:dd/MM/yyyy}. Vui lòng hủy lịch cũ trước khi đặt lịch mới.");
        }

        if (!await _appointmentRepository.IsTimeSlotAvailableAsync(request.DoctorId, request.AppointmentTime))
        {
            throw new InvalidOperationException("Khung giờ này đã được đặt");
        }

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            AppointmentTime = request.AppointmentTime,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTimeHelper.Now
        };

        await _appointmentRepository.AddAsync(appointment);
        appointment.Doctor = doctor;

        var patient = await _patientRepository.GetPatientWithUserAsync(patientId);
        var patientName = patient?.User?.FullName ?? "Bệnh nhân";
        var appointmentTimeStr = appointment.AppointmentTime.ToString("HH:mm dd/MM/yyyy");
        await _notificationService.SendToUserAsync(
            doctor.UserId,
            "Lịch hẹn mới",
            $"Bệnh nhân {patientName} vừa đặt lịch khám vào {appointmentTimeStr}.",
            NotificationType.AppointmentCreated,
            appointment.AppointmentId);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAppointmentStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
        if (appointment == null) return null;

        appointment.Status = status;
        await _appointmentRepository.UpdateAsync(appointment);

        var appointmentTimeStr = appointment.AppointmentTime.ToString("HH:mm dd/MM/yyyy");
        var doctorName = appointment.Doctor?.User?.FullName ?? "Bác sĩ";
        var patientName = appointment.Patient?.User?.FullName ?? "Bệnh nhân";

        if (status == AppointmentStatus.Confirmed)
        {
            await _notificationService.SendToUserAsync(
                appointment.Patient!.UserId,
                "Lịch hẹn được xác nhận",
                $"Bác sĩ {doctorName} đã xác nhận lịch khám của bạn vào {appointmentTimeStr}.",
                NotificationType.AppointmentConfirmed,
                appointment.AppointmentId);
        }
        else if (status == AppointmentStatus.Cancelled)
        {
            await _notificationService.SendToUserAsync(
                appointment.Patient.UserId,
                "Lịch hẹn đã bị hủy",
                $"Bác sĩ {doctorName} đã hủy lịch khám vào {appointmentTimeStr}.",
                NotificationType.AppointmentCancelled,
                appointment.AppointmentId);
        }

        return MapToDto(appointment);
    }

    public async Task<bool> CancelAppointmentAsync(int id, int patientId)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
        if (appointment == null || appointment.PatientId != patientId)
        {
            return false;
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Không thể hủy lịch hẹn đã hoàn thành");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _appointmentRepository.UpdateAsync(appointment);

        var appointmentTimeStr = appointment.AppointmentTime.ToString("HH:mm dd/MM/yyyy");
        var patientName = appointment.Patient?.User?.FullName ?? "Bệnh nhân";
        await _notificationService.SendToUserAsync(
            appointment.Doctor!.UserId,
            "Lịch hẹn bị hủy",
            $"Bệnh nhân {patientName} đã hủy lịch khám vào {appointmentTimeStr}.",
            NotificationType.AppointmentCancelled,
            appointment.AppointmentId);

        return true;
    }

    public async Task<IEnumerable<AppointmentDto>> GetDoctorScheduleAsync(int doctorId, DateTime date)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByDoctorAndDateAsync(doctorId, date);
        return appointments.Select(MapToDto);
    }

    public async Task<IEnumerable<AppointmentDto>> GetActiveAppointmentsByPatientDepartmentDateAsync(int patientId, int departmentId, DateTime date)
    {
        var appointments = await _appointmentRepository.GetActiveAppointmentsByPatientDepartmentDateAsync(patientId, departmentId, date);
        return appointments.Select(MapToDto);
    }

    public async Task<IEnumerable<AdminAppointmentDto>> GetFilteredAppointmentsAsync(AppointmentFilterRequest filter)
    {
        var appointments = await _appointmentRepository.GetFilteredAppointmentsAsync(filter);
        return appointments.Select(MapToAdminDto);
    }

    public async Task<AdminAppointmentDto?> GetAppointmentDetailsForAdminAsync(int id)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
        return appointment == null ? null : MapToAdminDto(appointment);
    }

    public async Task<AdminAppointmentDto?> AdminUpdateStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
        if (appointment == null) return null;

        appointment.Status = status;
        await _appointmentRepository.UpdateAsync(appointment);
        return MapToAdminDto(appointment);
    }

    public async Task<bool> AdminCancelAppointmentAsync(int id)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(id);
        if (appointment == null) return false;

        if (appointment.Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Không thể hủy lịch hẹn đã hoàn thành");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _appointmentRepository.UpdateAsync(appointment);
        return true;
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.User?.FullName ?? string.Empty,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor?.User?.FullName ?? string.Empty,
            DepartmentName = appointment.Doctor?.Department?.DepartmentName ?? string.Empty,
            AppointmentTime = appointment.AppointmentTime,
            Status = appointment.Status.ToString(),
            CreatedAt = appointment.CreatedAt
        };
    }

    private static AdminAppointmentDto MapToAdminDto(Appointment appointment)
    {
        return new AdminAppointmentDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.User?.FullName ?? string.Empty,
            PatientEmail = appointment.Patient?.User?.Email ?? string.Empty,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor?.User?.FullName ?? string.Empty,
            DepartmentName = appointment.Doctor?.Department?.DepartmentName ?? string.Empty,
            AppointmentTime = appointment.AppointmentTime,
            Status = appointment.Status.ToString(),
            CreatedAt = appointment.CreatedAt,
            HasMedicalRecord = appointment.MedicalRecord != null
        };
    }
}
