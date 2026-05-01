# MedicalBookingAI — Hướng Dẫn Kiểm Thử API

> Bộ tài liệu test đầy đủ cho hệ thống API đặt lịch khám bệnh viện
> ASP.NET Core 8.0 + Entity Framework Core + SQL Server
> Base URL: `http://localhost:5220`
> Cập nhật: 2026-04-30

---

## Mục lục

1. [Cấu trúc Response & Mã lỗi](#1-cấu-trúc-response--mã-lỗi)
2. [Tài khoản mặc định](#2-tài-khoản-mặc-định)
3. [Cách lấy JWT Token](#3-cách-lấy-jwt-token)
4. [API Xác thực — Auth](#4-api-xác-thực--auth)
5. [API Khoa — Departments](#5-api-khoa--departments)
6. [API Bác sĩ — Doctors](#6-api-bác-sĩ--doctors)
7. [API Người dùng — Users](#7-api-người-dùng--users)
8. [API Lịch hẹn — Appointments](#8-api-lịch-hẹn--appointments)
9. [API Hồ sơ bệnh án — Medical Records](#10-api-hồ-sơ-bệnh-án--medical-records)
10. [API AI Chat — AI Chat](#11-api-ai-chat--ai-chat)
11. [API Thông báo — Notifications](#13-api-thông-báo--notifications)
12. [Danh sách HTTP Status Codes](#14-danh-sách-http-status-codes)

---

## 1. Cấu trúc Response & Mã lỗi

Tất cả API trả về response theo định dạng `ApiResponse<T>`:

### Thành công

```json
{
  "success": true,
  "message": "Mô tả thao tác",
  "data": { ... }
}
```

### Thất bại

```json
{
  "success": false,
  "message": "Mô tả lỗi",
  "data": null
}
```

---

## 2. Tài khoản mặc định

| Vai trò | Username / Email | Password | Quyền |
|---------|-----------------|----------|-------|
| **Admin** | `admin` | `admin123` | Toàn quyền |
| **Doctor** | `doctor1` | `doctor123` | Quản lý lịch hẹn, hồ sơ bệnh án |
| **Patient** | `patient1` | `patient123` | Đặt lịch khám, xem lịch sử |

> **Lưu ý**: Token JWT có thời hạn theo cấu hình `ExpireDays` trong `appsettings.json` (mặc định 7 ngày).

---

## 3. Cách lấy JWT Token

Gọi `POST /api/Auth/login` với email và password. Token nằm trong `data.token` của response.

### Cấu hình Authorization Header

Sau khi có token, thêm header cho các request cần xác thực:

```
Authorization: Bearer <token>
```

### Quyền theo vai trò

| Vai trò | Ký hiệu |
|---------|----------|
| Admin | `[Authorize(Roles = "Admin")]` |
| Doctor | `[Authorize(Roles = "Doctor")]` |
| Patient | `[Authorize(Roles = "Patient")]` |
| Tất cả đã đăng nhập | `[Authorize]` |
| Không cần đăng nhập | `[AllowAnonymous]` |

---

## 4. API Xác thực — Auth

> **Base**: `/api/Auth`
> **Auth**: Không yêu cầu

---

### 4.1 Đăng ký tài khoản

**POST** `/api/Auth/register`

#### Request Body

```json
{
  "fullName": "Nguyễn Văn A",
  "email": "nguyenvana@example.com",
  "password": "password123",
  "phone": "0901234567",
  "role": "Patient",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "departmentId": 1,
  "qualification": "Thạc sĩ"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `fullName` | string | ✅ | Họ tên (2-100 ký tự) |
| `email` | string | ✅ | Email hợp lệ, duy nhất |
| `password` | string | ✅ | Tối thiểu 6 ký tự |
| `phone` | string | ❌ | Số điện thoại |
| `role` | string | ✅ | `Patient`, `Doctor`, `Admin` |
| `dateOfBirth` | datetime | ❌ | Ngày sinh (format: `YYYY-MM-DD`) |
| `gender` | string | ❌ | Giới tính |
| `departmentId` | int | ❌ | ID khoa (dành cho Doctor) |
| `qualification` | string | ❌ | Bằng cấp chuyên môn (dành cho Doctor) |

#### Response 200

```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "nguyenvana@example.com",
    "fullName": "Nguyễn Văn A",
    "role": "Patient",
    "userId": 4
  }
}
```

#### Response 400

```json
{
  "success": false,
  "message": "Đăng ký thất bại, email có thể đã tồn tại hoặc vai trò không hợp lệ",
  "data": null
}
```

---

### 4.2 Đăng nhập

**POST** `/api/Auth/login`

#### Request Body

```json
{
  "email": "admin",
  "password": "admin123"
}
```

#### Response 200

```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "admin",
    "fullName": "Admin User",
    "role": "Admin",
    "userId": 1
  }
}
```

#### Response 401

```json
{
  "success": false,
  "message": "Email hoặc mật khẩu không đúng",
  "data": null
}
```

---

### 4.3 Đổi mật khẩu

**PUT** `/api/Auth/change-password`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Request Body

```json
{
  "oldPassword": "admin123",
  "newPassword": "newpassword123"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `oldPassword` | string | ✅ | Mật khẩu cũ |
| `newPassword` | string | ✅ | Mật khẩu mới (tối thiểu 6 ký tự) |

#### Response 200

```json
{
  "success": true,
  "message": "Đổi mật khẩu thành công",
  "data": true
}
```

#### Response 400 — Mật khẩu cũ không đúng

```json
{
  "success": false,
  "message": "Mật khẩu cũ không đúng hoặc người dùng không tồn tại",
  "data": null
}
```

---

## 5. API Khoa — Departments

> **Base**: `/api/Departments`
> **Auth**: Đọc công khai, Ghi yêu cầu Admin

---

### 5.1 Lấy danh sách tất cả khoa

**GET** `/api/Departments`

#### Auth

`[AllowAnonymous]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "departmentId": 1,
      "departmentName": "Khoa Nội tổng hợp",
      "description": "Khám và điều trị các bệnh nội khoa"
    },
    {
      "departmentId": 2,
      "departmentName": "Khoa Ngoại tổng hợp",
      "description": "Phẫu thuật và điều trị ngoại khoa"
    }
  ]
}
```

---

### 5.2 Lấy thông tin khoa theo ID

**GET** `/api/Departments/{id}`

#### Auth

`[AllowAnonymous]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID khoa |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "departmentId": 1,
    "departmentName": "Khoa Nội tổng hợp",
    "description": "Khám và điều trị các bệnh nội khoa"
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Khoa không tồn tại",
  "data": null
}
```

---

### 5.3 Tạo khoa mới

**POST** `/api/Departments`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Request Body

```json
{
  "departmentName": "Khoa Tim mạch",
  "description": "Khám và điều trị các bệnh về tim mạch"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `departmentName` | string | ✅ | Tên khoa (2-100 ký tự) |
| `description` | string | ❌ | Mô tả (tối đa 500 ký tự) |

#### Response 200

```json
{
  "success": true,
  "message": "Tạo khoa thành công",
  "data": {
    "departmentId": 5,
    "departmentName": "Khoa Tim mạch",
    "description": "Khám và điều trị các bệnh về tim mạch"
  }
}
```

---

### 5.4 Cập nhật khoa

**PUT** `/api/Departments/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID khoa |

#### Request Body

```json
{
  "departmentName": "Khoa Tim mạch (Cập nhật)",
  "description": "Mô tả mới"
}
```

#### Response 200

```json
{
  "success": true,
  "message": "Cập nhật khoa thành công",
  "data": {
    "departmentId": 5,
    "departmentName": "Khoa Tim mạch (Cập nhật)",
    "description": "Mô tả mới"
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Khoa không tồn tại",
  "data": null
}
```

---

### 5.5 Xóa khoa

**DELETE** `/api/Departments/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID khoa |

#### Response 200

```json
{
  "success": true,
  "message": "Xóa khoa thành công"
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Khoa không tồn tại",
  "data": null
}
```

---

## 6. API Bác sĩ — Doctors

> **Base**: `/api/Doctors`
> **Auth**: Đọc công khai, Ghi yêu cầu Admin

---

### 6.1 Lấy danh sách tất cả bác sĩ

**GET** `/api/Doctors`

#### Auth

`[AllowAnonymous]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "doctorId": 1,
      "userId": 2,
      "fullName": "Dr. John Smith",
      "email": "doctor1",
      "phone": "0901234567",
      "departmentId": 1,
      "departmentName": "Khoa Nội tổng hợp",
      "qualification": "Thạc sĩ"
    }
  ]
}
```

---

### 6.2 Lấy thông tin bác sĩ theo ID

**GET** `/api/Doctors/{id}`

#### Auth

`[AllowAnonymous]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID bác sĩ |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "doctorId": 1,
    "userId": 2,
    "fullName": "Dr. John Smith",
    "email": "doctor1",
    "phone": "0901234567",
    "departmentId": 1,
    "departmentName": "Khoa Nội tổng hợp",
    "qualification": "Thạc sĩ"
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Bác sĩ không tồn tại",
  "data": null
}
```

---

### 6.3 Lấy danh sách bác sĩ theo khoa

**GET** `/api/Doctors/department/{departmentId}`

#### Auth

`[AllowAnonymous]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `departmentId` | int | ID khoa |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "doctorId": 1,
      "userId": 2,
      "fullName": "Dr. John Smith",
      "email": "doctor1",
      "phone": "0901234567",
      "departmentId": 1,
      "departmentName": "Khoa Nội tổng hợp",
      "qualification": "Thạc sĩ"
    }
  ]
}
```

---

### 6.4 Tạo bác sĩ mới

**POST** `/api/Doctors`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Request Body

```json
{
  "fullName": "Dr. Jane Doe",
  "email": "doctor2",
  "password": "doctor123",
  "phone": "0909876543",
  "departmentId": 2,
  "qualification": "Tiến sĩ"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `fullName` | string | ✅ | Họ tên (2-100 ký tự) |
| `email` | string | ✅ | Email hợp lệ, duy nhất |
| `password` | string | ✅ | Tối thiểu 6 ký tự |
| `phone` | string | ❌ | Số điện thoại |
| `departmentId` | int | ✅ | ID khoa |
| `qualification` | string | ❌ | Bằng cấp chuyên môn |

#### Response 200

```json
{
  "success": true,
  "message": "Tạo bác sĩ thành công",
  "data": {
    "doctorId": 3,
    "userId": 5,
    "fullName": "Dr. Jane Doe",
    "email": "doctor2",
    "phone": "0909876543",
    "departmentId": 2,
    "departmentName": "Khoa Ngoại tổng hợp",
    "qualification": "Tiến sĩ"
  }
}
```

#### Response 400

```json
{
  "success": false,
  "message": "Email đã tồn tại hoặc khoa không hợp lệ",
  "data": null
}
```

---

### 6.5 Cập nhật thông tin bác sĩ

**PUT** `/api/Doctors/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID bác sĩ |

#### Request Body

```json
{
  "qualification": "Giáo sư",
  "phone": "0912345678"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `qualification` | string | ❌ | Bằng cấp chuyên môn mới |
| `phone` | string | ❌ | Số điện thoại mới |

#### Response 200

```json
{
  "success": true,
  "message": "Cập nhật thông tin bác sĩ thành công",
  "data": {
    "doctorId": 1,
    "userId": 2,
    "fullName": "Dr. John Smith",
    "email": "doctor1",
    "phone": "0912345678",
    "departmentId": 1,
    "departmentName": "Khoa Nội tổng hợp",
    "qualification": "Giáo sư"
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Bác sĩ không tồn tại",
  "data": null
}
```

---

### 6.6 Phân công bác sĩ vào khoa

**PUT** `/api/Doctors/{id}/department`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID bác sĩ |

#### Request Body

```json
{
  "departmentId": 3
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `departmentId` | int | ✅ | ID khoa muốn phân công |

#### Response 200

```json
{
  "success": true,
  "message": "Phân công khoa thành công"
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Bác sĩ hoặc khoa không tồn tại",
  "data": null
}
```

---

## 7. API Người dùng — Users

> **Base**: `/api/Users`
> **Auth**: Yêu cầu đăng nhập

---

### 7.1 Lấy thông tin người dùng hiện tại

**GET** `/api/Users/me`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "userId": 3,
    "fullName": "Patient One",
    "email": "patient1",
    "phone": "0901234567",
    "role": "Patient",
    "createdAt": "2026-03-25T13:38:01",
    "patient": {
      "patientId": 1,
      "dateOfBirth": "1985-05-20T00:00:00",
      "gender": "Male"
    },
    "doctor": null
  }
}
```

#### Response 401

```json
{
  "success": false,
  "message": "Thông tin người dùng không hợp lệ",
  "data": null
}
```

---

### 7.2 Cập nhật thông tin người dùng hiện tại

**PUT** `/api/Users/me`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Request Body

```json
{
  "fullName": "Nguyễn Văn B",
  "phone": "0912345678",
  "dateOfBirth": "1990-05-20",
  "gender": "Male",
  "qualification": "Giáo sư"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `fullName` | string | ❌ | Họ tên mới |
| `phone` | string | ❌ | Số điện thoại mới |
| `dateOfBirth` | datetime | ❌ | Ngày sinh mới (format: `YYYY-MM-DD`) |
| `gender` | string | ❌ | Giới tính mới |
| `qualification` | string | ❌ | Bằng cấp (chỉ áp dụng cho Doctor) |

#### Response 200

```json
{
  "success": true,
  "message": "Cập nhật hồ sơ thành công",
  "data": {
    "userId": 3,
    "fullName": "Nguyễn Văn B",
    "email": "patient1",
    "phone": "0912345678",
    "role": "Patient",
    "createdAt": "2026-03-25T13:38:01",
    "patient": {
      "patientId": 1,
      "dateOfBirth": "1990-05-20T00:00:00",
      "gender": "Male"
    },
    "doctor": null
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Người dùng không tồn tại",
  "data": null
}
```

---

### 7.3 Lấy danh sách tất cả người dùng

**GET** `/api/Users`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "userId": 1,
      "fullName": "Admin User",
      "email": "admin",
      "phone": null,
      "role": "Admin",
      "createdAt": "2026-03-25T13:38:01"
    },
    {
      "userId": 2,
      "fullName": "Dr. John Smith",
      "email": "doctor1",
      "phone": "0901234567",
      "role": "Doctor",
      "createdAt": "2026-03-25T13:38:01"
    }
  ]
}
```

---

### 7.4 Lấy thông tin người dùng theo ID

**GET** `/api/Users/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID người dùng |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "userId": 3,
    "fullName": "Patient One",
    "email": "patient1",
    "phone": "0901234567",
    "role": "Patient",
    "createdAt": "2026-03-25T13:38:01",
    "patient": {
      "patientId": 1,
      "dateOfBirth": "1985-05-20T00:00:00",
      "gender": "Male"
    },
    "doctor": null
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Người dùng không tồn tại",
  "data": null
}
```

---

### 7.5 Tạo tài khoản Admin

**POST** `/api/Users`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Request Body

```json
{
  "email": "admin2",
  "password": "admin123",
  "fullName": "Admin Two",
  "phone": "0901234567"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `email` | string | ✅ | Email hợp lệ, duy nhất |
| `password` | string | ✅ | Tối thiểu 6 ký tự |
| `fullName` | string | ✅ | Họ tên (2-100 ký tự) |
| `phone` | string | ❌ | Số điện thoại |

#### Response 200

```json
{
  "success": true,
  "message": "Tạo tài khoản Admin thành công",
  "data": {
    "userId": 10,
    "fullName": "Admin Two",
    "email": "admin2",
    "phone": "0901234567",
    "role": "Admin",
    "createdAt": "2026-04-30T10:00:00"
  }
}
```

#### Response 400

```json
{
  "success": false,
  "message": "Email đã tồn tại",
  "data": null
}
```

---

### 7.6 Xóa người dùng

**DELETE** `/api/Users/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID người dùng |

#### Response 200

```json
{
  "success": true,
  "message": "Xóa người dùng thành công"
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Người dùng không tồn tại",
  "data": null
}
```

---

## 8. API Lịch hẹn — Appointments

> **Base**: `/api/Appointments`
> **Auth**: Yêu cầu đăng nhập (Patient, Doctor)

---

### Trạng thái lịch hẹn

| Giá trị | Mô tả |
|---------|-------|
| `Pending` | Chờ xác nhận |
| `Confirmed` | Đã xác nhận |
| `Completed` | Đã hoàn thành |
| `Cancelled` | Đã hủy |

---

### 8.1 Lấy lịch hẹn của bệnh nhân hiện tại

**GET** `/api/Appointments/patient`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "appointmentId": 1,
      "patientId": 1,
      "patientName": "Patient One",
      "doctorId": 1,
      "doctorName": "Dr. John Smith",
      "departmentName": "Khoa Nội tổng hợp",
      "appointmentTime": "2026-03-26T09:00:00",
      "status": "Pending",
      "createdAt": "2026-03-25T14:00:00"
    }
  ]
}
```

---

### 8.2 Lấy lịch hẹn của bác sĩ hiện tại

**GET** `/api/Appointments/doctor`

#### Auth

`[Authorize(Roles = "Doctor")]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "appointmentId": 1,
      "patientId": 1,
      "patientName": "Patient One",
      "doctorId": 1,
      "doctorName": "Dr. John Smith",
      "departmentName": "Khoa Nội tổng hợp",
      "appointmentTime": "2026-03-26T09:00:00",
      "status": "Pending",
      "createdAt": "2026-03-25T14:00:00"
    }
  ]
}
```

---

### 8.3 Lấy lịch hẹn của bác sĩ theo ngày

**GET** `/api/Appointments/doctor/schedule?date={date}`

#### Auth

`[Authorize(Roles = "Doctor")]`

#### Query Parameters

| Tham số | Kiểu | Bắt buộc | Mô tả |
|---------|------|---------|-------|
| `date` | datetime | ✅ | Ngày cần xem (format: `YYYY-MM-DD`) |

#### Ví dụ

```
GET /api/Appointments/doctor/schedule?date=2026-03-26
```

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "appointmentId": 1,
      "patientId": 1,
      "patientName": "Patient One",
      "doctorId": 1,
      "doctorName": "Dr. John Smith",
      "departmentName": "Khoa Nội tổng hợp",
      "appointmentTime": "2026-03-26T09:00:00",
      "status": "Pending",
      "createdAt": "2026-03-25T14:00:00"
    }
  ]
}
```

