using Microsoft.EntityFrameworkCore;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Helpers;
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
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();

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
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
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
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETDATE()");

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
            entity.Property(m => m.Prescription).HasColumnType("nvarchar(max)");
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(m => m.IsEncrypted).HasDefaultValue(false);

            entity.HasOne(m => m.Appointment)
                  .WithOne(a => a.MedicalRecord)
                  .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => m.AppointmentId).IsUnique();
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(s => s.Patient)
                  .WithMany()
                  .HasForeignKey(s => s.PatientId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.Property(m => m.Content).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(m => m.Sender).IsRequired().HasConversion<string>();
            entity.Property(m => m.SuggestedSpecialty).HasMaxLength(100);
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(m => m.IsEncrypted).HasDefaultValue(false);

            entity.HasOne(m => m.ChatSession)
                  .WithMany(s => s.Messages)
                  .HasForeignKey(m => m.ChatSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(n => n.Type).IsRequired().HasConversion<string>();
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1,  DepartmentName = "Tim mạch",        Description = "Khoa Tim mạch - Chẩn đoán và điều trị các bệnh lý tim mạch" },
            new Department { DepartmentId = 2,  DepartmentName = "Thần kinh",       Description = "Khoa Thần kinh - Điều trị các bệnh lý thần kinh và não bộ" },
            new Department { DepartmentId = 3,  DepartmentName = "Tiêu hóa",         Description = "Khoa Tiêu hóa - Điều trị các bệnh lý đường tiêu hóa" },
            new Department { DepartmentId = 4,  DepartmentName = "Hô hấp",           Description = "Khoa Hô hấp - Điều trị các bệnh lý đường hô hấp" },
            new Department { DepartmentId = 5,  DepartmentName = "Tai mũi họng",     Description = "Khoa Tai mũi họng - Điều trị các bệnh lý tai, mũi, họng" },
            new Department { DepartmentId = 6,  DepartmentName = "Da liễu",           Description = "Khoa Da liễu - Điều trị các bệnh lý da và các bệnh lây truyền qua đường tình dục" },
            new Department { DepartmentId = 7,  DepartmentName = "Cơ xương khớp",     Description = "Khoa Cơ xương khớp - Điều trị các bệnh lý cơ, xương, khớp" },
            new Department { DepartmentId = 8,  DepartmentName = "Nội tổng quát",    Description = "Khoa Nội tổng quát - Khám và điều trị các bệnh nội khoa tổng hợp" },
            new Department { DepartmentId = 9,  DepartmentName = "Nhi khoa",          Description = "Khoa Nhi khoa - Chăm sóc sức khỏe trẻ em từ sơ sinh đến 15 tuổi" },
            new Department { DepartmentId = 10, DepartmentName = "Sản phụ khoa",      Description = "Khoa Sản phụ khoa - Chăm sóc sức khỏe phụ nữ và sinh sản" }
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
                CreatedAt = DateTimeHelper.Now
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
                CreatedAt = DateTimeHelper.Now
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
                CreatedAt = DateTimeHelper.Now
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
                DateOfBirth = DateTimeHelper.Now,
                Gender = "Male"
            }
        );
    }
}
