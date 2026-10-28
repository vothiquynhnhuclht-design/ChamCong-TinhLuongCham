using System.ComponentModel.DataAnnotations;
using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

/// <summary>
/// Thực thể đơn xin nghỉ phép của nhân viên (Leave Request).
/// Là mắt xích trọng yếu để Động cơ tính lương (CalculatePayrollUseCase) tự động
/// cộng gộp ngày nghỉ phép có hưởng lương (LeavePaid) vào tổng công tháng.
/// </summary>
public class LeaveRequest
{
    public int Id { get; set; }
    
    // Khóa ngoại nối về nhân viên gửi đơn
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu nghỉ.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc nghỉ.")]
    public DateTime EndDate { get; set; }

    [Range(0.5, 365, ErrorMessage = "Số ngày nghỉ phải từ 0.5 ngày trở lên.")]
    public double NumberOfDays { get; set; } = 1.0;
    
    [Required(ErrorMessage = "Vui lòng nhập lý do xin nghỉ phép.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Lý do xin nghỉ phải từ 5 đến 500 ký tự.")]
    public string Reason { get; set; } = string.Empty;

    public LeaveType LeaveType { get; set; } = LeaveType.Paid;
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    // Thông tin phê duyệt
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
