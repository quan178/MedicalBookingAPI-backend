using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public MedicalRecordsController(
        IMedicalRecordService medicalRecordService,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _medicalRecordService = medicalRecordService;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("patient")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordDto>>>> GetMyRecords()
    {
        var userId = GetCurrentUserId();
        var patient = await _patientRepository.GetPatientByUserIdAsync(userId);
        if (patient == null)
        {
            return NotFound(ApiResponse<IEnumerable<MedicalRecordDto>>.ErrorResponse("Thông tin bệnh nhân không tồn tại"));
        }

        var records = await _medicalRecordService.GetMedicalRecordsByPatientAsync(patient.PatientId);
        return Ok(ApiResponse<IEnumerable<MedicalRecordDto>>.SuccessResponse(records));
    }

    [HttpGet("doctor")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicalRecordDto>>>> GetDoctorRecords()
    {
        var userId = GetCurrentUserId();
        var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
        if (doctor == null)
        {
            return NotFound(ApiResponse<IEnumerable<MedicalRecordDto>>.ErrorResponse("Thông tin bác sĩ không tồn tại"));
        }

        var records = await _medicalRecordService.GetMedicalRecordsByDoctorAsync(doctor.DoctorId);
        return Ok(ApiResponse<IEnumerable<MedicalRecordDto>>.SuccessResponse(records));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> GetById(int id)
    {
        var record = await _medicalRecordService.GetMedicalRecordByIdAsync(id);
        if (record == null)
        {
            return NotFound(ApiResponse<MedicalRecordDto>.ErrorResponse("Hồ sơ bệnh án không tồn tại"));
        }

        return Ok(ApiResponse<MedicalRecordDto>.SuccessResponse(record));
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> Create([FromBody] CreateMedicalRecordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        try
        {
            var userId = GetCurrentUserId();
            var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
            if (doctor == null)
            {
                return NotFound(ApiResponse<MedicalRecordDto>.ErrorResponse("Thông tin bác sĩ không tồn tại"));
            }

            var record = await _medicalRecordService.CreateMedicalRecordAsync(request, doctor.DoctorId);
            return Ok(ApiResponse<MedicalRecordDto>.SuccessResponse(record, "Tạo hồ sơ bệnh án thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor")]
    public async Task<ActionResult<ApiResponse<MedicalRecordDto>>> Update(int id, [FromBody] UpdateMedicalRecordRequest request)
    {
        try
        {
            var record = await _medicalRecordService.UpdateMedicalRecordAsync(id, request);
            return Ok(ApiResponse<MedicalRecordDto>.SuccessResponse(record, "Cập nhật hồ sơ bệnh án thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MedicalRecordDto>.ErrorResponse(ex.Message));
        }
    }
}
