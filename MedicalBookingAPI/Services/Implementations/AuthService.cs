using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalBookingAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IJwtHelper _jwtHelper;

    public AuthService(
        IUserRepository userRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        IDepartmentRepository departmentRepository,
        IJwtHelper jwtHelper)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _departmentRepository = departmentRepository;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return null;
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            return null;
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = role,
            CreatedAt = DateTimeHelper.Now
        };

        await _userRepository.AddAsync(user);

        if (role == UserRole.Patient)
        {
            var patient = new Patient
            {
                UserId = user.UserId,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender
            };
            await _patientRepository.AddAsync(patient);
        }
        else if (role == UserRole.Doctor)
        {
            if (request.DepartmentId == null)
            {
                return null;
            }

            var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
            if (department == null)
            {
                return null;
            }

            var doctor = new Doctor
            {
                UserId = user.UserId,
                DepartmentId = request.DepartmentId.Value,
                Qualification = request.Qualification
            };
            await _doctorRepository.AddAsync(doctor);
        }

        var token = _jwtHelper.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            UserId = user.UserId
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtHelper.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            UserId = user.UserId
        };
    }

    public async Task<User?> GetCurrentUserAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        return true;
    }
}
