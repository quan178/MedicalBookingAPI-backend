using MedicalBookingAPI.DTOs;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request);
    Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request);
    Task<bool> DeleteDepartmentAsync(int id);
}
