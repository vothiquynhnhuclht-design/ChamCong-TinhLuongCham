using System.ComponentModel.DataAnnotations;
using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class Contract
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số hợp đồng.")]
    [StringLength(50, ErrorMessage = "Số hợp đồng tối đa 50 ký tự.")]
    public string ContractNumber { get; set; } = string.Empty; // Số hợp đồng lao động

    public ContractType ContractType { get; set; } = ContractType.FullTime;
    
    // Lương và phụ cấp
    [Range(1_000_000, 500_000_000, ErrorMessage = "Lương cơ bản phải từ 1.000.000 đ đến 500.000.000 đ.")]
    public decimal BasicSalary { get; set; } // Lương cơ bản (dùng tính công chuẩn và đóng bảo hiểm)

    [Range(0, 200_000_000, ErrorMessage = "Phụ cấp không hợp lệ.")]
    public decimal Allowance { get; set; } // Phụ cấp cố định (trách nhiệm, ăn trưa, xăng xe)

    [Range(1_000_000, 500_000_000, ErrorMessage = "Lương đóng BHXH phải từ 1.000.000 đ đến 500.000.000 đ.")]
    public decimal InsuranceSalary { get; set; } // Mức lương làm căn cứ đóng BHXH/BHYT/BHTN

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; } = true; // Hợp đồng đang có hiệu lực hiện tại

    // Foreign Key
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
