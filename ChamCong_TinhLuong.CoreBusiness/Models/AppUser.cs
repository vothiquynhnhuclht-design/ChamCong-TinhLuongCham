using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

/// <summary>
/// Thực thể người dùng hệ thống phục vụ Xác thực (Authentication) & Phân quyền (Authorization).
/// Thay thế hoàn toàn cơ chế mật khẩu tĩnh hardcode, hỗ trợ băm mật khẩu bảo mật (SHA-256 / BCrypt).
/// </summary>
public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    
    // Khóa ngoại liên kết với Employee (Nullable: Tài khoản Admin hệ thống có thể không gắn với nhân viên)
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
}
