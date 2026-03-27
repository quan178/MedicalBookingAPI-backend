using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Entities;
using BCrypt.Net;

namespace MedicalBookingAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasConversion<string>();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.PatientId);
            entity.Property(p => p.Gender).HasMaxLength(20);

            entity.HasOne(p => p.User)
                  .WithOne(u => u.Patient)
                  .HasForeignKey<Patient>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.UserId).IsUnique();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.DepartmentId);
            entity.Property(d => d.DepartmentName).IsRequired().HasMaxLength(100);
            entity.HasIndex(d => d.DepartmentName).IsUnique();
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.DoctorId);
            entity.Property(d => d.Qualification).HasMaxLength(200);

            entity.HasOne(d => d.User)
                  .WithOne(u => u.Doctor)
                  .HasForeignKey<Doctor>(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Department)
                  .WithMany(dept => dept.Doctors)
                  .HasForeignKey(d => d.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(d => d.UserId).IsUnique();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.AppointmentId);
            entity.Property(a => a.Status).IsRequired().HasConversion<string>();
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.DoctorId, a.AppointmentTime });
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(m => m.MedicalRecordId);
            entity.Property(m => m.DoctorDiagnosis).HasMaxLength(2000);
            entity.Property(m => m.Treatment).HasMaxLength(2000);
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(m => m.Appointment)
                  .WithOne(a => a.MedicalRecord)
                  .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => m.AppointmentId).IsUnique();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, DepartmentName = "Nội khoa", Description = "Internal Medicine Department" },
            new Department { DepartmentId = 2, DepartmentName = "Da liễu", Description = "Dermatology Department" },
            new Department { DepartmentId = 3, DepartmentName = "Tim mạch", Description = "Cardiology Department" }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                FullName = "Quản trị viên",
                Email = "admin@medical.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Phone = "1234567890",
                Role = UserRole.Admin,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 2,
                FullName = "Bác sĩ Trương",
                Email = "doctor@medical.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!"),
                Phone = "9876543210",
                Role = UserRole.Doctor,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 3,
                FullName = "Đỗ Minh Quân",
                Email = "patient@medical.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient123!"),
                Phone = "0123456789",
                Role = UserRole.Patient,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                DoctorId = 1,
                UserId = 2,
                DepartmentId = 1,
                Qualification = "Bác sĩ chuyên khoa"
            }
        );

        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                PatientId = 1,
                UserId = 3,
                DateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                Gender = "Male"
            }
        );
    }
}
