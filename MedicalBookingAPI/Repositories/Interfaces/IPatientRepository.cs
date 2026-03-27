using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<Patient?> GetPatientWithUserAsync(int patientId);
    Task<Patient?> GetPatientByUserIdAsync(int userId);
}
