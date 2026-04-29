using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId);
    Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId);
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
    Task<AppointmentDto> CreateAppointmentAsync(int patientId, CreateAppointmentRequest request);
    Task<AppointmentDto?> UpdateAppointmentStatusAsync(int id, AppointmentStatus status);
    Task<bool> CancelAppointmentAsync(int id, int patientId);
    Task<IEnumerable<AppointmentDto>> GetDoctorScheduleAsync(int doctorId, DateTime date);
    Task<IEnumerable<AppointmentDto>> GetActiveAppointmentsByPatientDepartmentDateAsync(int patientId, int departmentId, DateTime date);
    Task<IEnumerable<AdminAppointmentDto>> GetFilteredAppointmentsAsync(AppointmentFilterRequest filter);
    Task<AdminAppointmentDto?> GetAppointmentDetailsForAdminAsync(int id);
    Task<AdminAppointmentDto?> AdminUpdateStatusAsync(int id, AppointmentStatus status);
    Task<bool> AdminCancelAppointmentAsync(int id);
}
