using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<User?> GetCurrentUserAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