---

### 8.4 Lấy thông tin lịch hẹn theo ID

**GET** `/api/Appointments/{id}`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID lịch hẹn |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "appointmentId": 1,
    "patientId": 1,
    "patientName": "Patient One",
    "doctorId": 1,
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "appointmentTime": "2026-03-26T09:00:00",
    "status": "Pending",
    "createdAt": "2026-03-25T14:00:00"
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Lịch hẹn không tồn tại",
  "data": null
}
```

---

### 8.5 Tạo lịch hẹn mới

**POST** `/api/Appointments`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Request Body

```json
{
  "doctorId": 1,
  "appointmentTime": "2026-03-26T09:00:00"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `doctorId` | int | ✅ | ID bác sĩ |
| `appointmentTime` | datetime | ✅ | Thời gian hẹn (phải là thời gian tương lai) |

#### Response 200

```json
{
  "success": true,
  "message": "Tạo lịch hẹn thành công",
  "data": {
    "appointmentId": 5,
    "patientId": 1,
    "patientName": "Patient One",
    "doctorId": 1,
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "appointmentTime": "2026-03-26T09:00:00",
    "status": "Pending",
    "createdAt": "2026-03-26T08:00:00"
  }
}
```

#### Response 400 — Lịch hẹn trùng hoặc không hợp lệ

```json
{
  "success": false,
  "message": "Bác sĩ đã có lịch hẹn vào thời gian này",
  "data": null
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Thông tin bệnh nhân không tồn tại",
  "data": null
}
```

---

### 8.6 Cập nhật trạng thái lịch hẹn

**PUT** `/api/Appointments/{id}/status`

#### Auth

`[Authorize(Roles = "Doctor")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID lịch hẹn |

#### Request Body

```json
{
  "status": "Confirmed"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `status` | AppointmentStatus | ✅ | `Pending`, `Confirmed`, `Completed`, `Cancelled` |

#### Response 200

```json
{
  "success": true,
  "message": "Cập nhật trạng thái lịch hẹn thành công",
  "data": {
    "appointmentId": 1,
    "patientId": 1,
    "patientName": "Patient One",
    "doctorId": 1,
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "appointmentTime": "2026-03-26T09:00:00",
    "status": "Confirmed",
    "createdAt": "2026-03-25T14:00:00"
  }
}
```

#### Response 403 — Bác sĩ không sở hữu lịch hẹn

```json
{
  "success": false,
  "message": "Forbidden",
  "data": null
}
```

---

### 8.7 Hủy lịch hẹn

**DELETE** `/api/Appointments/{id}`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID lịch hẹn |

#### Response 200

```json
{
  "success": true,
  "message": "Hủy lịch hẹn thành công"
}
```

#### Response 400 — Lịch hẹn không thể hủy

```json
{
  "success": false,
  "message": "Không thể hủy lịch hẹn đã hoàn thành",
  "data": null
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Lịch hẹn không tồn tại hoặc không có quyền hủy",
  "data": null
}
```

---

### 8.8 Lấy tất cả lịch hẹn (Admin)

**GET** `/api/Appointments/admin/all`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Query Parameters (tất cả optional)

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `fromDate` | datetime | Lọc từ ngày (format: `YYYY-MM-DD`) |
| `toDate` | datetime | Lọc đến ngày (format: `YYYY-MM-DD`) |
| `status` | string | Lọc theo trạng thái (`Pending`, `Confirmed`, `Completed`, `Cancelled`) |
| `doctorId` | int | Lọc theo ID bác sĩ |
| `patientId` | int | Lọc theo ID bệnh nhân |

#### Ví dụ

```
GET /api/Appointments/admin/all?status=Pending&fromDate=2026-04-01
```

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "appointmentId": 1,
      "patientId": 1,
      "patientName": "Patient One",
      "patientEmail": "patient1",
      "doctorId": 1,
      "doctorName": "Dr. John Smith",
      "departmentName": "Khoa Nội tổng hợp",
      "appointmentTime": "2026-04-15T09:00:00",
      "status": "Pending",
      "createdAt": "2026-04-10T14:00:00",
      "hasMedicalRecord": false
    }
  ]
}
```

---

### 8.9 Lấy chi tiết lịch hẹn (Admin)

**GET** `/api/Appointments/admin/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID lịch hẹn |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "appointmentId": 1,
    "patientId": 1,
    "patientName": "Patient One",
    "patientEmail": "patient1",
    "doctorId": 1,
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "appointmentTime": "2026-04-15T09:00:00",
    "status": "Confirmed",
    "createdAt": "2026-04-10T14:00:00",
    "hasMedicalRecord": true
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Lịch hẹn không tồn tại",
  "data": null
}
```

---

### 8.10 Cập nhật trạng thái lịch hẹn (Admin)

**PUT** `/api/Appointments/admin/{id}/status`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID lịch hẹn |

#### Request Body

```json
{
  "status": "Completed"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `status` | AppointmentStatus | ✅ | `Pending`, `Confirmed`, `Completed`, `Cancelled` |

#### Response 200

```json
{
  "success": true,
  "message": "Cập nhật trạng thái thành công",
  "data": {
    "appointmentId": 1,
    "patientId": 1,
    "patientName": "Patient One",
    "patientEmail": "patient1",
    "doctorId": 1,
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "appointmentTime": "2026-04-15T09:00:00",
    "status": "Completed",
    "createdAt": "2026-04-10T14:00:00",
    "hasMedicalRecord": true
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Lịch hẹn không tồn tại",
  "data": null
}
```

---

### 8.11 Hủy lịch hẹn (Admin)

**DELETE** `/api/Appointments/admin/{id}`

#### Auth

`[Authorize(Roles = "Admin")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID lịch hẹn |

#### Response 200

```json
{
  "success": true,
  "message": "Hủy lịch hẹn thành công",
  "data": null
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Lịch hẹn không tồn tại",
  "data": null
}
```

---

## 10. API Hồ sơ bệnh án — Medical Records

> **Base**: `/api/MedicalRecords`
> **Auth**: Yêu cầu đăng nhập (Patient, Doctor)

---

### 9.1 Lấy hồ sơ bệnh án của bệnh nhân hiện tại

**GET** `/api/MedicalRecords/patient`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "medicalRecordId": 1,
      "appointmentId": 1,
      "appointmentTime": "2026-03-26T09:00:00",
      "patientName": "Patient One",
      "doctorName": "Dr. John Smith",
      "departmentName": "Khoa Nội tổng hợp",
      "doctorDiagnosis": "Cảm cúm nhẹ",
      "treatment": "Nghỉ ngơi, uống nhiều nước, uống thuốc hạ sốt",
      "prescription": "Paracetamol 500mg x 2 viên/ngày, uống sau ăn",
      "createdAt": "2026-03-26T10:00:00"
    }
  ]
}
```

---

### 9.2 Lấy hồ sơ bệnh án của bác sĩ hiện tại

**GET** `/api/MedicalRecords/doctor`

#### Auth

`[Authorize(Roles = "Doctor")]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "medicalRecordId": 1,
      "appointmentId": 1,
      "appointmentTime": "2026-03-26T09:00:00",
      "patientName": "Patient One",
      "doctorName": "Dr. John Smith",
      "departmentName": "Khoa Nội tổng hợp",
      "doctorDiagnosis": "Cảm cúm nhẹ",
      "treatment": "Nghỉ ngơi, uống nhiều nước, uống thuốc hạ sốt",
      "prescription": "Paracetamol 500mg x 2 viên/ngày, uống sau ăn",
      "createdAt": "2026-03-26T10:00:00"
    }
  ]
}
```

---

### 9.3 Lấy hồ sơ bệnh án theo ID

**GET** `/api/MedicalRecords/{id}`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID hồ sơ bệnh án |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "medicalRecordId": 1,
    "appointmentId": 1,
    "appointmentTime": "2026-03-26T09:00:00",
    "patientName": "Patient One",
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "doctorDiagnosis": "Cảm cúm nhẹ",
    "treatment": "Nghỉ ngơi, uống nhiều nước, uống thuốc hạ sốt",
    "prescription": "Paracetamol 500mg x 2 viên/ngày, uống sau ăn",
    "createdAt": "2026-03-26T10:00:00"
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Hồ sơ bệnh án không tồn tại",
  "data": null
}
```

---

### 9.4 Tạo hồ sơ bệnh án mới

**POST** `/api/MedicalRecords`

#### Auth

`[Authorize(Roles = "Doctor")]`

#### Request Body

```json
{
  "appointmentId": 1,
  "doctorDiagnosis": "Cảm cúm nhẹ",
  "treatment": "Nghỉ ngơi, uống nhiều nước, uống thuốc hạ sốt trong 3 ngày",
  "prescription": "Paracetamol 500mg x 2 viên/ngày, uống sau ăn"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `appointmentId` | int | ✅ | ID lịch hẹn đã hoàn thành |
| `doctorDiagnosis` | string | ❌ | Chẩn đoán của bác sĩ |
| `treatment` | string | ❌ | Phương pháp điều trị |
| `prescription` | string | ❌ | Đơn thuốc / toa thuốc |

#### Response 200

```json
{
  "success": true,
  "message": "Tạo hồ sơ bệnh án thành công",
  "data": {
    "medicalRecordId": 3,
    "appointmentId": 1,
    "appointmentTime": "2026-03-26T09:00:00",
    "patientName": "Patient One",
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "doctorDiagnosis": "Cảm cúm nhẹ",
    "treatment": "Nghỉ ngơi, uống nhiều nước, uống thuốc hạ sốt trong 3 ngày",
    "prescription": "Paracetamol 500mg x 2 viên/ngày, uống sau ăn",
    "createdAt": "2026-03-26T10:00:00"
  }
}
```

#### Response 400 — Lịch hẹn không hợp lệ

```json
{
  "success": false,
  "message": "Lịch hẹn không tồn tại hoặc không thuộc về bác sĩ này",
  "data": null
}
```

---

### 9.5 Cập nhật hồ sơ bệnh án

**PUT** `/api/MedicalRecords/{id}`

#### Auth

`[Authorize(Roles = "Doctor")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID hồ sơ bệnh án |

#### Request Body

```json
{
  "doctorDiagnosis": "Cảm cúm nhẹ (cập nhật)",
  "treatment": "Nghỉ ngơi, uống nhiều nước, bổ sung vitamin C",
  "prescription": "Paracetamol 500mg x 3 viên/ngày, Vitamin C 500mg x 1 viên/ngày"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `doctorDiagnosis` | string | ❌ | Chẩn đoán mới |
| `treatment` | string | ❌ | Phương pháp điều trị mới |
| `prescription` | string | ❌ | Đơn thuốc mới |

#### Response 200

```json
{
  "success": true,
  "message": "Cập nhật hồ sơ bệnh án thành công",
  "data": {
    "medicalRecordId": 1,
    "appointmentId": 1,
    "appointmentTime": "2026-03-26T09:00:00",
    "patientName": "Patient One",
    "doctorName": "Dr. John Smith",
    "departmentName": "Khoa Nội tổng hợp",
    "doctorDiagnosis": "Cảm cúm nhẹ (cập nhật)",
    "treatment": "Nghỉ ngơi, uống nhiều nước, bổ sung vitamin C",
    "prescription": "Paracetamol 500mg x 3 viên/ngày, Vitamin C 500mg x 1 viên/ngày",
    "createdAt": "2026-03-26T10:00:00"
  }
}
```

#### Response 400

```json
{
  "success": false,
  "message": "Hồ sơ bệnh án không tồn tại hoặc không thuộc về bác sĩ này",
  "data": null
}
```

---

## 11. API AI Chat — AI Chat

> **Base**: `/api/AI/chat`
> **Auth**: Yêu cầu đăng nhập (Patient)

---

### Trạng thái phiên trò chuyện

| Giá trị | Mô tả |
|---------|-------|
| `Active` | Phiên đang hoạt động |
| `Ended` | Phiên đã kết thúc |

---

### 10.1 Tạo phiên trò chuyện mới

**POST** `/api/AI/chat/sessions`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Response 200

```json
{
  "success": true,
  "message": "Tạo phiên trò chuyện thành công",
  "data": {
    "session": {
      "chatSessionId": "cs_a1b2c3d4e5f6",
      "createdAt": "2026-04-12T10:00:00",
      "updatedAt": "2026-04-12T10:00:00",
      "messageCount": 0,
      "status": "Active",
      "endedAt": null
    },
    "messages": []
  }
}
```

#### Response 400

```json
{
  "success": false,
  "message": "Không tìm thấy thông tin bệnh nhân",
  "data": null
}
```

---

### 10.2 Lấy danh sách phiên trò chuyện

**GET** `/api/AI/chat/sessions`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": [
    {
      "chatSessionId": "cs_a1b2c3d4e5f6",
      "createdAt": "2026-04-12T10:00:00",
      "updatedAt": "2026-04-12T10:30:00",
      "messageCount": 5,
      "status": "Active",
      "endedAt": null
    },
    {
      "chatSessionId": "cs_x9y8z7w6v5u",
      "createdAt": "2026-04-10T14:00:00",
      "updatedAt": "2026-04-10T14:45:00",
      "messageCount": 8,
      "status": "Ended",
      "endedAt": "2026-04-10T14:45:00"
    }
  ]
}
```

---

### 10.3 Lấy lịch sử trò chuyện

**GET** `/api/AI/chat/sessions/{sessionId}`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `sessionId` | string | ID phiên trò chuyện |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "chatSessionId": "cs_a1b2c3d4e5f6",
    "createdAt": "2026-04-12T10:00:00",
    "updatedAt": "2026-04-12T10:30:00",
    "messages": [
      {
        "chatMessageId": 1,
        "sender": "User",
        "content": "Tôi bị đau đầu và sốt nhẹ",
        "suggestedSpecialty": null,
        "confidenceScore": null,
        "createdAt": "2026-04-12T10:00:30"
      },
      {
        "chatMessageId": 2,
        "sender": "Assistant",
        "content": "Dựa trên triệu chứng bạn mô tả, tôi khuyên bạn nên đăng ký khám tại Khoa Nội tổng hợp để được khám và tư vấn chi tiết hơn.",
        "suggestedSpecialty": "Khoa Nội tổng hợp",
        "confidenceScore": 0.85,
        "createdAt": "2026-04-12T10:00:35"
      }
    ]
  }
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Phiên trò chuyện không tồn tại hoặc bạn không có quyền truy cập",
  "data": null
}
```

