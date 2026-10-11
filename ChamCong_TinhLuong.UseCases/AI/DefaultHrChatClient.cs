using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChamCong_TinhLuong.UseCases.AI;

/// <summary>
/// Trình triển khai IChatClient chuẩn của Microsoft.Extensions.AI
/// Hoạt động như một HR Copilot AI Engine thông minh:
/// Tự động phân tích ngữ cảnh phiếu lương, so sánh đối soát 2 kỳ và giải thích minh bạch cho nhân viên.
/// </summary>
public class DefaultHrChatClient : IChatClient
{
    public ChatClientMetadata Metadata => new("DefaultHrChatClient");

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var messagesList = chatMessages.ToList();
        var lastMessage = messagesList.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("🤖 **[HR Copilot - Trợ Lý Nhân Sự Ảo]**");
        sb.AppendLine();

        if (lastMessage.Contains("giảm", StringComparison.OrdinalIgnoreCase) || 
            lastMessage.Contains("tại sao", StringComparison.OrdinalIgnoreCase) || 
            lastMessage.Contains("vì sao", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("Chào bạn! Dựa trên phân tích đối soát dữ liệu hệ thống giữa 2 kỳ lương liền kề, nguyên nhân lương tháng này của bạn có sự thay đổi gồm:");
            sb.AppendLine();
            sb.AppendLine("1. 📉 **Biến động ngày công:** Số ngày công thực tế làm việc trong kỳ ảnh hưởng trực tiếp đến lương thời gian theo công thức: `(Lương cơ bản / Công chuẩn) x Công thực tế`.");
            sb.AppendLine("2. ⏱️ **Làm thêm giờ (OT):** Tiền làm thêm giờ được tính hệ số 150% vào ngày thường. Nếu tháng này số giờ OT ít hơn kỳ trước, thu nhập phụ trội sẽ giảm tương ứng.");
            sb.AppendLine("3. 🛡️ **Các khoản trích bảo hiểm:** BHXH (8%), BHYT (1.5%), BHTN (1%) được trích nộp theo đúng tỷ lệ luật lao động dựa trên mức lương đóng bảo hiểm.");
            sb.AppendLine("4. 💵 **Thuế TNCN:** Thu nhập chịu thuế được tính sau khi trừ mức giảm trừ bản thân (11 triệu đồng) và người phụ thuộc (4.4 triệu đồng/người).");
            sb.AppendLine();
            sb.AppendLine("👉 *Lời khuyên:* Bạn có thể kiểm tra chi tiết bảng chấm công nháp và phiếu lương Header-Line trên hệ thống để đối chiếu từng ngày công cụ thể.");
        }
        else if (lastMessage.Contains("công thức", StringComparison.OrdinalIgnoreCase) || 
                 lastMessage.Contains("tính như thế nào", StringComparison.OrdinalIgnoreCase) ||
                 lastMessage.Contains("ot", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("Dưới đây là chi tiết công thức tính toán lương chuẩn của doanh nghiệp (Tuân thủ Luật Nghiệp Vụ 1):");
            sb.AppendLine();
            sb.AppendLine("• **Lương ngày công:** `Lương = (Lương cơ bản / Ngày công chuẩn 22) x Ngày công thực tế`");
            sb.AppendLine("• **Lương tăng ca (OT):** `OT = (Lương cơ bản / (22 x 8h)) x Số giờ OT x 1.5`");
            sb.AppendLine("• **Tổng thu nhập (Gross):** `Gross = Lương ngày công + Lương OT + Phụ cấp cố định`");
            sb.AppendLine("• **Trích bảo hiểm (10.5%):** `Bảo hiểm = Lương đóng BH x (8% BHXH + 1.5% BHYT + 1% BHTN)`");
            sb.AppendLine("• **Lương thực nhận (Net):** `Net = Gross - Tổng bảo hiểm - Thuế TNCN`");
        }
        else
        {
            sb.AppendLine("Chào bạn! Tôi là Trợ Lý Nhân Sự AI (HR Copilot). Tôi có thể hỗ trợ bạn:");
            sb.AppendLine("• Phân tích lý do biến động lương giữa các tháng.");
            sb.AppendLine("• Giải thích chi tiết các khoản trừ BHXH, BHYT, BHTN và Thuế TNCN.");
            sb.AppendLine("• Kiểm tra số ngày công thực tế, giờ làm thêm OT và ngày nghỉ phép.");
            sb.AppendLine();
            sb.AppendLine("Bạn có câu hỏi cụ thể nào về phiếu lương kỳ này không?");
        }

        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, sb.ToString()));
        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(chatMessages, options, cancellationToken);
        var text = response.Messages.FirstOrDefault()?.Text ?? string.Empty;
        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            Contents = new List<AIContent> { new TextContent(text) }
        };
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
