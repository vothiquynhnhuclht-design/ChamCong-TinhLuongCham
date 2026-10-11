using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;
using Microsoft.Extensions.AI;
using System.Text;

namespace ChamCong_TinhLuong.UseCases.AI;

public class ExplainPayslipWithAIUseCase
{
    private readonly IChatClient _chatClient;
    private readonly IPayslipRepository _payslipRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IContractRepository _contractRepo;

    public ExplainPayslipWithAIUseCase(
        IChatClient chatClient,
        IPayslipRepository payslipRepo,
        IEmployeeRepository employeeRepo,
        IAttendanceRepository attendanceRepo,
        IContractRepository contractRepo)
    {
        _chatClient = chatClient;
        _payslipRepo = payslipRepo;
        _employeeRepo = employeeRepo;
        _attendanceRepo = attendanceRepo;
        _contractRepo = contractRepo;
    }

    /// <summary>
    /// Thực thi UseCase AI phân tích và giải thích phiếu lương cho nhân viên
    /// </summary>
    public async Task<string> ExecuteAsync(int employeeId, int currentMonth, int currentYear, string userQuestion)
    {
        var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
        if (employee == null) return "Không tìm thấy thông tin nhân viên.";

        // 1. Lấy phiếu lương kỳ hiện tại
        var currentPayslip = await _payslipRepo.GetPayslipByEmployeeAndPeriodAsync(employeeId, currentMonth, currentYear);
        if (currentPayslip == null)
        {
            return $"Nhân viên {employee.FullName} chưa có dữ liệu phiếu lương kỳ tháng {currentMonth}/{currentYear}.";
        }

        // 2. Lấy phiếu lương kỳ liền kề trước đó (để đối soát chênh lệch)
        int prevMonth = currentMonth == 1 ? 12 : currentMonth - 1;
        int prevYear = currentMonth == 1 ? currentYear - 1 : currentYear;
        var prevPayslip = await _payslipRepo.GetPayslipByEmployeeAndPeriodAsync(employeeId, prevMonth, prevYear);

        // 3. Lấy dữ liệu chấm công thực tế của kỳ này
        var attendances = (await _attendanceRepo.GetAttendancesByEmployeeAsync(employeeId, currentMonth, currentYear)).ToList();
        int absentDays = attendances.Count(a => a.ActualWorkDays == 0.0);
        int totalLateMinutes = attendances.Sum(a => a.LateMinutes);

        // 4. Xây dựng Ngữ Cảnh Dữ Liệu (Prompt Context) cho IChatClient
        var contextBuilder = new StringBuilder();
        contextBuilder.AppendLine("DƯỚI ĐÂY LÀ DỮ LIỆU ĐỐI SOÁT PHIẾU LƯƠNG CỦA NHÂN VIÊN:");
        contextBuilder.AppendLine($"- Nhân viên: {employee.FullName} (Mã: {employee.EmployeeCode}) - Vị trí: {employee.Position}");
        contextBuilder.AppendLine($"- Kỳ lương hiện tại: Tháng {currentMonth}/{currentYear}");
        contextBuilder.AppendLine($"  + Công chuẩn: {currentPayslip.StandardWorkDays} ngày | Công thực tế: {currentPayslip.ActualWorkDays} ngày");
        contextBuilder.AppendLine($"  + Giờ làm thêm OT: {currentPayslip.OvertimeHours} giờ");
        contextBuilder.AppendLine($"  + Số ngày nghỉ vắng: {absentDays} ngày | Tổng phút đi trễ: {totalLateMinutes} phút");
        contextBuilder.AppendLine($"  + Lương cơ bản: {currentPayslip.BasicSalary:N0} đ | Phụ cấp: {currentPayslip.Allowance:N0} đ");
        contextBuilder.AppendLine($"  + Tổng thu nhập (Gross): {currentPayslip.GrossSalary:N0} đ");
        contextBuilder.AppendLine($"  + Tổng khấu trừ: {currentPayslip.TotalDeductions:N0} đ");
        contextBuilder.AppendLine($"  + Lương thực nhận (Net): {currentPayslip.NetSalary:N0} đ");

        if (prevPayslip != null)
        {
            decimal diffNet = currentPayslip.NetSalary - prevPayslip.NetSalary;
            double diffWorkDays = currentPayslip.ActualWorkDays - prevPayslip.ActualWorkDays;
            double diffOt = currentPayslip.OvertimeHours - prevPayslip.OvertimeHours;

            contextBuilder.AppendLine($"- Kỳ lương trước đó: Tháng {prevMonth}/{prevYear}");
            contextBuilder.AppendLine($"  + Lương thực nhận trước: {prevPayslip.NetSalary:N0} đ (Chênh lệch: {(diffNet >= 0 ? "+" : "")}{diffNet:N0} đ)");
            contextBuilder.AppendLine($"  + Chênh lệch ngày công: {(diffWorkDays >= 0 ? "+" : "")}{diffWorkDays:N1} ngày");
            contextBuilder.AppendLine($"  + Chênh lệch giờ OT: {(diffOt >= 0 ? "+" : "")}{diffOt:N1} giờ");
        }

        contextBuilder.AppendLine("- Chi tiết các khoản trong phiếu lương hiện tại:");
        foreach (var line in currentPayslip.Lines)
        {
            contextBuilder.AppendLine($"  * {line.ItemName} ({line.LineType}): {line.Amount:N0} đ. Diễn giải: {line.CalculationNote}");
        }

        // 5. Chuẩn bị Messages gửi tới IChatClient
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, $"Bạn là HR Copilot - Trợ Lý Nhân Sự AI chuyên nghiệp và tận tâm của công ty.\n" +
                                 $"Nhiệm vụ: Sử dụng dữ liệu đối soát bên dưới để giải thích chi tiết, minh bạch và dễ hiểu câu hỏi của nhân viên bằng tiếng Việt.\n" +
                                 $"Phong cách: Nhã nhặn, chính xác về số liệu, nêu rõ từng nguyên nhân (ngày công, giờ OT, phụ cấp, bảo hiểm, thuế).\n\n" +
                                 contextBuilder.ToString()),
            new(ChatRole.User, userQuestion)
        };

        // 6. Gọi IChatClient
        var response = await _chatClient.GetResponseAsync(messages);
        return response.Messages.FirstOrDefault()?.Text ?? "Không nhận được phản hồi từ AI.";
    }
}