---

### 10.4 Gửi tin nhắn

**POST** `/api/AI/chat/sessions/{sessionId}/message`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `sessionId` | string | ID phiên trò chuyện |

#### Request Body

```json
{
  "content": "Tôi bị đau đầu và sốt nhẹ"
}
```

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|---------|-------|
| `content` | string | ✅ | Nội dung tin nhắn (1-2000 ký tự) |

#### Response 200

```json
{
  "success": true,
  "message": "Thao tác thành công",
  "data": {
    "userMessage": {
      "chatMessageId": 3,
      "sender": "User",
      "content": "Tôi bị đau đầu và sốt nhẹ",
      "suggestedSpecialty": null,
      "confidenceScore": null,
      "createdAt": "2026-04-12T10:30:00"
    },
    "assistantMessage": {
      "chatMessageId": 4,
      "sender": "Assistant",
      "content": "Dựa trên triệu chứng bạn mô tả, tôi khuyên bạn nên đăng ký khám tại Khoa Nội tổng hợp để được khám và tư vấn chi tiết hơn.",
      "suggestedSpecialty": "Khoa Nội tổng hợp",
      "confidenceScore": 0.85,
      "createdAt": "2026-04-12T10:30:05"
    },
    "session": {
      "chatSessionId": "cs_a1b2c3d4e5f6",
      "createdAt": "2026-04-12T10:00:00",
      "updatedAt": "2026-04-12T10:30:05",
      "messageCount": 4,
      "status": "Active",
      "endedAt": null
    }
  }
}
```

