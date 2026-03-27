using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
