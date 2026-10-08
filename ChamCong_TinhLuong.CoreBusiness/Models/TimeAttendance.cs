using ChamCong_TinhLuong.CoreBusiness.Enums;

namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class TimeAttendance
{
    public int Id { get; set; }
    
    // Foreign Key
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime WorkDate { get; set; } // Ngày làm việc
    public TimeSpan? CheckIn { get; set; } // Giờ vào thực tế
    public TimeSpan? CheckOut { get; set; } // Giờ ra thực tế

    public int LateMinutes { get; set; } = 0; // Số phút đi trễ
    public int EarlyLeaveMinutes { get; set; } = 0; // Số phút về sớm
    
    public double ActualWorkDays { get; set; } = 1.0; // Số công thực tế (1.0, 0.5, 0.0)
    public double OvertimeHours { get; set; } = 0.0; // Số giờ tăng ca OT
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Note { get; set; }
}