#### Response 400 — Tin nhắn không hợp lệ

```json
{
  "success": false,
  "message": "Nội dung tin nhắn không hợp lệ",
  "data": null
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Phiên trò chuyện không tồn tại",
  "data": null
}
```

---

### 10.5 Xóa phiên trò chuyện

**DELETE** `/api/AI/chat/sessions/{sessionId}`

#### Auth

`[Authorize(Roles = "Patient")]`

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `sessionId` | string | ID phiên trò chuyện |

#### Response 200

```json
{
  "success": true,
  "message": "Xóa phiên trò chuyện thành công",
  "data": null
}
```

#### Response 404

```json
{
  "success": false,
  "message": "Phiên trò chuyện không tồn tại hoặc bạn không có quyền xóa",
  "data": null
}
```

---

## 12. API Thông báo — Notifications

> **Base**: `/api/notifications`
> **Auth**: Yêu cầu đăng nhập (tất cả vai trò)

---

### Loại thông báo

| Giá trị | Mô tả | Người nhận |
|---------|-------|------------|
| `AppointmentCreated` | Có lịch hẹn mới | Bác sĩ |
| `AppointmentConfirmed` | Lịch hẹn được xác nhận | Bệnh nhân |
| `AppointmentCancelled` | Lịch hẹn bị hủy | Cả hai |
| `AppointmentAutoCancelled` | Lịch hẹn tự động bị hủy | Bệnh nhân |
| `AppointmentCompleted` | Lịch khám hoàn thành | Bệnh nhân |

