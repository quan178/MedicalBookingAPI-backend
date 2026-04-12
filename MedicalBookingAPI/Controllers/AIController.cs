using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Services.Interfaces;
using MedicalBookingAPI.Repositories.Interfaces;

namespace MedicalBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IChatService chatService,
        IDepartmentRepository departmentRepository,
        IPatientRepository patientRepository,
        ILogger<AIController> logger)
    {
        _chatService = chatService;
        _departmentRepository = departmentRepository;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return 0;
        }
        return userId;
    }

    private async Task<int> GetCurrentPatientIdAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return 0;

        var patient = await _patientRepository.GetPatientByUserIdAsync(userId);
        return patient?.PatientId ?? 0;
    }

    [HttpPost("chat/sessions")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<ChatSessionResponse>>> CreateChatSession()
    {
        try
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == 0)
            {
                return BadRequest(ApiResponse<ChatSessionResponse>.ErrorResponse("Không tìm thấy thông tin bệnh nhân"));
            }

            var session = await _chatService.CreateSessionAsync(patientId);
            return Ok(ApiResponse<ChatSessionResponse>.SuccessResponse(session, "Tạo phiên trò chuyện thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat session");
            return StatusCode(500, ApiResponse<ChatSessionResponse>.ErrorResponse("Đã xảy ra lỗi khi tạo phiên trò chuyện"));
        }
    }

    [HttpGet("chat/sessions")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ChatSessionDto>>>> GetChatSessions()
    {
        try
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == 0)
            {
                return BadRequest(ApiResponse<IEnumerable<ChatSessionDto>>.ErrorResponse("Không tìm thấy thông tin bệnh nhân"));
            }

            var sessions = await _chatService.GetUserSessionsAsync(patientId);
            return Ok(ApiResponse<IEnumerable<ChatSessionDto>>.SuccessResponse(sessions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat sessions");
            return StatusCode(500, ApiResponse<IEnumerable<ChatSessionDto>>.ErrorResponse("Đã xảy ra lỗi khi lấy danh sách phiên trò chuyện"));
        }
    }

    [HttpGet("chat/sessions/{sessionId}")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<ChatHistoryDto>>> GetChatHistory(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(ApiResponse<ChatHistoryDto>.ErrorResponse("Session token không hợp lệ"));
        }

        try
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == 0)
            {
                return BadRequest(ApiResponse<ChatHistoryDto>.ErrorResponse("Không tìm thấy thông tin bệnh nhân"));
            }

            var history = await _chatService.GetChatHistoryAsync(sessionId, patientId);
            if (history == null)
            {
                return NotFound(ApiResponse<ChatHistoryDto>.ErrorResponse("Phiên trò chuyện không tồn tại hoặc bạn không có quyền truy cập"));
            }

            return Ok(ApiResponse<ChatHistoryDto>.SuccessResponse(history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for session: {SessionToken}", sessionId);
            return StatusCode(500, ApiResponse<ChatHistoryDto>.ErrorResponse("Đã xảy ra lỗi khi lấy lịch sử trò chuyện"));
        }
    }

    [HttpPost("chat/sessions/{sessionId}/message")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse<SendMessageResponse>>> SendMessage(
        string sessionId,
        [FromBody] SendMessageRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<SendMessageResponse>.ErrorResponse("Nội dung tin nhắn không hợp lệ"));
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(ApiResponse<SendMessageResponse>.ErrorResponse("Session token không hợp lệ"));
        }

        try
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == 0)
            {
                return BadRequest(ApiResponse<SendMessageResponse>.ErrorResponse("Không tìm thấy thông tin bệnh nhân"));
            }

            request.SessionToken = sessionId;
            var response = await _chatService.SendMessageAsync(request, patientId);

            _logger.LogInformation("Message sent in session: {SessionToken}", sessionId);

            return Ok(ApiResponse<SendMessageResponse>.SuccessResponse(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for send message");
            return BadRequest(ApiResponse<SendMessageResponse>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(503, ApiResponse<SendMessageResponse>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending message");
            return StatusCode(500, ApiResponse<SendMessageResponse>.ErrorResponse("Đã xảy ra lỗi không mong muốn khi gửi tin nhắn"));
        }
    }

    [HttpDelete("chat/sessions/{sessionId}")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ApiResponse>> DeleteChatSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(ApiResponse.ErrorResponse("Session token không hợp lệ"));
        }

        try
        {
            var patientId = await GetCurrentPatientIdAsync();
            if (patientId == 0)
            {
                return BadRequest(ApiResponse.ErrorResponse("Không tìm thấy thông tin bệnh nhân"));
            }

            var deleted = await _chatService.DeleteSessionAsync(sessionId, patientId);
            if (!deleted)
            {
                return NotFound(ApiResponse.ErrorResponse("Phiên trò chuyện không tồn tại hoặc bạn không có quyền xóa"));
            }

            return Ok(ApiResponse.SuccessResponse("Xóa phiên trò chuyện thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat session: {SessionToken}", sessionId);
            return StatusCode(500, ApiResponse.ErrorResponse("Đã xảy ra lỗi khi xóa phiên trò chuyện"));
        }
    }
}
