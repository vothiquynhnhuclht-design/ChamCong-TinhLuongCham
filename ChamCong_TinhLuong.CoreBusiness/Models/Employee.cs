namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty; // Mã nhân viên (duy nhất)
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty; // Chức vụ
    public DateTime JoinDate { get; set; } = DateTime.Today;
    public int DependentCount { get; set; } = 0; // Số người phụ thuộc để tính giảm trừ gia cảnh
    public bool IsActive { get; set; } = true;

    // Foreign Key
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Navigation Properties
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<TimeAttendance> TimeAttendances { get; set; } = new List<TimeAttendance>();
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();

    // Helper method to get the current active contract
    public Contract? GetCurrentContract()
    {
        return Contracts.FirstOrDefault(c => c.IsCurrent);
    }
}
