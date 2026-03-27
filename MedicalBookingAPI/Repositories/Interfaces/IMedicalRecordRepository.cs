using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IMedicalRecordRepository : IGenericRepository<MedicalRecord>
{
    Task<MedicalRecord?> GetMedicalRecordWithDetailsAsync(int medicalRecordId);
    Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByPatientAsync(int patientId);
    Task<MedicalRecord?> GetMedicalRecordByAppointmentAsync(int appointmentId);
}
