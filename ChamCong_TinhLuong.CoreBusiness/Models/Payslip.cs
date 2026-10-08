using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

/// <summary>
/// Bảng lương Header (tương đương Order Header)
/// Đại diện cho phiếu lương tổng của 1 nhân viên trong 1 kỳ tháng/năm.
/// </summary>
public class Payslip
{
    public int Id { get; set; }

    // Foreign Key
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // Kỳ tính lương
    public int Month { get; set; }
    public int Year { get; set; }

    // Thông tin công tháng
    public double StandardWorkDays { get; set; } = 22.0; // Ngày công chuẩn trong tháng
    public double ActualWorkDays { get; set; } = 0.0;   // Tổng ngày công thực tế
    public double OvertimeHours { get; set; } = 0.0;    // Tổng giờ tăng ca

    // Mức lương cơ sở tại thời điểm tính
    public decimal BasicSalary { get; set; }            // Lương cơ bản theo hợp đồng
    public decimal Allowance { get; set; }              // Phụ cấp cố định

    // Tổng hợp tài chính
    public decimal GrossSalary { get; set; }            // Tổng thu nhập (Lương công + OT + Phụ cấp + Thưởng...)
    public decimal TotalDeductions { get; set; }        // Tổng các khoản trừ (Bảo hiểm + Thuế + Phạt...)
    public decimal NetSalary { get; set; }              // Lương thực nhận = GrossSalary - TotalDeductions

    // Trạng thái khóa sổ & duyệt
    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LockedAt { get; set; }             // Thời điểm khóa sổ
    public string? LockedBy { get; set; }               // Người thực hiện khóa sổ

    // Chi tiết Header-Line (tương đương OrderLineItem)
    public ICollection<PayslipLine> Lines { get; set; } = new List<PayslipLine>();
}
