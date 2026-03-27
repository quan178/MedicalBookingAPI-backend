using System.ComponentModel.DataAnnotations;

namespace MedicalBookingAPI.DTOs;

public class DoctorDetailDto
{
    public int DoctorId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Qualification { get; set; }
}

public class CreateDoctorRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải có độ dài từ 2-100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "ID khoa không được để trống")]
    public int DepartmentId { get; set; }

    public string? Qualification { get; set; }
}

public class AssignDepartmentRequest
{
    [Required(ErrorMessage = "ID khoa không được để trống")]
    public int DepartmentId { get; set; }
}

public class UpdateDoctorRequest
{
    public string? Qualification { get; set; }
    public string? Phone { get; set; }
}
