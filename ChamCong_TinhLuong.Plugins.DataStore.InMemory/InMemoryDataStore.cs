using ChamCong_TinhLuong.CoreBusiness.Models;

namespace ChamCong_TinhLuong.Plugins.DataStore.InMemory;


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
