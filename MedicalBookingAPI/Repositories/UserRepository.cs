using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetUserWithDetailsAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _dbSet.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public override async Task<User?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Patient)
            .Include(u => u.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }
}
