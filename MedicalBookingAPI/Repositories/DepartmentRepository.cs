using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.DepartmentName == name);
    }

    public override async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _dbSet.Include(d => d.Doctors).ToListAsync();
    }
}
