using System.ComponentModel.DataAnnotations;

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

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Qualification { get; set; }
}

public class CreateUserRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "�ịnh dạng email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
}
