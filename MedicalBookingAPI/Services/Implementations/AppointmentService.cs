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

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
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

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAppointmentStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null) return null;

        appointment.Status = status;
        await _appointmentRepository.UpdateAsync(appointment);

        return MapToDto(appointment);
    }

    public async Task<bool> CancelAppointmentAsync(int id, int patientId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
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
        return true;
    }

    public async Task<IEnumerable<AppointmentDto>> GetDoctorScheduleAsync(int doctorId, DateTime date)
    {
        var appointments = await _appointmentRepository.GetAppointmentsByDoctorAndDateAsync(doctorId, date);
        return appointments.Select(MapToDto);
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
}
