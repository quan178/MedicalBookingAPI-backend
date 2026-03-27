using System.ComponentModel.DataAnnotations;

namespace MedicalBookingAPI.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateDepartmentRequest
{
    [Required(ErrorMessage = "Tên khoa không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên khoa phải có độ dài từ 2-100 ký tự")]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string? Description { get; set; }
}

public class UpdateDepartmentRequest
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên khoa phải có độ dài từ 2-100 ký tự")]
    public string? DepartmentName { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string? Description { get; set; }
}
