using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Doctor?> GetDoctorWithDetailsAsync(int doctorId)
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
    }

    public async Task<Doctor?> GetDoctorByUserIdAsync(int userId)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<IEnumerable<Doctor>> GetDoctorsByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsWithDetailsAsync()
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.Department)
            .ToListAsync();
    }

    public override async Task<Doctor?> GetByIdAsync(int id)
    {
        return await GetDoctorWithDetailsAsync(id);
    }
}
