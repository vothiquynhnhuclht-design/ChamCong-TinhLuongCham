namespace ChamCong_TinhLuong.CoreBusiness.Models;

public class WorkShift
{
    public int Id { get; set; }
    public string ShiftCode { get; set; } = "HC"; // HC = Hành chính, CA1, CA2...
    public string ShiftName { get; set; } = "Ca Hành Chính";
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0); // 08:00
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 30, 0); // 17:30
    public TimeSpan BreakStartTime { get; set; } = new TimeSpan(12, 0, 0); // 12:00
    public TimeSpan BreakEndTime { get; set; } = new TimeSpan(13, 30, 0); // 13:30
    public double WorkDayFactor { get; set; } = 1.0; // 1.0 công
}
