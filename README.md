# MedicalBookingAI

> Hệ thống API đặt lịch khám bệnh viện thông minh
>
> ASP.NET Core 8.0 + Entity Framework Core + SQL Server + JWT Authentication

---

## Mục lục

- [Giới thiệu](#giới-thiệu)
- [Tính năng chính](#tính-năng-chính)
- [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt nhanh](#cài-đặt-nhanh)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Tài khoản mặc định](#tài-khoản-mặc-định)
- [API Endpoints](#api-endpoints)
- [Tài liệu liên quan](#tài-liệu-liên-quan)

---

## Giới thiệu

**MedicalBookingAI** là một RESTful API được phát triển cho hệ thống đặt lịch khám bệnh viện. Hệ thống hỗ trợ:

- Quản lý người dùng với 3 vai trò: Admin, Doctor, Patient
- Quản lý khoa và bác sĩ
- Đặt lịch hẹn khám bệnh
- Ghi nhận và quản lý hồ sơ bệnh án
- Xác thực và phân quyền bằng JWT

---

## Tính năng chính

| Tính năng | Mô tả |
|-----------|-------|
| **Xác thực & Ủy quyền** | JWT Token-based authentication với Role-based access control |
| **Quản lý Khoa** | CRUD đầy đủ cho các khoa khám bệnh |
| **Quản lý Bác sĩ** | Tạo, cập nhật, phân công bác sĩ vào khoa |
| **Quản lý Người dùng** | Quản lý tài khoản Admin, Doctor, Patient |
| **Đặt lịch hẹn** | Bệnh nhân đặt lịch, bác sĩ xác nhận/hủy lịch hẹn |
| **Hồ sơ bệnh án** | Bác sĩ tạo và quản lý hồ sơ bệnh án |
| **Swagger UI** | Tài liệu API tự động với giao diện trực quan |

---

## Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────────┐
│                     Client (Frontend)                       │
│              React / Angular / Vue / Mobile                  │
└─────────────────────────┬───────────────────────────────────┘
                          │ HTTP/REST + JWT
┌─────────────────────────▼───────────────────────────────────┐
│                     MedicalBookingAPI                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ Controllers │  │ Middleware  │  │   Swagger/OpenAPI   │  │
│  └──────┬──────┘  └─────────────┘  └─────────────────────┘  │
│         │                                                    │
│  ┌──────▼──────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  Services   │  │   Helpers   │  │    DTOs/Models     │  │
│  └──────┬──────┘  └─────────────┘  └─────────────────────┘  │
│         │                                                    │
│  ┌──────▼──────┐                                             │
│  │ Repositories│  ← Data Access Layer                        │
│  └──────┬──────┘                                             │
└─────────┼───────────────────────────────────────────────────┘
          │
┌─────────▼───────────────────────────────────────────────────┐
│                      SQL Server                              │
│                   (Entity Framework Core)                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Công nghệ sử dụng

| Layer | Technology |
|-------|------------|
| **Runtime** | .NET 8.0 |
| **Framework** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | SQL Server 2016+ |
| **Authentication** | JWT (JSON Web Tokens) |
| **API Documentation** | Swagger / OpenAPI |
| **Architecture** | Repository Pattern |

---

## Yêu cầu hệ thống

| Component | Version | Notes |
|-----------|---------|-------|
| **.NET SDK** | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server** | 2016+ hoặc LocalDB | [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) |
| **OS** | Windows 10/11 | Hỗ trợ Linux/macOS với container |

---

## Cài đặt nhanh

### 1. Clone và cài đặt dependencies

```powershell
# Di chuyển đến thư mục API
cd d:\KLTN\MedicalBookingAI\MedicalBookingAPI

# Khôi phục packages
dotnet restore

# Build project
dotnet build
```

### 2. Cấu hình Database

Kiểm tra `appsettings.json` và đảm bảo connection string phù hợp:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MedicalBookingDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### 3. Chạy Migration

```powershell
# Tạo database và seed data
dotnet ef database update
```

### 4. Chạy API

```powershell
dotnet run
```

### 5. Truy cập Swagger UI

```
http://localhost:5220/swagger
```

---

## Cấu trúc dự án

```
MedicalBookingAI/
├── MedicalBookingAPI/
│   ├── Configuration/         # Cấu hình ứng dụng
│   ├── Controllers/           # API Controllers
│   │   ├── AuthController.cs
│   │   ├── DepartmentsController.cs
│   │   ├── DoctorsController.cs
│   │   ├── UsersController.cs
│   │   ├── AppointmentsController.cs
│   │   └── MedicalRecordsController.cs
│   ├── Data/                  # DbContext và cấu hình
│   │   └── AppDbContext.cs
│   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Entities/              # Entity Models
│   │   ├── User.cs
│   │   ├── Department.cs
│   │   ├── Doctor.cs
│   │   ├── Patient.cs
│   │   ├── Appointment.cs
│   │   └── MedicalRecord.cs
│   ├── Helpers/                # Helper classes
│   │   └── ApiResponse.cs
│   ├── Middleware/             # Custom Middleware
│   ├── Migrations/            # EF Core Migrations
│   ├── Repositories/          # Repository Pattern
│   ├── Services/              # Business Logic Services
│   ├── Program.cs             # Entry point
│   └── appsettings.json       # Configuration
├── RUNNING.md                  # Hướng dẫn chạy chi tiết
├── TESTING.md                  # Tài liệu test API
└── README.md                   # (file này)
```

---

## Tài khoản mặc định

Sau khi chạy `dotnet ef database update`, hệ thống sẽ tự động tạo các tài khoản test:

| Vai trò | Username | Password | Quyền hạn |
|---------|----------|----------|------------|
| **Admin** | `admin` | `admin123` | Toàn quyền quản trị |
| **Doctor** | `doctor1` | `doctor123` | Quản lý lịch hẹn, hồ sơ bệnh án |
| **Patient** | `patient1` | `patient123` | Đặt lịch khám, xem lịch sử |

> **Lưu ý bảo mật**: Đây là tài khoản cho môi trường phát triển. Hãy thay đổi mật khẩu trước khi triển khai production!

---

## API Endpoints

### Tổng quan

| Module | Endpoints | Auth |
|--------|-----------|------|
| **Auth** | Login, Register | Public |
| **Departments** | CRUD | Read: Public, Write: Admin |
| **Doctors** | CRUD + Assign | Read: Public, Write: Admin |
| **Users** | CRUD + Profile | Authenticated |
| **Appointments** | CRUD + Status | Role-based |
| **MedicalRecords** | CRUD | Role-based |

### Chi tiết đầy đủ

Xem [TESTING.md](./TESTING.md) để có danh sách đầy đủ các endpoints với request/response examples.

### Cách lấy JWT Token

```bash
# Đăng nhập
curl -X POST http://localhost:5220/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin","password":"admin123"}'

# Response sẽ chứa token trong field "data.token"
```

### Sử dụng Token

Thêm header cho các request cần xác thực:

```
Authorization: Bearer <token>
```

---

## Tài liệu liên quan

| Document | Mô tả |
|----------|-------|
| [RUNNING.md](./RUNNING.md) | Hướng dẫn cài đặt và chạy chi tiết |
| [TESTING.md](./TESTING.md) | Tài liệu test API đầy đủ |
| [Swagger UI](http://localhost:5220/swagger) | Tài liệu API trực quan (khi API đang chạy) |

---

## Cấu hình JWT

Trong `appsettings.json`:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "MedicalBookingAPI",
  "Audience": "MedicalBookingClient",
  "ExpireDays": "7"
}
```

## CORS Configuration

Thêm origin của frontend vào `AllowedOrigins`:

```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:5173",
  "http://localhost:4200"
]
```

---

## License

MIT License

---

*Document được tạo cho dự án MedicalBookingAI - Khóa luận tốt nghiệp*