---

### 12.1 Lấy danh sách thông báo của user hiện tại

**GET** `/api/notifications`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Response 200

```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "notificationId": 1,
      "title": "Lịch hẹn mới",
      "message": "Bệnh nhân Đỗ Minh Quân vừa đặt lịch khám vào 14:00 01/05/2026.",
      "type": "AppointmentCreated",
      "relatedId": 5,
      "isRead": false,
      "createdAt": "2026-04-30T14:00:00"
    }
  ]
}
```

| Trường | Kiểu | Mô tả |
|--------|------|-------|
| `notificationId` | int | ID thông báo |
| `title` | string | Tiêu đề thông báo |
| `message` | string | Nội dung chi tiết |
| `type` | string | Loại thông báo |
| `relatedId` | int? | AppointmentId liên quan |
| `isRead` | bool | Đã đọc hay chưa |
| `createdAt` | datetime | Thời gian tạo |

---

### 12.2 Lấy số thông báo chưa đọc

**GET** `/api/notifications/unread-count`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Response 200

```json
{
  "success": true,
  "message": null,
  "data": {
    "count": 3
  }
}
```

---

### 12.3 Đánh dấu một thông báo là đã đọc

**PUT** `/api/notifications/{id}/read`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Path Parameters

| Tham số | Kiểu | Mô tả |
|---------|------|-------|
| `id` | int | ID thông báo |

