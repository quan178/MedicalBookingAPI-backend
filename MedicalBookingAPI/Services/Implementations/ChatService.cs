using System.Text;
using System.Text.Json;
using MedicalBookingAPI.Data;
using MedicalBookingAPI.DTOs;
using MedicalBookingAPI.Entities;
using MedicalBookingAPI.Enums;
using MedicalBookingAPI.Helpers;
using MedicalBookingAPI.Repositories.Interfaces;
using MedicalBookingAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MedicalBookingAPI.Services.Implementations;

public class ChatSettings
{
    public int InactivityTimeoutMinutes { get; set; } = 10;
}

public class ChatService : IChatService
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IChatEncryptionService _encryptionService;
    private readonly ChatSettings _chatSettings;

    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPrompt = @"Bạn là một trợ lý y tế thông minh của bệnh viện. Nhiệm vụ của bạn là:
1. Trò chuyện với người dùng để thu thập đầy đủ thông tin về triệu chứng
2. HỎI CÂU HỎI LÀM RÕ trước khi đưa ra bất kỳ gợi ý khoa nào
3. Gợi ý chuyên khoa phù hợp NHƯNG CHỈ SAU KHI đã có đủ thông tin
4. Trả lời các câu hỏi thường gặp về sức khỏe một cách thông tin (không thay thế tư vấn bác sĩ)

DANH SÁCH 10 CHUYÊN KHOA:
1. Tim mạch - Đau ngực, tim đập nhanh/chậm, khó thở, huyết áp cao/thấp, đau vùng tim
2. Thần kinh - Đau đầu, chóng mặt, run tay, mất ngủ kéo dài, đau thần kinh tọa, tai biến mạch máu não
3. Tiêu hóa - Đau bụng, buồn nôn, tiêu chảy, táo bón, ợ chua, viêm dạ dày, gan mật
4. Hô hấp - Ho, sốt, khó thở, viêm họng, viêm phổi, hen suyễn, lao phổi
5. Tai mũi họng - Đau tai, ù tai, nghẹt mũi, viêm amidan, viêm xoang, polyp mũi, viêm tai giữa
6. Da liễu - Nổi mẩn, ngứa, mụn trứng cá, herpes, zona, eczema, viêm da tiếp xúc
7. Cơ xương khớp - Đau lưng, đau khớp, viêm khớp, thoái hóa khớp, gãy xương, loãng xương
8. Nội tổng quát - Sốt, mệt mỏi, đau cơ, cảm cúm, kiểm tra sức khỏe định kỳ, bệnh tiểu đường
9. Nhi khoa - Sốt ở trẻ, ho ở trẻ, tiêu chảy ở trẻ, phát ban ở trẻ, tiêm chủng, phát triển trẻ
10. Sản phụ khoa - Đau bụng dưới, kinh nguyệt không đều, viêm nhiễm phụ khoa, thai kỳ, sinh sản

QUY TRÌNH XỬ LÝ (TUYỆT ĐỐI TUÂN THỦ):

BƯỚC 1 - THU THẬP THÔNG TIN:
- Khi người dùng mới mô tả triệu chứng (1-2 triệu chứng đơn lẻ), BẮT BUỘC phải hỏi câu hỏi làm rõ
- Câu hỏi bắt buộc hỏi (tùy triệu chứng, chọn phù hợp):
  * Thời gian: ""Bạn bị triệu chứng này bao lâu rồi?""
  * Mức độ: ""Mức độ đau/nặng như thế nào? (nhẹ/trung bình/nặng)""
  * Triệu chứng kèm theo: ""Bạn có triệu chứng nào khác đi kèm không?""
  * Vị trí cụ thể (nếu đau bụng/đau đầu): ""Bạn có thể mô tả cụ thể vị trí không?""
  * Tần suất: ""Triệu chứng xuất hiện thường xuyên không? Khi nào?""
- Hỏi TỐI ĐA 2 câu hỏi mỗi lần phản hồi, tự nhiên và thân thiện
- KHÔNG BAO GIỜ gợi ý khoa khi mới chỉ có 1-2 triệu chứng chưa rõ ràng

