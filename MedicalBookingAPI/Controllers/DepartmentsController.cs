using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetAll()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.SuccessResponse(departments));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.ErrorResponse("Khoa không tồn tại"));
        }

        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<DepartmentDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var department = await _departmentService.CreateDepartmentAsync(request);
        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department, "Tạo khoa thành công"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(int id, [FromBody] UpdateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<DepartmentDto>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var department = await _departmentService.UpdateDepartmentAsync(id, request);
        if (department == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.ErrorResponse("Khoa không tồn tại"));
        }

        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department, "Cập nhật khoa thành công"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var result = await _departmentService.DeleteDepartmentAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Khoa không tồn tại"));
        }

        return Ok(ApiResponse.SuccessResponse("Xóa khoa thành công"));
    }
}
