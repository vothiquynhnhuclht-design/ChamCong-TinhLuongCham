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
    Absent = 5,     // Vắng không phép
    PendingApproval = 6, // Chờ duyệt chấm công (Quét khuôn mặt)
    Rejected = 7    // Bị từ chối
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

public enum UserRole
{
    Admin = 0,      // Quản trị toàn quyền
    HR = 1,         // Kế toán / Nhân sự (Toàn quyền nghiệp vụ)
    Employee = 2    // Nhân viên (Row-Level Security: Chỉ xem dữ liệu cá nhân)
}

public enum LeaveType
{
    Paid = 0,       // Nghỉ phép có hưởng lương (Phép năm, chế độ cưới hỏi...)
    Unpaid = 1      // Nghỉ không hưởng lương
}

public enum LeaveRequestStatus
{
    Pending = 0,    // Chờ quản lý / HR duyệt
    Approved = 1,   // Đã phê duyệt (Được tính vào công kỳ lương)
    Rejected = 2    // Bị từ chối
}

public enum NotificationType
{
    EmailPayslip = 0,   // Gửi phiếu lương qua email
    LeaveApproval = 1,  // Thông báo duyệt đơn nghỉ phép
    AttendanceAlert = 2 // Cảnh báo đi trễ / vắng mặt
}

public enum NotificationStatus
{
    Pending = 0,    // Đang chờ gửi
    Success = 1,    // Gửi thành công
    Failed = 2      // Gửi thất bại
}
