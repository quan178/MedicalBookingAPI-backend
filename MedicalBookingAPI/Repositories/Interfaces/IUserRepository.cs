using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserWithDetailsAsync(int userId);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task<bool> EmailExistsAsync(string email);
}
