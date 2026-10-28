namespace ChamCong_TinhLuong.UseCases.AI;

/// <summary>
/// Kết quả an toàn trả về cho UI. Không bao giờ ném exception ra ngoài:
/// UI chỉ cần kiểm tra <see cref="IsSuccess"/> để hiển thị nội dung hoặc thông báo lỗi thân thiện.
/// Đây chỉ là GỢI Ý để hiển thị — không được tự động ghi vào CSDL.
/// </summary>
public sealed record AiAdviceResult
{
    public bool IsSuccess { get; init; }
    public string? Advice { get; init; }
    public string? ErrorMessage { get; init; }

    public static AiAdviceResult Success(string advice) => new() { IsSuccess = true, Advice = advice };
    public static AiAdviceResult Failure(string friendlyMessage) => new() { IsSuccess = false, ErrorMessage = friendlyMessage };
}