#### Response 200

```json
{
  "success": true,
  "message": "Đã đánh dấu là đã đọc",
  "data": null
}
```

---

### 12.4 Đánh dấu tất cả thông báo là đã đọc

**PUT** `/api/notifications/read-all`

#### Auth

`[Authorize]` — Tất cả vai trò đã đăng nhập

#### Response 200

```json
{
  "success": true,
  "message": "Đã đánh dấu tất cả là đã đọc",
  "data": null
}
```

---

### SignalR Hub — Real-time Notifications

**Endpoint:** `/hubs/notifications`
**Auth:** Truyền JWT token qua query string: `/hubs/notifications?access_token=<token>`

#### Cách sử dụng

```javascript
// 1. Kết nối đến hub với token
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications", { accessTokenFactory: () => jwtToken })
    .build();

// 2. Join group theo userId
await hubConnection.invoke("JoinUserGroup", userId);

// 3. Lắng nghe thông báo mới
hubConnection.on("ReceiveNotification", (notification) => {
    console.log("Thông báo mới:", notification);
    // Hiển thị toast / cập nhật badge
});
```

---

## 13. Danh sách HTTP Status Codes

| Status Code | Mô tả |
|-------------|-------|
| `200 OK` | Thành công |
| `400 Bad Request` | Dữ liệu không hợp lệ |
| `401 Unauthorized` | Chưa đăng nhập hoặc token không hợp lệ |
| `403 Forbidden` | Không có quyền thực hiện thao tác |
| `404 Not Found` | Tài nguyên không tồn tại |

