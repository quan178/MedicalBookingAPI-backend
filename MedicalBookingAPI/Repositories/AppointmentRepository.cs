using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
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
            .Where(a => a.DoctorId == doctorId && a.AppointmentTime == appointmentTime
                        && a.Status != AppointmentStatus.Cancelled)
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

    public async Task<IEnumerable<Appointment>> GetExpiredPendingAppointmentsAsync(int gracePeriodMinutes)
    {
        var cutoff = DateTimeHelper.Now.AddMinutes(-gracePeriodMinutes);
        return await _dbSet
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Where(a => a.Status == AppointmentStatus.Pending
                        && a.AppointmentTime < cutoff)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetActiveAppointmentsByPatientDepartmentDateAsync(int patientId, int departmentId, DateTime date)
    {
        return await _dbSet
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Where(a => a.PatientId == patientId
                        && a.Doctor.DepartmentId == departmentId
                        && a.AppointmentTime.Date == date.Date
                        && (a.Status == AppointmentStatus.Pending
                            || a.Status == AppointmentStatus.Confirmed))
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetFilteredAppointmentsAsync(AppointmentFilterRequest filter)
    {
        var query = _dbSet
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Include(a => a.MedicalRecord)
            .AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.AppointmentTime.Date >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.AppointmentTime.Date <= filter.ToDate.Value.Date);
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<AppointmentStatus>(filter.Status, out var status))
            query = query.Where(a => a.Status == status);
        if (filter.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == filter.DoctorId.Value);
        if (filter.PatientId.HasValue)
            query = query.Where(a => a.PatientId == filter.PatientId.Value);

        return await query.OrderByDescending(a => a.AppointmentTime).ToListAsync();
    }

    public override async Task<Appointment?> GetByIdAsync(int id)
    {
        return await GetAppointmentWithDetailsAsync(id);
    }
}
