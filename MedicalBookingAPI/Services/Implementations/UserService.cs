using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public UserService(
        IUserRepository userRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return MapToUserDetailDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        await _userRepository.DeleteAsync(id);
        return true;
    }

    public async Task<UserDetailDto?> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetUserWithDetailsAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;
        if (request.Phone != null)
            user.Phone = request.Phone;

        if (user.Patient != null)
        {
            if (request.DateOfBirth.HasValue)
                user.Patient.DateOfBirth = request.DateOfBirth;
            if (request.Gender != null)
                user.Patient.Gender = request.Gender;
        }

        if (user.Doctor != null)
        {
            if (request.Qualification != null)
                user.Doctor.Qualification = request.Qualification;
        }

        await _userRepository.UpdateAsync(user);

        return MapToUserDetailDto(user);
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return null;
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = UserRole.Admin,
            CreatedAt = DateTimeHelper.Now
        };

        await _userRepository.AddAsync(user);
        return MapToUserDto(user);
    }

    private static UserDto MapToUserDto(Entities.User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }

    private static UserDetailDto MapToUserDetailDto(Entities.User user)
    {
        var dto = new UserDetailDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };

        if (user.Patient != null)
        {
            dto.Patient = new PatientDto
            {
                PatientId = user.Patient.PatientId,
                DateOfBirth = user.Patient.DateOfBirth,
                Gender = user.Patient.Gender
            };
        }

        if (user.Doctor != null)
        {
            dto.Doctor = new DoctorDto
            {
                DoctorId = user.Doctor.DoctorId,
                DepartmentId = user.Doctor.DepartmentId,
                DepartmentName = user.Doctor.Department?.DepartmentName,
                Qualification = user.Doctor.Qualification
            };
        }

        return dto;
    }
}
