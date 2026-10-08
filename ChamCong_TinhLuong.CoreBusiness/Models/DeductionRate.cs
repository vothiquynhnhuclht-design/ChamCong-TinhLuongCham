namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class DeductionRate
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // BHXH, BHYT, BHTN, TNCN_BAN_THAN, TNCN_PHU_THUOC, CONG_DOAN
    public string Name { get; set; } = string.Empty; // Tên diễn giải: Bảo hiểm xã hội (8%), Giảm trừ bản thân (11tr)
    
    // Tỷ lệ trích phần trăm (ví dụ: 0.08 cho 8%, 0.015 cho 1.5%, 0.01 cho 1%)
    public decimal RatePercentage { get; set; } = 0m;

    // Số tiền cố định nếu có (ví dụ: Giảm trừ gia cảnh bản thân 11.000.000 VNĐ, phụ thuộc 4.400.000 VNĐ)
    public decimal FlatAmount { get; set; } = 0m;

    // Áp dụng tính trên: "InsuranceSalary", "GrossSalary", "TaxableIncome"
    public string ApplyOn { get; set; } = "InsuranceSalary";

    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