---

## Ví dụ Test với PowerShell

```powershell
# 1. Đăng nhập để lấy token
$login = Invoke-RestMethod -Uri "http://localhost:5220/api/Auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"email":"admin","password":"admin123"}'

$token = $login.data.token

# 2. Lấy danh sách khoa (công khai)
Invoke-RestMethod -Uri "http://localhost:5220/api/Departments" `
    -Method GET

# 3. Lấy danh sách bác sĩ (công khai)
Invoke-RestMethod -Uri "http://localhost:5220/api/Doctors" `
    -Method GET

# 4. Lấy thông tin user hiện tại (yêu cầu auth)
Invoke-RestMethod -Uri "http://localhost:5220/api/Users/me" `
    -Method GET `
    -Headers @{ "Authorization" = "Bearer $token" }

# 5. Cập nhật hồ sơ cá nhân
Invoke-RestMethod -Uri "http://localhost:5220/api/Users/me" `
    -Method PUT `
    -ContentType "application/json" `
    -Headers @{ "Authorization" = "Bearer $token" } `
    -Body '{"fullName":"Nguyễn Văn B","phone":"0912345678"}'

# 6. Tạo lịch hẹn mới (Patient)
Invoke-RestMethod -Uri "http://localhost:5220/api/Appointments" `
    -Method POST `
    -ContentType "application/json" `
    -Headers @{ "Authorization" = "Bearer $token" } `
    -Body '{"doctorId":1,"appointmentTime":"2026-03-27T10:00:00"}'

# 7. Lấy danh sách thông báo
Invoke-RestMethod -Uri "http://localhost:5220/api/notifications" `
    -Method GET `
    -Headers @{ "Authorization" = "Bearer $token" }

# 8. Lấy số thông báo chưa đọc
Invoke-RestMethod -Uri "http://localhost:5220/api/notifications/unread-count" `
    -Method GET `
    -Headers @{ "Authorization" = "Bearer $token" }

# 9. Đánh dấu thông báo đã đọc
Invoke-RestMethod -Uri "http://localhost:5220/api/notifications/1/read" `
    -Method PUT `
    -Headers @{ "Authorization" = "Bearer $token" }

# 10. Đánh dấu tất cả thông báo đã đọc
Invoke-RestMethod -Uri "http://localhost:5220/api/notifications/read-all" `
    -Method PUT `
    -Headers @{ "Authorization" = "Bearer $token" }
```

---

## Ví dụ Test với curl

