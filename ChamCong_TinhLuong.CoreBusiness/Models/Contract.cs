using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class Contract
{
    public int Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty; // Số hợp đồng lao động
    public ContractType ContractType { get; set; } = ContractType.FullTime;
    
    // Lương và phụ cấp
    public decimal BasicSalary { get; set; } // Lương cơ bản (dùng tính công chuẩn và đóng bảo hiểm)
    public decimal Allowance { get; set; } // Phụ cấp cố định (trách nhiệm, ăn trưa, xăng xe)
    public decimal InsuranceSalary { get; set; } // Mức lương làm căn cứ đóng BHXH/BHYT/BHTN

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; } = true; // Hợp đồng đang có hiệu lực hiện tại

    // Foreign Key
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
