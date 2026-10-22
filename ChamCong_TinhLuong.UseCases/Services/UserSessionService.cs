using System.Security.Cryptography;
using System.Text;
using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.UseCases.Services;

public enum AppUserRole
{
    AdminHr = 0,    // Kế toán / Quản trị Nhân sự (Toàn quyền quản trị)
    Employee = 1    // Nhân viên (Bảo mật cấp dòng - Row-level Security: chỉ xem dữ liệu chính mình)
}

/// <summary>
/// Scoped Session Service quản lý xác thực và phân quyền theo tài khoản đăng nhập trực tiếp từ SQL Server.
/// Kết nối trực tiếp bảng AppUsers và Employees, loại bỏ hoàn toàn mã gán dữ liệu cứng thủ công.
/// </summary>
public class UserSessionService
{
    private readonly IAppUserRepository _userRepo;
    private readonly IEmployeeRepository _employeeRepo;

    public UserSessionService(IAppUserRepository userRepo, IEmployeeRepository employeeRepo)
    {
        _userRepo = userRepo;
        _employeeRepo = employeeRepo;
    }

    public event Action? OnChange;

    public bool IsAuthenticated { get; private set; } = false;
    public AppUserRole CurrentRole { get; private set; } = AppUserRole.AdminHr;
    public int? CurrentEmployeeId { get; private set; } = null;
    public string CurrentEmployeeCode { get; private set; } = string.Empty;
    public string CurrentEmployeeName { get; private set; } = string.Empty;
    public string CurrentUserEmail { get; private set; } = string.Empty;

    public bool IsAdminHr => IsAuthenticated && CurrentRole == AppUserRole.AdminHr;
    public bool IsEmployee => IsAuthenticated && CurrentRole == AppUserRole.Employee;

    /// <summary>
    /// Đăng nhập hệ thống đồng bộ trực tiếp từ SQL Server (Bảng AppUsers và Employees).
    /// Hỗ trợ tìm kiếm theo Username, Mã nhân viên (EmployeeCode) hoặc Email.
    /// </summary>
    public async Task<(bool Success, string Message)> LoginAsync(string usernameOrCode, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrCode))
        {
            return (false, "Vui lòng nhập tên đăng nhập, mã nhân viên hoặc email.");
        }

        usernameOrCode = usernameOrCode.Trim();
        password = string.IsNullOrWhiteSpace(password) ? "123456" : password.Trim();

        try
        {
            // 1. Truy vấn tài khoản trực tiếp từ SQL Server
            var user = await _userRepo.GetByUsernameAsync(usernameOrCode);

            // 2. Nếu chưa có AppUser nhưng đã có Employee trong SQL Server, hỗ trợ tự động liên kết
            if (user == null)
            {
                var employees = await _employeeRepo.GetAllEmployeesAsync(includeInactive: false);
                var emp = employees.FirstOrDefault(e =>
                    e.EmployeeCode.Equals(usernameOrCode, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Equals(usernameOrCode, StringComparison.OrdinalIgnoreCase));

                if (emp != null)
                {
                    // Tạo tài khoản tự động trong SQL Server cho nhân viên
                    var hash = HashPassword(password);
                    user = new AppUser
                    {
                        Username = emp.EmployeeCode.ToLower(),
                        PasswordHash = hash,
                        Role = (emp.DepartmentId == 2) ? UserRole.HR : UserRole.Employee,
                        EmployeeId = emp.Id,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    await _userRepo.AddUserAsync(user);
                    user.Employee = emp;
                }
            }

            if (user == null)
            {
                return (false, $"Tài khoản hoặc mã nhân viên '{usernameOrCode}' không tồn tại trong cơ sở dữ liệu SQL Server.");
            }

            if (!user.IsActive)
            {
                return (false, "Tài khoản này hiện đang bị khóa hoặc ngưng hoạt động.");
            }

            // 3. Đối chiếu mật khẩu (Hỗ trợ cả SHA-256 hash từ CSDL và plain-text dự phòng)
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Mật khẩu không chính xác. (Mật khẩu mặc định: 123456)");
            }

            // 4. Ghi nhận thời điểm đăng nhập thành công vào SQL Server
            user.LastLoginAt = DateTime.Now;
            await _userRepo.UpdateUserAsync(user);

            // 5. Cập nhật trạng thái phiên làm việc theo dữ liệu SQL Server
            IsAuthenticated = true;
            CurrentRole = (user.Role == UserRole.Employee) ? AppUserRole.Employee : AppUserRole.AdminHr;
            CurrentEmployeeId = user.EmployeeId;
            CurrentEmployeeCode = user.Employee?.EmployeeCode ?? user.Username.ToUpper();
            CurrentEmployeeName = user.Employee?.FullName ?? (user.Role == UserRole.Admin ? "Quản trị viên Hệ thống" : (user.Role == UserRole.HR ? "Kế toán / Nhân sự" : user.Username));
            CurrentUserEmail = user.Employee?.Email ?? $"{user.Username}@company.com";

            OnChange?.Invoke();
            return (true, $"Đăng nhập thành công! Xin chào {CurrentEmployeeName}.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối cơ sở dữ liệu SQL Server: {ex.Message}");
        }
    }

    /// <summary>
    /// Đăng xuất khỏi hệ thống và xóa thông tin phiên làm việc
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

    /// <summary>
    /// Kiểm tra mật khẩu đối chiếu với chuỗi mã hóa trong SQL Server
    /// </summary>
    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash)) return false;

        // So khớp trực tiếp chuỗi thường
        if (string.Equals(inputPassword, storedHash, StringComparison.Ordinal))
            return true;

        // So khớp băm SHA-256
        var inputHash = HashPassword(inputPassword);
        return string.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Băm chuỗi mật khẩu bằng thuật toán SHA-256
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
