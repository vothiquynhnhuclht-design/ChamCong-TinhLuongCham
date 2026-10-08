using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.DTOs;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.UseCases.Services;

/// <summary>
/// Service Scoped quản lý trạng thái bảng công nháp (Tương đương ShoppingCart trong eShop).
/// Dữ liệu được lưu giữ trong phiên làm việc (Session/Scoped), cho phép HR nạp danh sách,
/// hiệu chỉnh ngày công, xem trước và bấm "Chốt duyệt" mới ghi xuống Database.
/// </summary>
public class AttendanceDraftService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IUnitOfWork _unitOfWork;

    public List<AttendanceDraftItemDto> DraftItems { get; private set; } = new();
    public int TargetMonth { get; private set; } = DateTime.Today.Month;
    public int TargetYear { get; private set; } = DateTime.Today.Year;
    public event Action? OnChange;

    public AttendanceDraftService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Khởi tạo bảng công nháp cho tất cả nhân viên trong tháng
    /// </summary>
    public async Task InitializeDraftForMonthAsync(int month, int year)
    {
        TargetMonth = month;
        TargetYear = year;
        DraftItems.Clear();

        var employees = await _employeeRepo.GetAllEmployeesAsync();
        var existingAttendances = (await _attendanceRepo.GetAttendancesByMonthAsync(month, year)).ToList();

        // Nếu đã có dữ liệu trong DB -> Load vào draft để chỉnh sửa
        if (existingAttendances.Any())
        {
            foreach (var att in existingAttendances)
            {
                var emp = employees.FirstOrDefault(e => e.Id == att.EmployeeId);
                DraftItems.Add(new AttendanceDraftItemDto
                {
                    EmployeeId = att.EmployeeId,
                    EmployeeCode = emp?.EmployeeCode ?? "",
                    FullName = emp?.FullName ?? "",
                    DepartmentName = emp?.Department?.DepartmentName ?? "",
                    WorkDate = att.WorkDate,
                    CheckIn = att.CheckIn,
                    CheckOut = att.CheckOut,
                    ActualWorkDays = att.ActualWorkDays,
                    OvertimeHours = att.OvertimeHours,
                    LateMinutes = att.LateMinutes,
                    Status = att.Status,
                    Note = att.Note
                });
            }
        }
        else
        {
            // Tự động sinh khung ngày công chuẩn (Thứ 2 - Thứ 6)
            int daysInMonth = DateTime.DaysInMonth(year, month);
            foreach (var emp in employees)
            {
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                    DraftItems.Add(new AttendanceDraftItemDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        FullName = emp.FullName,
                        DepartmentName = emp.Department?.DepartmentName ?? "",
                        WorkDate = date,
                        CheckIn = new TimeSpan(8, 0, 0),
                        CheckOut = new TimeSpan(17, 30, 0),
                        ActualWorkDays = 1.0,
                        OvertimeHours = 0.0,
                        LateMinutes = 0,
                        Status = AttendanceStatus.Present
                    });
                }
            }
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Cập nhật nhanh số công hoặc giờ OT cho 1 dòng trong bảng nháp
    /// </summary>
    public void UpdateDraftItem(int employeeId, DateTime workDate, double actualWorkDays, double otHours, AttendanceStatus status, string? note = null)
    {
        var item = DraftItems.FirstOrDefault(d => d.EmployeeId == employeeId && d.WorkDate.Date == workDate.Date);
        if (item != null)
        {
            item.ActualWorkDays = actualWorkDays;
            item.OvertimeHours = otHours;
            item.Status = status;
            item.Note = note;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Chốt duyệt toàn bộ bảng công nháp -> Lưu chính thức vào Database
    /// </summary>
    public async Task<int> CommitDraftToDatabaseAsync()
    {
        if (!DraftItems.Any()) return 0;

        var entities = DraftItems.Select(d => new TimeAttendance
        {
            EmployeeId = d.EmployeeId,
            WorkDate = d.WorkDate,
            CheckIn = d.CheckIn,
            CheckOut = d.CheckOut,
            ActualWorkDays = d.ActualWorkDays,
            OvertimeHours = d.OvertimeHours,
            LateMinutes = d.LateMinutes,
            Status = d.Status,
            Note = d.Note
        }).ToList();

        await _attendanceRepo.AddRangeAttendanceAsync(entities);
        await _unitOfWork.SaveChangesAsync();

        int savedCount = entities.Count;
        NotifyStateChanged();
        return savedCount;
    }

    public void ClearDraft()
    {
        DraftItems.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
