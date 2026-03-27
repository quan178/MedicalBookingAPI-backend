using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Đăng ký thất bại, email có thể đã tồn tại hoặc vai trò không hợp lệ"));
        }

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Đăng ký thành công"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Email hoặc mật khẩu không đúng"));
        }

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Đăng nhập thành công"));
    }

    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Dữ liệu không hợp lệ"));
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Không xác định được người dùng"));
        }

        var result = await _authService.ChangePasswordAsync(userId, request);
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Mật khẩu cũ không đúng hoặc người dùng không tồn tại"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Đổi mật khẩu thành công"));
    }
}
