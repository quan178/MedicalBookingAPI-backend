namespace MedicalBookingAPI.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserDetailDto : UserDto
{
    public PatientDto? Patient { get; set; }
    public DoctorDto? Doctor { get; set; }
}

public class PatientDto
{
    public int PatientId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}

public class DoctorDto
{
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Qualification { get; set; }
}
