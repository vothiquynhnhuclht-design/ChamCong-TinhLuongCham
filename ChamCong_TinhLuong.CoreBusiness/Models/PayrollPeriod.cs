using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

/// <summary>
/// Entity Kỳ Tính Lương (PayrollPeriod)
/// Quản lý chu kỳ tính lương từng tháng, ngày công chuẩn và trạng thái khóa sổ
/// </summary>
public class PayrollPeriod
{
    public int Id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string PeriodName => $"Tháng {Month:D2}/{Year}";
    public decimal StandardWorkDays { get; set; } = 22m;
    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;
    public DateTime? LockedAt { get; set; }
    public string? LockedBy { get; set; }
    public string? Note { get; set; }

    // Relationship 1-N: 1 kỳ tính lương chứa nhiều phiếu lương của nhân viên
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
