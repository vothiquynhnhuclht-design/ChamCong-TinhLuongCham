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
                    Id = att.Id,
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
                    Note = att.Note,
                    PhotoPath = att.PhotoPath,
                    CheckOutPhotoPath = att.CheckOutPhotoPath,
                    WorkShiftId = att.WorkShiftId,
                    ShiftName = att.WorkShift?.ShiftName
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
    /// Gửi yêu cầu chấm công quét khuôn mặt từ nhân viên (Trạng thái PendingApproval - Chờ admin duyệt)
    /// </summary>
    public async Task<TimeAttendance> SubmitFaceAttendanceAsync(
        int employeeId, 
        DateTime workDate, 
        TimeSpan time, 
        bool isCheckIn, 
        string? note = null,
        string? photoPath = null,
        int? workShiftId = null)
    {
        var existing = await _attendanceRepo.GetAttendanceByEmployeeAndDateAsync(employeeId, workDate);
        if (existing != null)
        {
            if (isCheckIn)
            {
                existing.CheckIn = time;
                if (!string.IsNullOrEmpty(photoPath)) existing.PhotoPath = photoPath;
                if (workShiftId.HasValue) existing.WorkShiftId = workShiftId.Value;
                if (time > new TimeSpan(8, 30, 0))
                {
                    existing.LateMinutes = (int)(time - new TimeSpan(8, 30, 0)).TotalMinutes;
                }
            }
            else
            {
                existing.CheckOut = time;
                if (!string.IsNullOrEmpty(photoPath)) existing.CheckOutPhotoPath = photoPath;
            }
            existing.Status = AttendanceStatus.PendingApproval;
            if (!string.IsNullOrEmpty(note)) existing.Note = note;
            await _attendanceRepo.UpdateAttendanceAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            await InitializeDraftForMonthAsync(TargetMonth, TargetYear);
            return existing;
        }

        var newAtt = new TimeAttendance
        {
            EmployeeId = employeeId,
            WorkShiftId = workShiftId ?? 1,
            WorkDate = workDate.Date,
            CheckIn = isCheckIn ? time : null,
            CheckOut = !isCheckIn ? time : null,
            PhotoPath = isCheckIn ? photoPath : null,
            CheckOutPhotoPath = !isCheckIn ? photoPath : null,
            ActualWorkDays = 1.0,
            LateMinutes = (isCheckIn && time > new TimeSpan(8, 30, 0)) ? (int)(time - new TimeSpan(8, 30, 0)).TotalMinutes : 0,
            Status = AttendanceStatus.PendingApproval,
            Note = note ?? (isCheckIn ? "Quét Face ID vào ca" : "Quét Face ID hết ca")
        };
        await _attendanceRepo.AddAttendanceAsync(newAtt);
        await _unitOfWork.SaveChangesAsync();
        await InitializeDraftForMonthAsync(TargetMonth, TargetYear);
        return newAtt;
    }

    /// <summary>
    /// Admin phê duyệt yêu cầu chấm công của nhân viên
    /// </summary>
    public async Task<bool> ApproveAttendanceAsync(int attendanceId)
    {
        var att = await _attendanceRepo.GetAttendanceByIdAsync(attendanceId);
        if (att == null) return false;

        att.Status = (att.CheckIn.HasValue && att.CheckIn.Value > new TimeSpan(8, 30, 0)) 
            ? AttendanceStatus.Late 
            : AttendanceStatus.Present;
        att.ActualWorkDays = 1.0;
        att.Note = string.IsNullOrEmpty(att.Note) 
            ? "Đã duyệt bởi Quản trị viên" 
            : $"{att.Note} • [Đã duyệt]";

        await _attendanceRepo.UpdateAttendanceAsync(att);
        await _unitOfWork.SaveChangesAsync();
        await InitializeDraftForMonthAsync(TargetMonth, TargetYear);
        return true;
    }

    /// <summary>
    /// Admin từ chối yêu cầu chấm công của nhân viên
    /// </summary>
    public async Task<bool> RejectAttendanceAsync(int attendanceId, string? reason = null)
    {
        var att = await _attendanceRepo.GetAttendanceByIdAsync(attendanceId);
        if (att == null) return false;

        att.Status = AttendanceStatus.Rejected;
        att.ActualWorkDays = 0.0;
        string rejectReason = string.IsNullOrWhiteSpace(reason) ? "Không hợp lệ" : reason;
        att.Note = string.IsNullOrEmpty(att.Note) 
            ? $"Từ chối: {rejectReason}" 
            : $"{att.Note} • [Từ chối: {rejectReason}]";

        await _attendanceRepo.UpdateAttendanceAsync(att);
        await _unitOfWork.SaveChangesAsync();
        await InitializeDraftForMonthAsync(TargetMonth, TargetYear);
        return true;
    }

    /// <summary>
    /// Lấy danh sách các yêu cầu chấm công đang chờ duyệt
    /// </summary>
    public async Task<List<TimeAttendance>> GetPendingAttendancesAsync()
    {
        return (await _attendanceRepo.GetPendingAttendancesAsync()).ToList();
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

        foreach (var item in DraftItems)
        {
            if (item.Id > 0)
            {
                var existing = await _attendanceRepo.GetAttendanceByIdAsync(item.Id);
                if (existing != null)
                {
                    existing.CheckIn = item.CheckIn;
                    existing.CheckOut = item.CheckOut;
                    existing.ActualWorkDays = item.ActualWorkDays;
                    existing.OvertimeHours = item.OvertimeHours;
                    existing.LateMinutes = item.LateMinutes;
                    existing.Status = item.Status;
                    existing.Note = item.Note;
                    if (!string.IsNullOrEmpty(item.PhotoPath)) existing.PhotoPath = item.PhotoPath;
                    if (!string.IsNullOrEmpty(item.CheckOutPhotoPath)) existing.CheckOutPhotoPath = item.CheckOutPhotoPath;
                    if (item.WorkShiftId.HasValue) existing.WorkShiftId = item.WorkShiftId;
                    await _attendanceRepo.UpdateAttendanceAsync(existing);
                }
            }
            else
            {
                var newEntity = new TimeAttendance
                {
                    EmployeeId = item.EmployeeId,
                    WorkDate = item.WorkDate,
                    CheckIn = item.CheckIn,
                    CheckOut = item.CheckOut,
                    ActualWorkDays = item.ActualWorkDays,
                    OvertimeHours = item.OvertimeHours,
                    LateMinutes = item.LateMinutes,
                    Status = item.Status,
                    Note = item.Note,
                    PhotoPath = item.PhotoPath,
                    CheckOutPhotoPath = item.CheckOutPhotoPath,
                    WorkShiftId = item.WorkShiftId ?? 1
                };
                await _attendanceRepo.AddAttendanceAsync(newEntity);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        int savedCount = DraftItems.Count;
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
