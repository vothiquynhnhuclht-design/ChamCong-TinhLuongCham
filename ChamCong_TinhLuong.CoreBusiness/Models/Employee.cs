using System.ComponentModel.DataAnnotations;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã nhân viên.")]
    [StringLength(20, ErrorMessage = "Mã nhân viên tối đa 20 ký tự.")]
    public string EmployeeCode { get; set; } = string.Empty; // Mã nhân viên (duy nhất)

    [Required(ErrorMessage = "Vui lòng nhập họ và tên nhân viên.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 150 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và gồm 10-11 chữ số.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập chức vụ.")]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty; // Chức vụ

    public DateTime JoinDate { get; set; } = DateTime.Today;

    [Range(0, 20, ErrorMessage = "Số người phụ thuộc phải từ 0 đến 20.")]
    public int DependentCount { get; set; } = 0; // Số người phụ thuộc để tính giảm trừ gia cảnh

    public bool IsActive { get; set; } = true;

    // Foreign Key
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng ban.")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Navigation Properties
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<TimeAttendance> TimeAttendances { get; set; } = new List<TimeAttendance>();
    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    public AppUser? AppUser { get; set; }

    // Helper method to get the current active contract
    public Contract? GetCurrentContract()
    {
        return Contracts.FirstOrDefault(c => c.IsCurrent);
    }
}
