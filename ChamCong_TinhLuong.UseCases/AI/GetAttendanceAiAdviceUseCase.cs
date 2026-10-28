using System.Text;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChamCong_TinhLuong.UseCases.AI;

/// <summary>
/// Use Case: AI phân tích dữ liệu chấm công trong tháng và đưa ra GỢI Ý cho nhân viên.
/// - Chỉ ĐỌC dữ liệu, chỉ TRẢ VỀ text gợi ý. Không ghi gì vào CSDL.
/// - Mọi lỗi (mất kết nối, hết quota, timeout, sai key...) đều được bắt, log và chuyển thành thông báo thân thiện.
/// </summary>
public sealed class GetAttendanceAiAdviceUseCase
{
    private readonly IChatClient _chatClient;
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ILogger<GetAttendanceAiAdviceUseCase> _logger;
    private readonly AiOptions _options;

    public GetAttendanceAiAdviceUseCase(
        IChatClient chatClient,
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IOptions<AiOptions> options,
        ILogger<GetAttendanceAiAdviceUseCase> logger)
    {
        _chatClient = chatClient;
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiAdviceResult> ExecuteAsync(int employeeId, int month, int year, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
            if (employee is null)
                return AiAdviceResult.Failure("Không tìm thấy thông tin nhân viên.");

            var records = (await _attendanceRepo.GetAttendancesByEmployeeAsync(employeeId, month, year)).ToList();
            if (records.Count == 0)
                return AiAdviceResult.Failure($"Chưa có dữ liệu chấm công tháng {month}/{year} để phân tích.");

            // Chỉ gửi số liệu tổng hợp cần thiết — không gửi email, SĐT hay dữ liệu nhạy cảm khác lên AI.
            var context = new StringBuilder()
                .AppendLine($"Tháng {month}/{year}:")
                .AppendLine($"- Tổng công thực tế: {records.Sum(r => r.ActualWorkDays):0.#}")
                .AppendLine($"- Số ngày vắng (0 công): {records.Count(r => r.ActualWorkDays == 0)}")
                .AppendLine($"- Số lần đi trễ: {records.Count(r => r.LateMinutes > 0)}, tổng {records.Sum(r => r.LateMinutes)} phút")
                .AppendLine($"- Số lần về sớm: {records.Count(r => r.EarlyLeaveMinutes > 0)}, tổng {records.Sum(r => r.EarlyLeaveMinutes)} phút")
                .AppendLine($"- Tổng giờ OT: {records.Sum(r => r.OvertimeHours):0.#}")
                .ToString();

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System,
                    "Bạn là trợ lý nhân sự. Dựa trên số liệu chấm công, đưa ra tối đa 4 gợi ý ngắn gọn, lịch sự, bằng tiếng Việt " +
                    "để nhân viên cải thiện chuyên cần. Không bịa số liệu ngoài dữ liệu được cung cấp."),
                new(ChatRole.User, context)
            };

            // Giới hạn thời gian chờ để UI không bị treo khi AI phản hồi chậm.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, _options.TimeoutSeconds)));

            var response = await _chatClient.GetResponseAsync(messages, cancellationToken: timeoutCts.Token);
            var text = response.Text;

            return string.IsNullOrWhiteSpace(text)
                ? AiAdviceResult.Failure("AI không trả về nội dung. Vui lòng thử lại sau.")
                : AiAdviceResult.Success(text.Trim());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI timeout sau {Timeout}s (Provider={Provider}, Employee={EmployeeId})",
                _options.TimeoutSeconds, _options.Provider, employeeId);
            return AiAdviceResult.Failure("Trợ lý AI phản hồi quá lâu. Vui lòng thử lại sau ít phút.");
        }
        catch (OperationCanceledException)
        {
            // Người dùng rời trang / huỷ yêu cầu — không phải lỗi.
            return AiAdviceResult.Failure("Yêu cầu đã bị huỷ.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Không kết nối được dịch vụ AI (Provider={Provider}, Endpoint={Endpoint})",
                _options.Provider, _options.Endpoint);
            return AiAdviceResult.Failure("Không thể kết nối tới dịch vụ AI. Vui lòng kiểm tra kết nối hoặc thử lại sau.");
        }
        catch (Exception ex)
        {
            // Bao gồm lỗi quota (429), sai API key (401), lỗi phía nhà cung cấp (5xx)...
            _logger.LogError(ex, "Lỗi khi gọi AI (Provider={Provider}, Employee={EmployeeId})", _options.Provider, employeeId);
            return AiAdviceResult.Failure("Trợ lý AI tạm thời không khả dụng. Các chức năng khác vẫn hoạt động bình thường.");
        }
    }
}
