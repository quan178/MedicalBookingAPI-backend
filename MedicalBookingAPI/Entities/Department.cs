namespace MedicalBookingAPI.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
