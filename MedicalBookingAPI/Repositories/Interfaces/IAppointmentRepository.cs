using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
    Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime appointmentTime);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAndDateAsync(int doctorId, DateTime date);
    Task<IEnumerable<Appointment>> GetExpiredPendingAppointmentsAsync(int gracePeriodMinutes);
    Task<IEnumerable<Appointment>> GetActiveAppointmentsByPatientDepartmentDateAsync(int patientId, int departmentId, DateTime date);
    Task<IEnumerable<Appointment>> GetFilteredAppointmentsAsync(AppointmentFilterRequest filter);
}
