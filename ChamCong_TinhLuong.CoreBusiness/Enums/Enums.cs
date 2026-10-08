namespace ChamCong_TinhLuong.CoreBusiness.Enums;

public enum PayslipStatus
{
    Draft = 0,      // Bảng lương nháp, đang tính toán/hiệu chỉnh
    Locked = 1,     // Đã chốt khóa sổ, không được phép sửa đổi hoặc tính lại
    Paid = 2        // Đã chi trả
}

public enum AttendanceStatus
{
    Present = 0,    // Đi làm đủ công
    Late = 1,       // Đi trễ
    EarlyLeave = 2, // Về sớm
    LeavePaid = 3,  // Nghỉ phép có lương
    LeaveUnpaid = 4,// Nghỉ không lương
    Absent = 5      // Vắng không phép
}

public enum ContractType
{
    Probation = 0,      // Thử việc
    FullTime = 1,       // Chính thức
    PartTime = 2,       // Bán thời gian
    Seasonal = 3        // Thời vụ
}

public enum PayslipLineType
{
    Earning = 0,    // Khoản cộng (Lương thời gian, Phụ cấp, Thưởng, OT...)
    Deduction = 1   // Khoản trừ (BHXH, BHYT, BHTN, Thuế TNCN, Tạm ứng, Phạt...)
}
