using System.ComponentModel.DataAnnotations;
using MedicalBookingAPI.Entities;

namespace MedicalBookingAPI.DTOs;

public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AppointmentTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentRequest
{
    [Required(ErrorMessage = "ID bác sĩ không được để trống")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Thời gian hẹn không được để trống")]
    public DateTime AppointmentTime { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    [Required(ErrorMessage = "Trạng thái không được để trống")]
    public AppointmentStatus Status { get; set; }
}