BƯỚC 2 - ĐÁNH GIÁ ĐỦ THÔNG TIN:
- Đủ thông tin khi có đầy đủ: triệu chứng + thời gian + mức độ + triệu chứng kèm theo
- Hoặc khi người dùng chủ động nói rõ nhiều triệu chứng cùng lúc
- Nếu thông tin vẫn chưa đủ, tiếp tục hỏi câu hỏi tiếp theo

BƯỚC 3 - GỢI Ý KHOA:
- Khi đã thu thập đủ thông tin, đưa ra gợi ý khoa phù hợp nhất
- Lời khuyên sức khỏe phải DO BẠN (AI) VIẾT TỰ NHIÊN trong cùng đoạn phản hồi, phù hợp với triệu chứng cụ thể đã trao đổi (ví dụ: nếu đau đầu thì khuyên nghỉ ngơi, tránh màn hình; nếu đau bụng thì khuyên ăn nhẹ, uống nước ấm...)
- Lời khuyên mang tính TỔNG QUÁT, không thay thế tư vấn bác sĩ
- Tất cả viết trong MỘT đoạn văn tự nhiên, kết thúc bằng lời dẫn người dùng đến trang đặt lịch thực tế của bệnh viện. Ví dụ: ""Bạn có thể đặt lịch khám tại Khoa [tên khoa] qua trang đặt lịch của bệnh viện để được tư vấn và khám chuyên sâu hơn.""

TRƯỜNG HỢP ĐẶC BIỆT:

TRƯỜNG HỢP NGƯỜI DÙNG MÔ TẢ TRIỆU CHỨNG NGUY CẤP:
Khi người dùng mô tả các triệu chứng cấp bách như: đau ngực dữ dội, khó thở nặng,
mất ý thức, đột quỵ, co giật, chảy máu nặng, ngộ độc, tự tử, hôn mê, méo miệng, nói lắp đột ngột, mắt mờ đột ngột, 
chảy máu nhiều, liệt, liệt nửa người, nhịp tim dừng, huyết áp tụt đột ngột, nôn ra máu, đau thắt ngực hoặc BẤT KỲ
tình trạng nào có thể đe dọa tính mạng → BẮT BUỘC trả lời theo format:

[EMERGENCY] <Phản hồi khẩn cấp theo mẫu bên dưới>

⚠️ **CẢNH BÁO KHẨN CẤP**
⚠️ Tình trạng của bạn có thể nguy hiểm đến tính mạng.

**Hành động ngay lập tức:**
✅ Gọi cấp cứu 115 NGAY
✅ Gọi người thân hoặc người xung quanh giúp đỡ
✅ KHÔNG tự ý đi lại nếu cảm thấy yếu

**Nếu bạn đang ở nhà:**
📞 Gọi 115 và báo địa chỉ chính xác
📞 Gọi người thân đến hỗ trợ
🚪 Mở cửa sẵn để nhân viên y tế vào được

**Lưu ý quan trọng:**
⚠️ KHÔNG tự ý uống thuốc khi chưa có hướng dẫn của bác sĩ
⚠️ KHÔNG chờ đợi triệu chứng giảm để gọi cấp cứu
⚠️ Gọi 115 NGAY - đội ngũ y tế sẽ hướng dẫn bạn các bước xử lý

TRƯỜNG HỢP BÌNH THƯỜNG:
Khi triệu chứng không phải cấp cấp → trả lời bình thường, KHÔNG cần prefix.

QUY TẮC BẮT BUỘC:
- Nếu triệu chứng THỰC SỰ nguy cấp, đe dọa tính mạng → dùng [EMERGENCY]
- Phản hồi KHÔNG có [EMERGENCY] → coi như bình thường
- KHÔNG BAO GIỜ dùng [EMERGENCY] khi không phải trường hợp nguy cấp thực sự

- Triệu chứng không khớp 10 khoa trên: Gợi ý khoa Nội tổng quát, giải thích và mời đặt lịch
- Người dùng hỏi chung về sức khỏe: Trả lời thông tin, vẫn hỏi câu hỏi làm rõ nếu muốn gợi ý khoa

