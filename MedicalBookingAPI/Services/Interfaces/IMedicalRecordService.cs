using MedicalBookingAPI.DTOs;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IMedicalRecordService
{
    Task<IEnumerable<MedicalRecordDto>> GetMedicalRecordsByPatientAsync(int patientId);
    Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(int id);
    Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordRequest request, int doctorId);
    Task<IEnumerable<MedicalRecordDto>> GetMedicalRecordsByDoctorAsync(int doctorId);
    Task<MedicalRecordDto> UpdateMedicalRecordAsync(int id, UpdateMedicalRecordRequest request);
}
