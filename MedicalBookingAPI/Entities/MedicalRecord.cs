namespace MedicalBookingAPI.Entities;

public class MedicalRecord
{
    public int MedicalRecordId { get; set; }
    public int AppointmentId { get; set; }
    public string? DoctorDiagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Appointment Appointment { get; set; } = null!;
}
