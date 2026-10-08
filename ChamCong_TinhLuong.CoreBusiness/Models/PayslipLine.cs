using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

/// <summary>
/// Chi tiết dòng phiếu lương (tương đương OrderLineItem)
/// Phân tách rõ ràng từng khoản cộng (Earning) và trừ (Deduction)
/// </summary>
public class PayslipLine
{
    public int Id { get; set; }

    // Foreign Key về Header
    public int PayslipId { get; set; }
    public Payslip? Payslip { get; set; }

    // Loại dòng: Earning (Thu nhập) hoặc Deduction (Khấu trừ)
    public PayslipLineType LineType { get; set; }

    // Mã khoản mục: WORK_SALARY, OT_SALARY, ALLOWANCE_LUNCH, DEDUCTION_BHXH, DEDUCTION_BHYT, DEDUCTION_BHTN, DEDUCTION_TAX, BONUS, PENALTY...
    public string ItemCode { get; set; } = string.Empty;

    // Tên khoản mục hiển thị trên phiếu lương
    public string ItemName { get; set; } = string.Empty;

    // Số tiền (Dương với Earning, hoặc hiển thị số tiền trừ với Deduction)
    public decimal Amount { get; set; }

    // Tỷ lệ hoặc hệ số áp dụng (nếu có, ví dụ 8% BHXH hoặc 1.5 OT)
    public decimal? RateApplied { get; set; }

    // Ghi chú chi tiết phép tính (Ví dụ: "(10.000.000 / 22) * 20 ngày")
    public string? CalculationNote { get; set; }
}
