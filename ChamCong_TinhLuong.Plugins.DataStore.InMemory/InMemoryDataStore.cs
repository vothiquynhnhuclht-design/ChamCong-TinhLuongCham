using ChamCong_TinhLuong.CoreBusiness.Models;

namespace ChamCong_TinhLuong.Plugins.DataStore.InMemory;

/// <summary>
/// Lưu trữ dữ liệu trong bộ nhớ (In-Memory) thuần túy phục vụ kiểm thử (Unit Testing / Mock).
/// Đã loại bỏ hoàn toàn các đoạn code tạo dữ liệu giả lập thủ công.
/// Toàn bộ dữ liệu nghiệp vụ của hệ thống được quản lý và lưu trữ trực tiếp trên SQL Server
/// thông qua ChamCongDbContext và file kịch bản ChamCong_TinhLuong_Database.sql.
/// </summary>
public class InMemoryDataStore
{
    public List<Department> Departments { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<Contract> Contracts { get; set; } = new();
    public List<WorkShift> WorkShifts { get; set; } = new();
    public List<DeductionRate> DeductionRates { get; set; } = new();
    public List<TimeAttendance> TimeAttendances { get; set; } = new();
    public List<Payslip> Payslips { get; set; } = new();
    public List<PayrollPeriod> PayrollPeriods { get; set; } = new();
    public List<AppUser> AppUsers { get; set; } = new();
    public List<LeaveRequest> LeaveRequests { get; set; } = new();
    public List<NotificationLog> NotificationLogs { get; set; } = new();

    public InMemoryDataStore()
    {
        // Khởi tạo rỗng, không tạo dữ liệu giả lập thủ công trong mã nguồn.
        // Dữ liệu nghiệp vụ thực tế hoàn toàn đồng bộ từ SQL Server.
    }

    /// <summary>
    /// Xóa sạch dữ liệu trong bộ nhớ (hỗ trợ reset trạng thái khi chạy test)
    /// </summary>
    public void ClearAll()
    {
        Departments.Clear();
        Employees.Clear();
        Contracts.Clear();
        WorkShifts.Clear();
        DeductionRates.Clear();
        TimeAttendances.Clear();
        Payslips.Clear();
        PayrollPeriods.Clear();
        AppUsers.Clear();
        LeaveRequests.Clear();
        NotificationLogs.Clear();
    }
}
