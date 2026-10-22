using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

/// <summary>
/// Thực thể ghi vết lịch sử gửi Email / Thông báo phân phối phiếu lương tự động đến nhân viên.
/// Phục vụ khâu hậu kiểm (Audit Trail) khi Khóa sổ kỳ lương và phát hành phiếu lương.
/// </summary>
public class NotificationLog
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? PayslipId { get; set; }
    public Payslip? Payslip { get; set; }

    public DateTime SentDate { get; set; } = DateTime.Now;
    public NotificationType Type { get; set; } = NotificationType.EmailPayslip;
    public NotificationStatus Status { get; set; } = NotificationStatus.Success;

    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
