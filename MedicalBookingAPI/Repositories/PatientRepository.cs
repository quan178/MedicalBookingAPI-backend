using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetPatientWithUserAsync(int patientId)
    {
        return await _dbSet
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<Patient?> GetPatientByUserIdAsync(int userId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
