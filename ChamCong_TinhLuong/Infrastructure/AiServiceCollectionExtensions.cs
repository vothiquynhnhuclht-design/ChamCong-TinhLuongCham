using System.ClientModel;
using ChamCong_TinhLuong.UseCases.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace ChamCong_TinhLuong.Infrastructure;

/// <summary>
/// Đăng ký IChatClient theo cấu hình "AI:Provider". Đổi nhà cung cấp chỉ cần đổi cấu hình, không sửa Use Case.
/// Ollama và hầu hết dịch vụ Cloud đều hỗ trợ API chuẩn OpenAI (/v1/chat/completions),
/// nên chỉ cần một adapter (Microsoft.Extensions.AI.OpenAI) + Endpoint khác nhau.
/// </summary>
public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddHrAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName));

        services.AddSingleton<IChatClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AI");

            try
            {
                switch (opt.Provider.Trim().ToLowerInvariant())
                {
                    case "ollama":
                        // Ollama không kiểm tra API key nhưng SDK yêu cầu chuỗi khác rỗng.
                        return CreateOpenAiCompatible(opt.Endpoint ?? "http://localhost:11434/v1", opt.Model, opt.ApiKey ?? "ollama");

                    case "openai":
                    case "openaicompatible":
                        if (string.IsNullOrWhiteSpace(opt.ApiKey))
                        {
                            logger.LogWarning("Thiếu AI:ApiKey (User Secrets / biến môi trường AI__ApiKey). Chuyển sang chế độ Offline.");
                            break;
                        }
                        return CreateOpenAiCompatible(opt.Endpoint, opt.Model, opt.ApiKey);
                }
            }
            catch (Exception ex)
            {
                // Cấu hình sai (URL không hợp lệ...) không được làm sập ứng dụng lúc khởi động.
                logger.LogError(ex, "Không khởi tạo được AI provider {Provider}. Chuyển sang chế độ Offline.", opt.Provider);
            }

            return new DefaultHrChatClient();
        });

        services.AddTransient<GetAttendanceAiAdviceUseCase>();
        services.AddTransient<SaveConfirmedAttendanceNoteUseCase>();
        services.AddTransient<ExplainPayslipWithAIUseCase>();
        return services;
    }

    private static IChatClient CreateOpenAiCompatible(string? endpoint, string model, string apiKey)
    {
        var clientOptions = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(endpoint))
            clientOptions.Endpoint = new Uri(endpoint);

        return new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions)
            .GetChatClient(model)
            .AsIChatClient();
    }
}
