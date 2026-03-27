using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<Doctor?> GetDoctorWithDetailsAsync(int doctorId);
    Task<Doctor?> GetDoctorByUserIdAsync(int userId);
    Task<IEnumerable<Doctor>> GetDoctorsByDepartmentAsync(int departmentId);
    Task<IEnumerable<Doctor>> GetAllDoctorsWithDetailsAsync();
}
