namespace ChamCong_TinhLuong.UseCases.AI;

/// <summary>
/// Cấu hình AI đọc từ IConfiguration (section "AI").
/// ApiKey KHÔNG được đặt trong appsettings.json — chỉ đọc từ User Secrets hoặc biến môi trường (AI__ApiKey).
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "AI";

    /// <summary>"Offline" (bộ trả lời nội bộ, không gọi mạng) | "Ollama" | "OpenAI" | "OpenAICompatible".</summary>
    public string Provider { get; set; } = "Offline";

    /// <summary>Ví dụ Ollama: http://localhost:11434/v1 — Cloud tương thích OpenAI: URL của nhà cung cấp.</summary>
    public string? Endpoint { get; set; }

    /// <summary>Tên model, ví dụ "llama3.2" (Ollama) hoặc "gpt-4o-mini" (OpenAI).</summary>
    public string Model { get; set; } = "llama3.2";

    /// <summary>Đọc từ User Secrets / biến môi trường. Ollama local không cần.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Thời gian chờ tối đa cho mỗi lần gọi AI (giây).</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
