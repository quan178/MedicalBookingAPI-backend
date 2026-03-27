using MedicalBookingAPI.DTOs;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDetailDto>> GetAllDoctorsAsync();
    Task<DoctorDetailDto?> GetDoctorByIdAsync(int id);
    Task<DoctorDetailDto?> GetDoctorByUserIdAsync(int userId);
    Task<IEnumerable<DoctorDetailDto>> GetDoctorsByDepartmentAsync(int departmentId);
    Task<DoctorDetailDto> CreateDoctorAsync(CreateDoctorRequest request);
    Task<DoctorDetailDto?> UpdateDoctorAsync(int id, UpdateDoctorRequest request);
    Task<bool> AssignDepartmentAsync(int doctorId, int departmentId);
}
