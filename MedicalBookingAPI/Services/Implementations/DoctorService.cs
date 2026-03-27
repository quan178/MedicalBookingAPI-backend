using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Services.Implementations;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IJwtHelper _jwtHelper;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        IJwtHelper jwtHelper)
    {
        _doctorRepository = doctorRepository;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<IEnumerable<DoctorDetailDto>> GetAllDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetAllDoctorsWithDetailsAsync();
        return doctors.Select(MapToDetailDto);
    }

    public async Task<DoctorDetailDto?> GetDoctorByIdAsync(int id)
    {
        var doctor = await _doctorRepository.GetDoctorWithDetailsAsync(id);
        return doctor == null ? null : MapToDetailDto(doctor);
    }

    public async Task<DoctorDetailDto?> GetDoctorByUserIdAsync(int userId)
    {
        var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
        if (doctor == null) return null;
        return await GetDoctorByIdAsync(doctor.DoctorId);
    }

    public async Task<IEnumerable<DoctorDetailDto>> GetDoctorsByDepartmentAsync(int departmentId)
    {
        var doctors = await _doctorRepository.GetDoctorsByDepartmentAsync(departmentId);
        return doctors.Select(MapToDetailDto);
    }

    public async Task<DoctorDetailDto> CreateDoctorAsync(CreateDoctorRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
        if (department == null)
        {
            throw new InvalidOperationException("Department not found");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = UserRole.Doctor,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        var doctor = new Doctor
        {
            UserId = user.UserId,
            DepartmentId = request.DepartmentId,
            Qualification = request.Qualification
        };

        await _doctorRepository.AddAsync(doctor);
        doctor.User = user;
        doctor.Department = department;

        return MapToDetailDto(doctor);
    }

    public async Task<DoctorDetailDto?> UpdateDoctorAsync(int id, UpdateDoctorRequest request)
    {
        var doctor = await _doctorRepository.GetDoctorWithDetailsAsync(id);
        if (doctor == null) return null;

        var user = doctor.User;
        if (request.Qualification != null)
            doctor.Qualification = request.Qualification;
        if (request.Phone != null)
            user.Phone = request.Phone;

        await _userRepository.UpdateAsync(user);
        await _doctorRepository.UpdateAsync(doctor);

        return MapToDetailDto(doctor);
    }

    public async Task<bool> AssignDepartmentAsync(int doctorId, int departmentId)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId);
        if (doctor == null) return false;

        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) return false;

        doctor.DepartmentId = departmentId;
        await _doctorRepository.UpdateAsync(doctor);
        return true;
    }

    private static DoctorDetailDto MapToDetailDto(Doctor doctor)
    {
        return new DoctorDetailDto
        {
            DoctorId = doctor.DoctorId,
            UserId = doctor.UserId,
            FullName = doctor.User?.FullName ?? string.Empty,
            Email = doctor.User?.Email ?? string.Empty,
            Phone = doctor.User?.Phone,
            DepartmentId = doctor.DepartmentId,
            DepartmentName = doctor.Department?.DepartmentName ?? string.Empty,
            Qualification = doctor.Qualification
        };
    }
}
