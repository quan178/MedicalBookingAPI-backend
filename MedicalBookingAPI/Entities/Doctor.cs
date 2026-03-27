namespace MedicalBookingAPI.Entities;

public class Doctor
{
    public int DoctorId { get; set; }
    public int UserId { get; set; }
    public int DepartmentId { get; set; }
    public string? Qualification { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
