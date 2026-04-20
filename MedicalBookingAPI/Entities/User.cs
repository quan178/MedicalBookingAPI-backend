using System.Text.Json.Serialization;
using MedicalBookingAPI.Helpers;

namespace MedicalBookingAPI.Entities;

public enum UserRole
{
    Admin,
    Doctor,
    Patient
}

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now;

    // Navigation properties
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}
