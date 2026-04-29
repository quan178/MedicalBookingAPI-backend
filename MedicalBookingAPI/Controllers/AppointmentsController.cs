using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentService = appointmentService;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private async Task<int> GetPatientIdFromUserId(int userId)
    {
        var patient = await _patientRepository.GetPatientByUserIdAsync(userId);
        return patient?.PatientId ?? 0;
    }

    private async Task<int> GetDoctorIdFromUserId(int userId)
    {
        var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
        return doctor?.DoctorId ?? 0;
    }

    [HttpGet("patient")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentDto>>>> GetMyAppointments()
    {
        var userId = GetCurrentUserId();
        var patientId = await GetPatientIdFromUserId(userId);
        if (patientId == 0)
        {
            return NotFound(ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse("Thông tin bệnh nhân không tồn tại"));
        }

        var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
        return Ok(ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointments));
    }

    [HttpGet("doctor")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentDto>>>> GetMySchedule()
    {
        var userId = GetCurrentUserId();
        var doctorId = await GetDoctorIdFromUserId(userId);
        if (doctorId == 0)
        {
            return NotFound(ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse("Thông tin bác sĩ không tồn tại"));
        }

        var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
        return Ok(ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointments));
    }

    [HttpGet("doctor/schedule")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentDto>>>> GetDoctorSchedule([FromQuery] DateTime date)
    {
        var userId = GetCurrentUserId();
        var doctorId = await GetDoctorIdFromUserId(userId);
        if (doctorId == 0)
        {
            return NotFound(ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse("Thông tin bác sĩ không tồn tại"));
        }

        var appointments = await _appointmentService.GetDoctorScheduleAsync(doctorId, date);
        return Ok(ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointments));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetById(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound(ApiResponse<AppointmentDto>.ErrorResponse("Lịch hẹn không tồn tại"));
        }

        return Ok(ApiResponse<AppointmentDto>.SuccessResponse(appointment));
    }

    [HttpPost]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> Create([FromBody] CreateAppointmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        try
        {
            var userId = GetCurrentUserId();
            var patientId = await GetPatientIdFromUserId(userId);
            if (patientId == 0)
            {
                return NotFound(ApiResponse<AppointmentDto>.ErrorResponse("Thông tin bệnh nhân không tồn tại"));
            }

            var appointment = await _appointmentService.CreateAppointmentAsync(patientId, request);
            return Ok(ApiResponse<AppointmentDto>.SuccessResponse(appointment, "Tạo lịch hẹn thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<AppointmentDto>>> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AppointmentDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var userId = GetCurrentUserId();
        var doctorId = await GetDoctorIdFromUserId(userId);
        if (doctorId == 0)
        {
            return NotFound(ApiResponse<AppointmentDto>.ErrorResponse("Thông tin bác sĩ không tồn tại"));
        }

        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound(ApiResponse<AppointmentDto>.ErrorResponse("Lịch hẹn không tồn tại"));
        }

        if (appointment.DoctorId != doctorId)
        {
            return Forbid();
        }

        var updated = await _appointmentService.UpdateAppointmentStatusAsync(id, request.Status);
        return Ok(ApiResponse<AppointmentDto>.SuccessResponse(updated!, "Cập nhật trạng thái lịch hẹn thành công"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse>> Cancel(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var patientId = await GetPatientIdFromUserId(userId);
            if (patientId == 0)
            {
                return NotFound(ApiResponse.ErrorResponse("Thông tin bệnh nhân không tồn tại"));
            }

            var result = await _appointmentService.CancelAppointmentAsync(id, patientId);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Lịch hẹn không tồn tại hoặc không có quyền hủy"));
            }

            return Ok(ApiResponse.SuccessResponse("Hủy lịch hẹn thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AdminAppointmentDto>>>> GetAllAppointments([FromQuery] AppointmentFilterRequest filter)
    {
        var appointments = await _appointmentService.GetFilteredAppointmentsAsync(filter);
        return Ok(ApiResponse<IEnumerable<AdminAppointmentDto>>.SuccessResponse(appointments));
    }

    [HttpGet("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminAppointmentDto>>> GetAppointmentDetail(int id)
    {
        var appointment = await _appointmentService.GetAppointmentDetailsForAdminAsync(id);
        if (appointment == null)
        {
            return NotFound(ApiResponse<AdminAppointmentDto>.ErrorResponse("Lịch hẹn không tồn tại"));
        }

        return Ok(ApiResponse<AdminAppointmentDto>.SuccessResponse(appointment));
    }

    [HttpPut("admin/{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminAppointmentDto>>> AdminUpdateStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AdminAppointmentDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var updated = await _appointmentService.AdminUpdateStatusAsync(id, request.Status);
        if (updated == null)
        {
            return NotFound(ApiResponse<AdminAppointmentDto>.ErrorResponse("Lịch hẹn không tồn tại"));
        }

        return Ok(ApiResponse<AdminAppointmentDto>.SuccessResponse(updated, "Cập nhật trạng thái thành công"));
    }

    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> AdminCancel(int id)
    {
        try
        {
            var result = await _appointmentService.AdminCancelAppointmentAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse.ErrorResponse("Lịch hẹn không tồn tại"));
            }

            return Ok(ApiResponse.SuccessResponse("Hủy lịch hẹn thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
    }
}
