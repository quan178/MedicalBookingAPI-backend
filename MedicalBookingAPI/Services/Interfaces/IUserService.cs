using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDetailDto?> GetUserByIdAsync(int id);
    Task<bool> DeleteUserAsync(int id);
    Task<UserDetailDto?> UpdateUserAsync(int userId, UpdateUserRequest request);
}
