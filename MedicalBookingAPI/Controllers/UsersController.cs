using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Services.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDetailDto>.ErrorResponse("Người dùng không tồn tại"));
        }

        return Ok(ApiResponse<UserDetailDto>.SuccessResponse(user));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Thông tin người dùng không hợp lệ"));
        }

        if (id == currentUserId)
        {
            return BadRequest(ApiResponse.ErrorResponse("Bạn không thể xóa tài khoản của chính mình"));
        }

        var targetUser = await _userService.GetUserByIdAsync(id);
        if (targetUser == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại"));
        }

        if (targetUser.Role == "Admin")
        {
            return BadRequest(ApiResponse.ErrorResponse("Không thể xóa tài khoản Admin khác"));
        }

        var result = await _userService.DeleteUserAsync(id);
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResponse("Người dùng không tồn tại"));
        }

        return Ok(ApiResponse.SuccessResponse("Xóa người dùng thành công"));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateAdmin([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        if (user == null)
        {
            return BadRequest(ApiResponse.ErrorResponse("Email đã tồn tại"));
        }

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Tạo tài khoản Admin thành công"));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserDetailDto>.ErrorResponse("Thông tin người dùng không hợp lệ"));
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDetailDto>.ErrorResponse("Người dùng không tồn tại"));
        }

        return Ok(ApiResponse<UserDetailDto>.SuccessResponse(user));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserDetailDto>.ErrorResponse("Thông tin người dùng không hợp lệ"));
        }

        var user = await _userService.UpdateUserAsync(userId, request);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDetailDto>.ErrorResponse("Người dùng không tồn tại"));
        }

        return Ok(ApiResponse<UserDetailDto>.SuccessResponse(user, "Cập nhật hồ sơ thành công"));
    }
}
