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

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double NumberOfDays { get; set; } = 1.0;
    
    public string Reason { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; } = LeaveType.Paid;
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    // Thông tin phê duyệt
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