```bash
# 1. Đăng nhập
curl -X POST http://localhost:5220/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin","password":"admin123"}'

# 2. Lấy danh sách khoa
curl http://localhost:5220/api/Departments

# 3. Lấy danh sách bác sĩ
curl http://localhost:5220/api/Doctors

# 4. Lấy thông tin user hiện tại
TOKEN="<token>"
curl http://localhost:5220/api/Users/me \
  -H "Authorization: Bearer $TOKEN"

# 5. Cập nhật hồ sơ cá nhân
curl -X PUT http://localhost:5220/api/Users/me \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"fullName":"Nguyễn Văn B","phone":"0912345678"}'

# 6. Tạo lịch hẹn (Patient)
curl -X POST http://localhost:5220/api/Appointments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"doctorId":1,"appointmentTime":"2026-03-27T10:00:00"}'

# 7. Tạo hồ sơ bệnh án (Doctor)
curl -X POST http://localhost:5220/api/MedicalRecords \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"appointmentId":1,"doctorDiagnosis":"Cảm cúm","treatment":"Nghỉ ngơi","prescription":"Paracetamol 500mg"}'

# 8. Cập nhật hồ sơ bệnh án (Doctor)
curl -X PUT http://localhost:5220/api/MedicalRecords/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"doctorDiagnosis":"Cảm cúm (cập nhật)","treatment":"Nghỉ ngơi, bổ sung vitamin"}'

# 9. Lấy danh sách thông báo
curl http://localhost:5220/api/notifications \
  -H "Authorization: Bearer $TOKEN"

# 10. Lấy số thông báo chưa đọc
curl http://localhost:5220/api/notifications/unread-count \
  -H "Authorization: Bearer $TOKEN"

# 11. Đánh dấu thông báo đã đọc
curl -X PUT http://localhost:5220/api/notifications/1/read \
  -H "Authorization: Bearer $TOKEN"

# 12. Đánh dấu tất cả thông báo đã đọc
curl -X PUT http://localhost:5220/api/notifications/read-all \
  -H "Authorization: Bearer $TOKEN"
```

---

## Tổng hợp Endpoints

| # | Phương thức | Endpoint | Auth | Vai trò |
|---|-------------|----------|------|---------|
| | | **Auth** | | |
| 1 | POST | `/api/Auth/register` | ❌ | — |
| 2 | POST | `/api/Auth/login` | ❌ | — |
| 3 | PUT | `/api/Auth/change-password` | ✅ | Tất cả |
| | | **Departments** | | |
| 4 | GET | `/api/Departments` | ❌ | Công khai |
| 5 | GET | `/api/Departments/{id}` | ❌ | Công khai |
| 6 | POST | `/api/Departments` | ✅ | Admin |
| 7 | PUT | `/api/Departments/{id}` | ✅ | Admin |
| 8 | DELETE | `/api/Departments/{id}` | ✅ | Admin |
| | | **Doctors** | | |
| 9 | GET | `/api/Doctors` | ❌ | Công khai |
| 10 | GET | `/api/Doctors/{id}` | ❌ | Công khai |
| 11 | GET | `/api/Doctors/department/{departmentId}` | ❌ | Công khai |
| 12 | POST | `/api/Doctors` | ✅ | Admin |
| 13 | PUT | `/api/Doctors/{id}` | ✅ | Admin |
| 14 | PUT | `/api/Doctors/{id}/department` | ✅ | Admin |
| | | **Users** | | |
| 15 | GET | `/api/Users/me` | ✅ | Tất cả |
| 16 | PUT | `/api/Users/me` | ✅ | Tất cả |
| 17 | GET | `/api/Users` | ✅ | Admin |
| 18 | GET | `/api/Users/{id}` | ✅ | Admin |
| 19 | POST | `/api/Users` | ✅ | Admin |
| 20 | DELETE | `/api/Users/{id}` | ✅ | Admin |
| | | **Appointments** | | |
| 21 | GET | `/api/Appointments/patient` | ✅ | Patient |
| 22 | GET | `/api/Appointments/doctor` | ✅ | Doctor |
| 23 | GET | `/api/Appointments/doctor/schedule?date=` | ✅ | Doctor |
| 24 | GET | `/api/Appointments/{id}` | ✅ | Tất cả |
| 25 | POST | `/api/Appointments` | ✅ | Patient |
| 26 | PUT | `/api/Appointments/{id}/status` | ✅ | Doctor |
| 27 | DELETE | `/api/Appointments/{id}` | ✅ | Patient |
| | | **Appointments (Admin)** | | |
| 28 | GET | `/api/Appointments/admin/all` | ✅ | Admin |
| 29 | GET | `/api/Appointments/admin/{id}` | ✅ | Admin |
| 30 | PUT | `/api/Appointments/admin/{id}/status` | ✅ | Admin |
| 31 | DELETE | `/api/Appointments/admin/{id}` | ✅ | Admin |
| | | **MedicalRecords** | | |
| 32 | GET | `/api/MedicalRecords/patient` | ✅ | Patient |
| 33 | GET | `/api/MedicalRecords/doctor` | ✅ | Doctor |
| 34 | GET | `/api/MedicalRecords/{id}` | ✅ | Tất cả |
| 35 | POST | `/api/MedicalRecords` | ✅ | Doctor |
| 36 | PUT | `/api/MedicalRecords/{id}` | ✅ | Doctor |
| | | **AI Chat** | | |
| 37 | POST | `/api/AI/chat/sessions` | ✅ | Patient |
| 38 | GET | `/api/AI/chat/sessions` | ✅ | Patient |
| 39 | GET | `/api/AI/chat/sessions/{sessionId}` | ✅ | Patient |
| 40 | POST | `/api/AI/chat/sessions/{sessionId}/message` | ✅ | Patient |
| 41 | DELETE | `/api/AI/chat/sessions/{sessionId}` | ✅ | Patient |
| | | **Notifications** | | |
| 42 | GET | `/api/notifications` | ✅ | Tất cả |
| 43 | GET | `/api/notifications/unread-count` | ✅ | Tất cả |
| 44 | PUT | `/api/notifications/{id}/read` | ✅ | Tất cả |
| 45 | PUT | `/api/notifications/read-all` | ✅ | Tất cả |
| | | **SignalR Hub** | | |
| 46 | WS | `/hubs/notifications` | ✅ | Tất cả |

---

*Tài liệu này được tạo tự động dựa trên mã nguồn API. Cập nhật khi có thay đổi về endpoints.*
