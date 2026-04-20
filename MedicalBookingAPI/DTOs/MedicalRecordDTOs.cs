using System.ComponentModel.DataAnnotations;

namespace MedicalBookingAPI.DTOs;

public class MedicalRecordDto
{
    public int MedicalRecordId { get; set; }
    public int AppointmentId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? DoctorDiagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public bool IsEncrypted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMedicalRecordRequest
{
    [Required(ErrorMessage = "ID lịch hẹn không được để trống")]
    public int AppointmentId { get; set; }

    public string? DoctorDiagnosis { get; set; }

    public string? Treatment { get; set; }

    public string? Prescription { get; set; }
}

public class UpdateMedicalRecordRequest
{
    public string? DoctorDiagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
}
