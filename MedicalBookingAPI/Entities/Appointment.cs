namespace MedicalBookingAPI.Entities;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
}

public class Appointment
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public MedicalRecord? MedicalRecord { get; set; }
}
