using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class MedicalRecordRepository : GenericRepository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<MedicalRecord?> GetMedicalRecordWithDetailsAsync(int medicalRecordId)
    {
        return await _dbSet
            .Include(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
            .Include(m => m.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Include(m => m.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);
    }

    public async Task<IEnumerable<MedicalRecord>> GetMedicalRecordsByPatientAsync(int patientId)
    {
        return await _dbSet
            .Include(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
            .Include(m => m.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
            .Include(m => m.Appointment)
                .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.Department)
            .Where(m => m.Appointment.PatientId == patientId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<MedicalRecord?> GetMedicalRecordByAppointmentAsync(int appointmentId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);
    }

    public override async Task<MedicalRecord?> GetByIdAsync(int id)
    {
        return await GetMedicalRecordWithDetailsAsync(id);
    }
}
