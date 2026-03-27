# MedicalBookingAI — Hướng Dẫn Chạy Dự Án

> Hệ thống API đặt lịch khám bệnh viện
> ASP.NET Core 8.0 + Entity Framework Core + SQL Server
> Cập nhật: 2026-03-25

---

## Mục lục

1. [Yêu cầu hệ thống](#1-yêu-cầu-hệ-thống)
2. [Cài đặt SQL Server](#2-cài-đặt-sql-server)
3. [Cấu hình cơ sở dữ liệu](#3-cấu-hình-cơ-sở-dữ-liệu)
4. [Chạy API](#4-chạy-api)
5. [Truy cập Swagger UI](#5-truy-cập-swagger-ui)
6. [Tài khoản mặc định](#6-tài-khoản-mặc-định)
7. [Cấu hình nâng cao](#7-cấu-hình-nâng-cao)
8. [Giải quyết sự cố](#8-giải-quyết-sự-cố)

---

## 1. Yêu cầu hệ thống

| Thành phần | Phiên bản tối thiểu | Ghi chú |
|------------|----------------------|---------|
| **.NET SDK** | 8.0+ | [Tải tại đây](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server** | 2016+ / LocalDB | [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) |
| **OS** | Windows 10/11 | Hỗ trợ Linux/macOS nếu dùng SQL Server container |

### Kiểm tra .NET SDK

```powershell
dotnet --version
```

Kết quả mong đợi: `8.0.x` hoặc cao hơn.

---

## 2. Cài đặt SQL Server

### 2.1 Sử dụng SQL Server Express (khuyến nghị)

1. Tải **SQL Server Express** tại: https://www.microsoft.com/sql-server/sql-server-downloads
2. Chọn phiên bản **Express** (miễn phí)
3. Trong quá trình cài đặt:
   - Chọn **Basic** hoặc **Custom**
   - Instance name: `SQLEXPRESS` hoặc `MSSQLSERVER` (default)
   - Bật **TCP/IP** protocol trong SQL Server Configuration Manager
   - Đặt cổng: `1433`

### 2.2 Sử dụng SQL Server LocalDB (nhanh gọn)

```powershell
# Kiểm tra LocalDB đã cài chưa
sqllocaldb info
```

Nếu chưa có, tải tại: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb

### 2.3 Bật TCP/IP cho SQL Server

1. Mở **SQL Server Configuration Manager**
2. Chọn **SQL Server Network Configuration** → **Protocols for SQLEXPRESS**
3. Bật **TCP/IP**
4. Chuột phải → **Properties** → Tab **IP Addresses**
5. Đặt **TCP Port** = `1433`
6. Restart SQL Server service

---

## 3. Cấu hình cơ sở dữ liệu

### 3.1 Kiểm tra connection string

Mở file `appsettings.json` trong thư mục `MedicalBookingAPI`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MedicalBookingDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### 3.2 Thay đổi connection string theo SQL Server của bạn

| Loại SQL Server | Connection String |
|-----------------|-------------------|
| SQL Server LocalDB | `Server=(localdb)\\mssqllocaldb;Database=MedicalBookingDB;Trusted_Connection=True;` |
| SQL Server Express | `Server=localhost\\SQLEXPRESS;Database=MedicalBookingDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server thường | `Server=localhost,1433;Database=MedicalBookingDB;User Id=sa;Password=yourpass;TrustServerCertificate=True;` |
| Azure SQL | `Server=tcp:youserver.database.windows.net;Database=MedicalBookingDB;User Id=user;Password=pass;` |

### 3.3 Tạo cơ sở dữ liệu bằng Migration

```powershell
# Di chuyển đến thư mục API
cd d:\KLTN\MedicalBookingAI\MedicalBookingAPI

# Áp dụng migration (tạo bảng và dữ liệu ban đầu)
dotnet ef database update
```

**Hoặc** nếu muốn xóa và tạo lại:

```powershell
# Xóa database cũ
dotnet ef database drop --force

# Tạo database mới từ migration
dotnet ef database update
```

---

## 4. Chạy API

### 4.1 Khôi phục gói và biên dịch

```powershell
cd d:\KLTN\MedicalBookingAI\MedicalBookingAPI
dotnet restore
dotnet build
```

### 4.2 Chạy ứng dụng

```powershell
dotnet run
```

Hoặc chạy với cổng cụ thể:

```powershell
dotnet run --urls "http://localhost:5000"
```

### 4.3 Các cách chạy khác

#### Cách 1: Qua Visual Studio 2022
1. Mở solution `MedicalBookingAPI.csproj` hoặc solution file
2. Chọn profile **http** hoặc **IIS Express**
3. Nhấn **F5** hoặc **Ctrl+F5**

#### Cách 2: Qua VS Code
1. Cài extension **C# Dev Kit**
2. Mở thư mục `MedicalBookingAPI`
3. Nhấn **F5** để debug

#### Cách 3: Qua dotnet script
```powershell
dotnet run --project d:\KLTN\MedicalBookingAI\MedicalBookingAPI\MedicalBookingAPI.csproj
```

---

## 5. Truy cập Swagger UI

Sau khi API khởi động thành công, truy cập:

| Giao diện | Địa chỉ |
|------------|---------|
| **Swagger UI** (khuyến nghị) | http://localhost:5220/swagger |
| **Swagger JSON** | http://localhost:5220/swagger/v1/swagger.json |
| **API Root** | http://localhost:5220 |

---

## 6. Tài khoản mặc định

Sau khi chạy `dotnet ef database update`, hệ thống sẽ tự động tạo các tài khoản sau:

| Vai trò | Username | Password | Mô tả |
|---------|----------|----------|-------|
| **Admin** | `admin` | `admin123` | Quản trị viên - truy cập toàn bộ API |
| **Doctor** | `doctor1` | `doctor123` | Bác sĩ - quản lý lịch hẹn, bệnh án |
| **Patient** | `patient1` | `patient123` | Bệnh nhân - đặt lịch khám |

> ⚠️ **Lưu ý bảo mật**: Đây là tài khoản mặc định cho môi trường phát triển. Hãy thay đổi mật khẩu hoặc xóa các tài khoản này trước khi triển khai production!

---

## 7. Cấu hình nâng cao

### 7.1 Thay đổi JWT Secret Key

Trong `appsettings.json`:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "MedicalBookingAPI",
  "Audience": "MedicalBookingClient",
  "ExpireDays": "7"
}
```

> Key phải có ít nhất **32 ký tự** để đảm bảo bảo mật.

### 7.2 Thay đổi thời gian hết hạn Token

```json
"ExpireDays": "7"    // Token hết hạn sau 7 ngày
```

### 7.3 Cấu hình CORS (cho Frontend)

Thêm domain frontend vào `AllowedOrigins` trong `appsettings.json`:

```json
"AllowedOrigins": [
  "http://localhost:3000",      # React
  "http://localhost:5173",      # Vite
  "http://localhost:4200",       # Angular
  "https://your-frontend.com"   # Production
]
```

### 7.4 Chạy trên cổng khác

Trong `appsettings.Development.json` hoặc `.csproj`:

```json
"urls": "http://localhost:5000;https://localhost:5001"
```

Hoặc qua command line:

```powershell
dotnet run --urls "http://0.0.0.0:5000"
```

---

## 8. Giải quyết sự cố

### Lỗi 1: Không kết nối được SQL Server

**Thông báo lỗi:**
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server
```

**Giải pháp:**
1. Kiểm tra SQL Server service đã chạy chưa:
   ```powershell
   # Mở Services
   services.msc
   # Tìm "SQL Server (SQLEXPRESS)" và đảm bảo Status = Running
   ```

2. Kiểm tra connection string đúng với SQL Server của bạn

3. Bật TCP/IP protocol:
   - Mở **SQL Server Configuration Manager**
   - **SQL Server Network Configuration** → **Protocols**
   - Bật **TCP/IP**

4. Tắt Windows Firewall cho cổng 1433 hoặc thêm rule cho SQL Server

---

### Lỗi 2: Migration thất bại

**Thông báo lỗi:**
```
Unable to create an object of type 'AppDbContext'
```

**Giải pháp:**
```powershell
# Xóa migration cũ (nếu có)
dotnet ef migrations remove

# Tạo migration mới
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

---

### Lỗi 3: Swagger không hiển thị

**Thông báo lỗi:**
```
Unable to load swagger page
```

**Giải pháp:**
1. Kiểm tra API đã chạy thành công chưa (cửa sổ console không có lỗi)
2. Truy cập trực tiếp: http://localhost:5220/swagger/index.html
3. Kiểm tra trong `Program.cs` đã có:
   ```csharp
   app.UseSwagger();
   app.UseSwaggerUI();
   ```

---

### Lỗi 4: CORS Error khi gọi từ Frontend

**Thông báo lỗi:**
```
Access to fetch at 'http://localhost:5220/api/...' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Giải pháp:**
1. Đảm bảo origin của frontend có trong `AllowedOrigins` trong `appsettings.json`
2. Ví dụ với React (Vite):
   ```json
   "AllowedOrigins": [
     "http://localhost:3000",
     "http://localhost:5173"
   ]
   ```

---

### Lỗi 5: Token hết hạn

**Thông báo lỗi:**
```
401 Unauthorized
```

**Giải pháp:**
- Đăng nhập lại để nhận token mới: `POST /api/Auth/login`
- Kiểm tra thời gian hết hạn trong `appsettings.json` (`ExpireDays`)

---

### Lỗi 6: Lỗi "Trusted Connection" khi dùng SQL Auth

**Giải pháp:** Đổi connection string sang SQL Authentication:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MedicalBookingDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

---

### Lỗi 7: Lỗi SSL Certificate

**Thông báo lỗi:**
```
The certificate chain was issued by an authority that is not trusted
```

**Giải pháp:** Thêm `TrustServerCertificate=True` vào connection string:

```
TrustServerCertificate=True
```

---

## Kiểm tra nhanh

Sau khi chạy API thành công, kiểm tra bằng curl:

```powershell
# Kiểm tra API có hoạt động không
curl http://localhost:5220/api/Departments

# Kết quả mong đợi: JSON response với danh sách departments
```

---

## Tiếp theo

Sau khi chạy thành công, xem thêm:

- 📋 **[TESTING.md](./TESTING.md)** — Hướng dẫn test đầy đủ API
- 🔗 **Swagger UI** — http://localhost:5220/swagger
- 💻 Kết nối với Frontend (React/Angular/Vue) qua các endpoint đã được cấu hình trong `AllowedOrigins`

---

*Tài liệu này được tạo tự động. Nếu có vấn đề gì, hãy kiểm tra phần [Giải quyết sự cố](#8-giải-quyết-sự-cố) hoặc liên hệ đội ngũ phát triển.*
