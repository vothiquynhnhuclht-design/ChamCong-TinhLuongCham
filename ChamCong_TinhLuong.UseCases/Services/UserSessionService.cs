using ChamCong_TinhLuong.CoreBusiness.Models;

namespace ChamCong_TinhLuong.UseCases.Services;

public enum AppUserRole
{
    AdminHr = 0,    // Kế toán / Nhân sự
    Employee = 1    // Nhân viên
}

/// <summary>
/// Scoped Session Service quản lý vai trò và ngữ cảnh người dùng đang đăng nhập
/// Phục vụ cơ chế Row-level security (Phân quyền theo dữ liệu)
/// </summary>
public class UserSessionService
{
    public event Action? OnChange;

    public AppUserRole CurrentRole { get; private set; } = AppUserRole.AdminHr;
    public int? CurrentEmployeeId { get; private set; } = 1; // Mặc định NV001 nếu là Nhân viên
    public string CurrentEmployeeCode { get; private set; } = "NV001";
    public string CurrentEmployeeName { get; private set; } = "Nguyễn Văn An";
    public string CurrentUserEmail { get; private set; } = "hr.admin@company.com";

    public bool IsAdminHr => CurrentRole == AppUserRole.AdminHr;
    public bool IsEmployee => CurrentRole == AppUserRole.Employee;

    public void SwitchToAdminHr()
    {
        CurrentRole = AppUserRole.AdminHr;
        CurrentUserEmail = "ketoan@company.com";
        OnChange?.Invoke();
    }

    public void SwitchToEmployee(Employee emp)
    {
        CurrentRole = AppUserRole.Employee;
        CurrentEmployeeId = emp.Id;
        CurrentEmployeeCode = emp.EmployeeCode;
        CurrentEmployeeName = emp.FullName;
        CurrentUserEmail = emp.Email;
        OnChange?.Invoke();
    }
}
