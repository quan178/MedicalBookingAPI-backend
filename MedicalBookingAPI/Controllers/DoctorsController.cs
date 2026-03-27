using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDetailDto>>>> GetAll()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(ApiResponse<IEnumerable<DoctorDetailDto>>.SuccessResponse(doctors));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DoctorDetailDto>>> GetById(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null)
        {
            return NotFound(ApiResponse<DoctorDetailDto>.ErrorResponse("Bác sĩ không tồn tại"));
        }

        return Ok(ApiResponse<DoctorDetailDto>.SuccessResponse(doctor));
    }

    [HttpGet("department/{departmentId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<DoctorDetailDto>>>> GetByDepartment(int departmentId)
    {
        var doctors = await _doctorService.GetDoctorsByDepartmentAsync(departmentId);
        return Ok(ApiResponse<IEnumerable<DoctorDetailDto>>.SuccessResponse(doctors));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DoctorDetailDto>>> Create([FromBody] CreateDoctorRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<DoctorDetailDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        try
        {
            var doctor = await _doctorService.CreateDoctorAsync(request);
            return Ok(ApiResponse<DoctorDetailDto>.SuccessResponse(doctor, "Tạo bác sĩ thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DoctorDetailDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DoctorDetailDto>>> Update(int id, [FromBody] UpdateDoctorRequest request)
    {
        var doctor = await _doctorService.UpdateDoctorAsync(id, request);
        if (doctor == null)
        {
            return NotFound(ApiResponse<DoctorDetailDto>.ErrorResponse("Bác sĩ không tồn tại"));
        }

        return Ok(ApiResponse<DoctorDetailDto>.SuccessResponse(doctor, "Cập nhật thông tin bác sĩ thành công"));
    }

    [HttpPut("{id}/department")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> AssignDepartment(int id, [FromBody] AssignDepartmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var result = await _doctorService.AssignDepartmentAsync(id, request.DepartmentId);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Bác sĩ hoặc khoa không tồn tại"));
        }

        return Ok(ApiResponse.SuccessResponse("Phân công khoa thành công"));
    }
}
