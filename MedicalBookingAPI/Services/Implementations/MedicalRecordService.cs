using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Services.Implementations;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;

    public MedicalRecordService(
        IMedicalRecordRepository medicalRecordRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetMedicalRecordsByPatientAsync(int patientId)
    {
        var records = await _medicalRecordRepository.GetMedicalRecordsByPatientAsync(patientId);
        return records.Select(MapToDto);
    }

    public async Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(int id)
    {
        var record = await _medicalRecordRepository.GetByIdAsync(id);
        return record == null ? null : MapToDto(record);
    }

    public async Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordRequest request, int doctorId)
    {
        var appointment = await _appointmentRepository.GetAppointmentWithDetailsAsync(request.AppointmentId);
        if (appointment == null)
        {
            throw new InvalidOperationException("Lịch hẹn không tồn tại");
        }

        if (appointment.DoctorId != doctorId)
        {
            throw new InvalidOperationException("Không có quyền tạo hồ sơ bệnh án cho lịch hẹn này");
        }

        if (appointment.Status != AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Chỉ có thể tạo hồ sơ bệnh án cho lịch hẹn đã hoàn thành");
        }

        var existingRecord = await _medicalRecordRepository.GetMedicalRecordByAppointmentAsync(request.AppointmentId);
        if (existingRecord != null)
        {
            throw new InvalidOperationException("Lịch hẹn này đã có hồ sơ bệnh án");
        }

        var medicalRecord = new MedicalRecord
        {
            AppointmentId = request.AppointmentId,
            DoctorDiagnosis = request.DoctorDiagnosis,
            Treatment = request.Treatment,
            Prescription = request.Prescription,
            CreatedAt = DateTime.UtcNow
        };

        await _medicalRecordRepository.AddAsync(medicalRecord);
        medicalRecord.Appointment = appointment;

        return MapToDto(medicalRecord);
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetMedicalRecordsByDoctorAsync(int doctorId)
    {
        var doctor = await _doctorRepository.GetDoctorWithDetailsAsync(doctorId);
        if (doctor == null)
        {
            return Enumerable.Empty<MedicalRecordDto>();
        }

        var appointments = await _appointmentRepository.GetAppointmentsByDoctorAsync(doctorId);
        var records = new List<MedicalRecordDto>();

        foreach (var appointment in appointments)
        {
            if (appointment.MedicalRecord != null)
            {
                records.Add(MapToDto(appointment.MedicalRecord));
            }
        }

        return records;
    }

    public async Task<MedicalRecordDto> UpdateMedicalRecordAsync(int id, UpdateMedicalRecordRequest request)
    {
        var record = await _medicalRecordRepository.GetByIdAsync(id);
        if (record == null)
        {
            throw new InvalidOperationException("Hồ sơ bệnh án không tồn tại");
        }

        if (request.DoctorDiagnosis != null)
            record.DoctorDiagnosis = request.DoctorDiagnosis;
        if (request.Treatment != null)
            record.Treatment = request.Treatment;
        if (request.Prescription != null)
            record.Prescription = request.Prescription;

        await _medicalRecordRepository.UpdateAsync(record);

        return MapToDto(record);
    }

    private static MedicalRecordDto MapToDto(MedicalRecord record)
    {
        return new MedicalRecordDto
        {
            MedicalRecordId = record.MedicalRecordId,
            AppointmentId = record.AppointmentId,
            AppointmentTime = record.Appointment?.AppointmentTime ?? DateTime.MinValue,
            PatientName = record.Appointment?.Patient?.User?.FullName ?? string.Empty,
            DoctorName = record.Appointment?.Doctor?.User?.FullName ?? string.Empty,
            DepartmentName = record.Appointment?.Doctor?.Department?.DepartmentName ?? string.Empty,
            DoctorDiagnosis = record.DoctorDiagnosis,
            Treatment = record.Treatment,
            Prescription = record.Prescription,
            CreatedAt = record.CreatedAt
        };
    }
}
