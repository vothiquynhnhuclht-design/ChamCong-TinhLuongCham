using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.UseCases.DTOs;

public class AttendanceDraftItemDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public double ActualWorkDays { get; set; } = 1.0;
    public double OvertimeHours { get; set; } = 0.0;
    public int LateMinutes { get; set; } = 0;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Note { get; set; }
}

public class MonthlyAttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public double TotalWorkDays { get; set; }
    public double TotalOvertimeHours { get; set; }
    public int TotalLateMinutes { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
}

public class PayrollCalculationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalProcessed { get; set; }
    public decimal TotalPayrollAmount { get; set; }
    public List<string> Warnings { get; set; } = new();
}
