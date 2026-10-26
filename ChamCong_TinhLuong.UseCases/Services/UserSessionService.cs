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
    public AppUserRole CurrentRole { get; private set; } = AppUserRole.Employee;
    public int? CurrentEmployeeId { get; private set; } = null;
    public string CurrentEmployeeCode { get; private set; } = string.Empty;
    public string CurrentEmployeeName { get; private set; } = string.Empty;
    public string CurrentUserEmail { get; private set; } = string.Empty;

    public bool IsAdminHr => IsAuthenticated && CurrentRole == AppUserRole.AdminHr;
    public bool IsEmployee => IsAuthenticated && CurrentRole == AppUserRole.Employee;

    /// <summary>
    /// Tên chức danh phân quyền hiển thị trên giao diện (Quản trị viên, Nhân sự - Kế toán, hoặc Nhân viên)
    /// </summary>
    public string CurrentRoleTitle
    {
        get
        {
            if (!IsAuthenticated) return string.Empty;
            if (CurrentRole == AppUserRole.AdminHr)
            {
                if (CurrentEmployeeCode.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    CurrentUserEmail.StartsWith("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return "Quản trị viên";
                }
                return "Nhân sự - Kế toán";
            }
            return "Nhân viên";
        }
    }

    /// <summary>
    /// Kiểm tra người dùng có thuộc bộ phận Nhân sự - Kế toán hoặc Quản trị hệ thống hay không.
    /// Theo quy tắc nghiệp vụ: Chỉ Nhân sự - Kế toán mới được cấp quyền AdminHr (Toàn quyền quản trị/nghiệp vụ),
    /// toàn bộ các phòng ban/nhân viên khác đều là Nhân viên (Employee).
    /// </summary>
    public static bool IsHrOrAccountingUser(AppUser user)
    {
        // 1. Tài khoản quản trị hệ thống không gắn nhân viên cụ thể (admin, ketoan)
        if (user.Employee == null)
        {
            return user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)
                || user.Username.Equals("ketoan", StringComparison.OrdinalIgnoreCase)
                || user.Role == UserRole.Admin
                || user.Role == UserRole.HR;
        }

        var emp = user.Employee;
        var deptCode = emp.Department?.DepartmentCode ?? string.Empty;
        var deptName = emp.Department?.DepartmentName ?? string.Empty;
        var position = emp.Position ?? string.Empty;

        // 2. Thuộc phòng Nhân sự - Kế toán (DepartmentId == 2 hoặc mã phòng HR hoặc tên phòng chứa Nhân sự / Kế toán)
        bool isDeptHrOrAcc = emp.DepartmentId == 2
            || deptCode.Equals("HR", StringComparison.OrdinalIgnoreCase)
            || deptName.Contains("Nhân sự", StringComparison.OrdinalIgnoreCase)
            || deptName.Contains("Kế toán", StringComparison.OrdinalIgnoreCase)
            || deptName.Contains("HR", StringComparison.OrdinalIgnoreCase);

        // 3. Hoặc có chức danh / vị trí công tác là Nhân sự / Kế toán
        bool isPositionHrOrAcc = position.Contains("Nhân sự", StringComparison.OrdinalIgnoreCase)
            || position.Contains("Kế toán", StringComparison.OrdinalIgnoreCase)
            || position.Contains("HR", StringComparison.OrdinalIgnoreCase)
            || position.Contains("Accountant", StringComparison.OrdinalIgnoreCase);

        return isDeptHrOrAcc || isPositionHrOrAcc;
    }

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
                    bool isHr = emp.DepartmentId == 2
                        || (emp.Department?.DepartmentCode.Equals("HR", StringComparison.OrdinalIgnoreCase) == true)
                        || (emp.Department?.DepartmentName.Contains("Nhân sự", StringComparison.OrdinalIgnoreCase) == true)
                        || (emp.Department?.DepartmentName.Contains("Kế toán", StringComparison.OrdinalIgnoreCase) == true)
                        || (emp.Position?.Contains("Nhân sự", StringComparison.OrdinalIgnoreCase) == true)
                        || (emp.Position?.Contains("Kế toán", StringComparison.OrdinalIgnoreCase) == true);

                    // Tạo tài khoản tự động trong SQL Server cho nhân viên
                    var hash = HashPassword(password);
                    user = new AppUser
                    {
                        Username = emp.EmployeeCode.ToLower(),
                        PasswordHash = hash,
                        Role = isHr ? UserRole.HR : UserRole.Employee,
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

            // 5. Cập nhật phân quyền chuẩn nghiệp vụ:
            // Chỉ nhân sự - kế toán (hoặc admin hệ thống) mới được phân quyền quản trị (AdminHr), còn lại đều là Nhân viên (Employee).
            bool isHrOrAdmin = IsHrOrAccountingUser(user);

            var expectedRole = isHrOrAdmin
                ? (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? UserRole.Admin : UserRole.HR)
                : UserRole.Employee;

            if (user.Role != expectedRole)
            {
                user.Role = expectedRole;
            }
            await _userRepo.UpdateUserAsync(user);

            IsAuthenticated = true;
            CurrentRole = isHrOrAdmin ? AppUserRole.AdminHr : AppUserRole.Employee;
            CurrentEmployeeId = user.EmployeeId;
            CurrentEmployeeCode = user.Employee?.EmployeeCode ?? user.Username.ToUpper();
            CurrentEmployeeName = user.Employee?.FullName ?? (isHrOrAdmin
                ? (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Quản trị viên Hệ thống" : "Nhân sự - Kế toán")
                : user.Username);
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
        CurrentRole = AppUserRole.Employee;
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