LUÔN LUÔN KHÔNG LÀM:
- Không được hứa hẹn, mô tả hoặc thực hiện quy trình đặt lịch khám
- Không được hỏi thông tin cá nhân như họ tên, số điện thoại, địa chỉ, ngày sinh để phục vụ đặt lịch
- Không được hỏi về khung giờ, ngày khám mong muốn
- Không được liệt kê các bước đặt lịch hoặc hướng dẫn người dùng quy trình đặt lịch
- Nếu người dùng yêu cầu đặt lịch, hãy xác nhận khoa đã gợi ý và hướng họ đến trang đặt lịch thực tế của bệnh viện

LUÔN LUÔN:
- Trả lời bằng tiếng Việt, thân thiện, cảm thông
- Không đưa ra chẩn đoán y khoa, chỉ gợi ý chuyên khoa";

    public ChatService(
        IChatSessionRepository sessionRepository,
        IChatMessageRepository messageRepository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ChatService> logger,
        AppDbContext dbContext,
        IChatEncryptionService encryptionService,
        IOptions<ChatSettings> chatSettings)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
        _encryptionService = encryptionService;
        _chatSettings = chatSettings.Value;
    }

    public async Task<ChatSessionResponse> CreateSessionAsync(int? patientId)
    {
        var session = new ChatSession
        {
            PatientId = patientId,
            ChatSessionId = $"chat_{Guid.NewGuid():N}",
            CreatedAt = DateTimeHelper.Now,
            UpdatedAt = DateTimeHelper.Now
        };

        await _sessionRepository.AddAsync(session);

        return new ChatSessionResponse
        {
            Session = MapToSessionDto(session),
            Messages = new List<ChatMessageDto>()
        };
    }

    public async Task<ChatSessionResponse?> GetSessionByTokenAsync(string sessionId, int? patientId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return null;

        if (patientId.HasValue && session.PatientId != patientId.Value)
            return null;

        var messages = await _messageRepository.GetMessagesBySessionIdAsync(session.ChatSessionId);

        return new ChatSessionResponse
        {
            Session = MapToSessionDto(session),
            Messages = messages.Select(MapToMessageDto).ToList()
        };
    }

    public async Task<ChatHistoryDto?> GetChatHistoryAsync(string sessionId, int? patientId)
    {
        var session = await _sessionRepository.GetByIdWithMessagesAsync(sessionId);
        if (session == null) return null;

        if (patientId.HasValue && session.PatientId != patientId.Value)
            return null;

        return new ChatHistoryDto
        {
            ChatSessionId = session.ChatSessionId,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Messages = session.Messages.OrderBy(m => m.CreatedAt).Select(MapToMessageDto).ToList()
        };
    }

    public async Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, int? patientId)
    {
        ChatSession session;

        if (!string.IsNullOrEmpty(request.SessionToken))
        {
            var existingSession = await _sessionRepository.GetByIdAsync(request.SessionToken);
            if (existingSession == null)
            {
                throw new ArgumentException("Phiên trò chuyện không tồn tại");
            }

            if (existingSession.Status == ChatSessionStatus.Ended)
            {
                throw new ArgumentException("Phiên trò chuyện này đã kết thúc. Vui lòng tạo phiên trò chuyện mới.");
            }

            if (patientId.HasValue && existingSession.PatientId != patientId.Value)
            {
                throw new ArgumentException("Bạn không có quyền truy cập phiên trò chuyện này");
            }

            session = existingSession;
        }
        else
        {
            session = new ChatSession
            {
                PatientId = patientId,
                ChatSessionId = $"chat_{Guid.NewGuid():N}",
                CreatedAt = DateTimeHelper.Now,
                UpdatedAt = DateTimeHelper.Now
            };
            await _sessionRepository.AddAsync(session);
        }

        var userMessage = new ChatMessage
        {
            ChatSessionId = session.ChatSessionId,
            Sender = MessageSender.User,
            Content = _encryptionService.Encrypt(request.Content),
            IsEncrypted = true,
            CreatedAt = DateTimeHelper.Now
        };
        await _messageRepository.AddAsync(userMessage);

        session.UpdatedAt = DateTimeHelper.Now;
        await _sessionRepository.UpdateAsync(session);

        var assistantResponse = await GetAssistantResponseAsync(session.ChatSessionId);

        var assistantMessage = new ChatMessage
        {
            ChatSessionId = session.ChatSessionId,
            Sender = MessageSender.Assistant,
            Content = _encryptionService.Encrypt(assistantResponse.Content),
            IsEncrypted = true,
            SuggestedSpecialty = assistantResponse.SuggestedSpecialty,
            ConfidenceScore = assistantResponse.ConfidenceScore,
            CreatedAt = DateTimeHelper.Now
        };
        await _messageRepository.AddAsync(assistantMessage);

        session.UpdatedAt = DateTimeHelper.Now;
        await _sessionRepository.UpdateAsync(session);

        await _dbContext.SaveChangesAsync();

        return new SendMessageResponse
        {
            UserMessage = MapToMessageDto(userMessage),
            AssistantMessage = MapToMessageDto(assistantMessage),
            Session = MapToSessionDto(session)
        };
    }

    public async Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(int? patientId)
    {
        IEnumerable<ChatSession> sessions;

        if (patientId.HasValue)
        {
            sessions = await _sessionRepository.GetSessionsByPatientAsync(patientId.Value);
        }
        else
        {
            sessions = await _sessionRepository.GetRecentSessionsAsync(1, 50);
        }

        var sessionDtos = new List<ChatSessionDto>();
        foreach (var session in sessions)
        {
            var messageCount = await _messageRepository.GetMessageCountBySessionIdAsync(session.ChatSessionId);
            sessionDtos.Add(new ChatSessionDto
            {
                ChatSessionId = session.ChatSessionId,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                MessageCount = messageCount,
                Status = session.Status.ToString(),
                EndedAt = session.EndedAt
            });
        }

        return sessionDtos.OrderByDescending(s => s.UpdatedAt);
    }

    public async Task<bool> DeleteSessionAsync(string sessionId, int? patientId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (patientId.HasValue && session.PatientId != patientId.Value)
            return false;

        await _sessionRepository.DeleteAsync(session.ChatSessionId);
        return true;
    }

    public async Task<bool> EndSessionAsync(string sessionId, int? patientId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (patientId.HasValue && session.PatientId != patientId.Value)
            return false;

        if (session.Status == ChatSessionStatus.Ended)
            return true;

        session.Status = ChatSessionStatus.Ended;
        session.EndedAt = DateTimeHelper.Now;
        await _sessionRepository.UpdateAsync(session);
        return true;
    }

    private async Task<(string Content, string? SuggestedSpecialty, double? ConfidenceScore)> GetAssistantResponseAsync(string sessionId)
    {
        var messages = (await _messageRepository.GetMessagesBySessionIdAsync(sessionId)).ToList();

        var conversationHistory = messages
            .Select(m => new
            {
                role = m.Sender == MessageSender.User ? "user" : "assistant",
                content = m.IsEncrypted
                    ? _encryptionService.Decrypt(m.Content)
                    : m.Content
            })
            .ToList();

        var responseContent = await CallBeeknoeeApiAsync(conversationHistory.Cast<object>().ToList(), SystemPrompt);

        if (responseContent.StartsWith("[EMERGENCY]", StringComparison.OrdinalIgnoreCase))
        {
            return (responseContent, (string?)null, (double?)null);
        }

        var (hasSpecialty, specialty) = TryExtractSpecialtyFromResponse(responseContent);

        string? suggestedSpecialty = null;
        double? confidenceScore = null;

        if (hasSpecialty && !string.IsNullOrEmpty(specialty))
        {
            suggestedSpecialty = specialty;
            confidenceScore = messages.Count switch
            {
                >= 6 => 0.90,
                >= 4 => 0.80,
                _ => 0.70
            };
        }

        return (responseContent, suggestedSpecialty, confidenceScore);
    }

    private static readonly string[] SpecialtyNames = new[]
    {
        "Tim mạch", "Thần kinh", "Tiêu hóa", "Hô hấp",
        "Tai mũi họng", "Da liễu", "Cơ xương khớp",
        "Nội tổng quát", "Nhi khoa", "Sản phụ khoa"
    };

    /// <summary>
    /// Trích xuất tên khoa từ phản hồi text của AI.
    /// Trả về (true, tên_khoa) nếu tìm thấy, ngược lại (false, null).
    /// </summary>
    private static (bool HasSpecialty, string? Specialty) TryExtractSpecialtyFromResponse(string response)
    {
        foreach (var specialty in SpecialtyNames)
        {
            if (response.Contains(specialty, StringComparison.OrdinalIgnoreCase))
            {
                return (true, specialty);
            }
        }
        return (false, null);
    }

    private async Task<string> CallBeeknoeeApiAsync(List<object> conversationHistory, string systemInstruction)
    {
        var httpClient = _httpClientFactory.CreateClient("BeeknoeeClient");

        var apiKey = _configuration["BeeknoeeAI:ApiKey"] ?? throw new InvalidOperationException("Beeknoee AI API key is not configured");
        var baseUrl = _configuration["BeeknoeeAI:BaseUrl"] ?? "https://platform.beeknoee.com/api/v1/chat/completions";
        var model = _configuration["BeeknoeeAI:DefaultModel"] ?? "gpt-5.4-mini";

        var messages = new List<object>();

        if (!string.IsNullOrEmpty(systemInstruction))
        {
            messages.Add(new { role = "system", content = systemInstruction });
        }

        foreach (var msg in conversationHistory)
        {
            if (msg is not null)
            {
                messages.Add(msg);
            }
        }

        var requestBody = new
        {
            model = model,
            messages = messages,
            temperature = 0.7,
            max_tokens = 1024
        };

        try
        {
            await _semaphore.WaitAsync();
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var jsonContent = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Beeknoee API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                    return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
                }

                var beeknoeeResponse = JsonSerializer.Deserialize<BeeknoeeChatResponse>(responseContent, JsonOptions);

                if (beeknoeeResponse?.Choices == null || beeknoeeResponse.Choices.Length == 0)
                {
                    return "Xin lỗi, tôi chưa có phản hồi phù hợp. Bạn có thể mô tả rõ hơn triệu chứng không?";
                }

                var rawText = beeknoeeResponse.Choices[0].Message?.Content ?? string.Empty;

                if (rawText.StartsWith('{') && rawText.Contains("specialty_name"))
                {
                    return "Tôi đã ghi nhận thông tin của bạn. Để tôi tổng hợp và đưa ra gợi ý chuyên khoa phù hợp nhất nhé.";
                }

                if (rawText.StartsWith("[EMERGENCY]"))
                {
                    return rawText.Substring("[EMERGENCY]".Length).Trim();
                }

                return rawText.Trim();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Beeknoee API");
            return "Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại sau.";
        }
    }

    private static ChatSessionDto MapToSessionDto(ChatSession session)
    {
        return new ChatSessionDto
        {
            ChatSessionId = session.ChatSessionId,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            MessageCount = session.Messages?.Count ?? 0,
            Status = session.Status.ToString(),
            EndedAt = session.EndedAt
        };
    }

    private ChatMessageDto MapToMessageDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            ChatMessageId = message.ChatMessageId,
            Sender = message.Sender.ToString(),
            Content = message.IsEncrypted
                ? _encryptionService.Decrypt(message.Content)
                : message.Content,
            SuggestedSpecialty = message.SuggestedSpecialty,
            ConfidenceScore = message.ConfidenceScore,
            CreatedAt = message.CreatedAt
        };
    }

    private class BeeknoeeChatResponse
    {
        public BeeknoeeChatChoice[]? Choices { get; set; }
    }

    private class BeeknoeeChatChoice
    {
        public BeeknoeeChatMessage? Message { get; set; }
    }

    private class BeeknoeeChatMessage
    {
        public string? Content { get; set; }
    }
}
