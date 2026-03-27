using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return departments.Select(MapToDto);
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        return department == null ? null : MapToDto(department);
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        var department = new Department
        {
            DepartmentName = request.DepartmentName,
            Description = request.Description
        };

        await _departmentRepository.AddAsync(department);
        return MapToDto(department);
    }

    public async Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) return null;

        if (!string.IsNullOrEmpty(request.DepartmentName))
            department.DepartmentName = request.DepartmentName;

        if (request.Description != null)
            department.Description = request.Description;

        await _departmentRepository.UpdateAsync(department);
        return MapToDto(department);
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) return false;

        await _departmentRepository.DeleteAsync(id);
        return true;
    }

    private static DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentName = department.DepartmentName,
            Description = department.Description
        };
    }
}
