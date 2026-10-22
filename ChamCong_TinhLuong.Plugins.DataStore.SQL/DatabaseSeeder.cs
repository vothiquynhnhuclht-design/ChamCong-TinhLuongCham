using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;
using Microsoft.EntityFrameworkCore;

namespace ChamCong_TinhLuong.Plugins.DataStore.SQL;

/// <summary>
/// Trình khởi tạo kết nối cơ sở dữ liệu SQL Server.
/// Đã loại bỏ hoàn toàn các đoạn code tạo dữ liệu giả lập thủ công trong C#.
/// Toàn bộ dữ liệu nghiệp vụ (Phòng ban, Ca làm việc, Tỷ lệ trích nộp, Nhân viên, Hợp đồng, Chấm công, Bảng lương)
/// được quản lý và lưu trữ trực tiếp trên SQL Server qua kịch bản ChamCong_TinhLuong_Database.sql.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ChamCongDbContext context)
    {
        // 1. Kiểm tra và đảm bảo lược đồ cơ sở dữ liệu SQL Server đã được khởi tạo
        await context.Database.EnsureCreatedAsync();

        // 2. Khởi tạo tài khoản Quản trị tối thiểu (Bootstrap Admin) nếu bảng AppUsers hoàn toàn trống
        // Giúp kiểm thử viên luôn có tài khoản đăng nhập ngay trên CSDL mới trước khi nạp SQL Script
        if (!await context.AppUsers.AnyAsync())
        {
            // SHA-256 của mật khẩu "123456"
            const string defaultPasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92";

            await context.AppUsers.AddRangeAsync(
                new AppUser
                {
                    Username = "admin",
                    PasswordHash = defaultPasswordHash,
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new AppUser
                {
                    Username = "ketoan",
                    PasswordHash = defaultPasswordHash,
                    Role = UserRole.HR,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
