using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.UseCases.Services;

public enum AppUserRole
{
    AdminHr = 0,    // Kế toán / Nhân sự (Toàn quyền quản trị)
    Employee = 1    // Nhân viên (Bảo mật cấp dòng - chỉ xem dữ liệu chính mình)
}

/// <summary>
/// Scoped Session Service quản lý xác thực và phân quyền theo tài khoản đăng nhập
/// Phục vụ cơ chế Authentication & Row-level Security (Phân quyền theo dữ liệu)
/// </summary>
public class UserSessionService
{
    private readonly IEmployeeRepository _employeeRepo;

    public UserSessionService(IEmployeeRepository employeeRepo)
    {
        _employeeRepo = employeeRepo;
    }

    public event Action? OnChange;

    public bool IsAuthenticated { get; private set; } = false; // Mặc định chưa đăng nhập
    public AppUserRole CurrentRole { get; private set; } = AppUserRole.AdminHr;
    public int? CurrentEmployeeId { get; private set; } = null;
    public string CurrentEmployeeCode { get; private set; } = string.Empty;
    public string CurrentEmployeeName { get; private set; } = string.Empty;
    public string CurrentUserEmail { get; private set; } = string.Empty;

    public bool IsAdminHr => IsAuthenticated && CurrentRole == AppUserRole.AdminHr;
    public bool IsEmployee => IsAuthenticated && CurrentRole == AppUserRole.Employee;

    /// <summary>
    /// Đăng nhập hệ thống theo tài khoản và mật khẩu
    /// </summary>
    public async Task<(bool Success, string Message)> LoginAsync(string usernameOrCode, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrCode))
        {
            return (false, "Vui lòng nhập tên đăng nhập hoặc mã nhân viên.");
        }

        usernameOrCode = usernameOrCode.Trim();

        // 1. Kiểm tra tài khoản Kế toán / Quản trị Nhân sự
        if (usernameOrCode.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
            usernameOrCode.Equals("ketoan", StringComparison.OrdinalIgnoreCase) ||
            usernameOrCode.Equals("ketoan@company.com", StringComparison.OrdinalIgnoreCase))
        {
            if (password == "123456" || password == "admin")
            {
                IsAuthenticated = true;
                CurrentRole = AppUserRole.AdminHr;
                CurrentEmployeeId = null;
                CurrentEmployeeCode = "ADMIN";
                CurrentEmployeeName = "Võ Thị Quỳnh Như (Kế toán Trưởng)";
                CurrentUserEmail = "ketoan@company.com";
                OnChange?.Invoke();
                return (true, "Đăng nhập thành công với vai trò Kế toán / Quản trị Nhân sự.");
            }
            return (false, "Mật khẩu không chính xác (Mặc định: 123456).");
        }

        // 2. Kiểm tra tài khoản Nhân viên (qua Mã NV hoặc Email)
        var employees = await _employeeRepo.GetAllEmployeesAsync(includeInactive: false);
        var emp = employees.FirstOrDefault(e => 
            e.EmployeeCode.Equals(usernameOrCode, StringComparison.OrdinalIgnoreCase) ||
            e.Email.Equals(usernameOrCode, StringComparison.OrdinalIgnoreCase));

        if (emp != null)
        {
            if (password == "123456" || password == emp.EmployeeCode.ToLower())
            {
                IsAuthenticated = true;
                CurrentRole = AppUserRole.Employee;
                CurrentEmployeeId = emp.Id;
                CurrentEmployeeCode = emp.EmployeeCode;
                CurrentEmployeeName = emp.FullName;
                CurrentUserEmail = emp.Email;
                OnChange?.Invoke();
                return (true, $"Đăng nhập thành công. Chào mừng {emp.FullName}!");
            }
            return (false, "Mật khẩu không chính xác (Mặc định: 123456).");
        }

        return (false, $"Không tìm thấy tài khoản '{usernameOrCode}'. Vui lòng kiểm tra lại.");
    }

    /// <summary>
    /// Đăng xuất khỏi hệ thống
    /// </summary>
    public void Logout()
    {
        IsAuthenticated = false;
        CurrentRole = AppUserRole.AdminHr;
        CurrentEmployeeId = null;
        CurrentEmployeeCode = string.Empty;
        CurrentEmployeeName = string.Empty;
        CurrentUserEmail = string.Empty;
        OnChange?.Invoke();
    }
}
