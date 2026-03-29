using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId)
    {
        return await _dbSet
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
    {
        return await _dbSet
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Include(a => a.MedicalRecord)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        return await _dbSet
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Include(a => a.MedicalRecord)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime appointmentTime)
    {
        var existingAppointment = await _dbSet
            .Where(a => a.DoctorId == doctorId && a.AppointmentTime == appointmentTime)
            .FirstOrDefaultAsync();
        return existingAppointment == null;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAndDateAsync(int doctorId, DateTime date)
    {
        return await _dbSet
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Where(a => a.DoctorId == doctorId && a.AppointmentTime.Date == date.Date)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public override async Task<Appointment?> GetByIdAsync(int id)
    {
        return await GetAppointmentWithDetailsAsync(id);
    }
}
