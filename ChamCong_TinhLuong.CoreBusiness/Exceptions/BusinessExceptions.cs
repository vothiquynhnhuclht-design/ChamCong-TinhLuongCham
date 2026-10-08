namespace ChamCong_TinhLuong.CoreBusiness.Exceptions;

public class PayslipLockedException : Exception
{
    public PayslipLockedException(int month, int year) 
        : base($"Kỳ lương tháng {month}/{year} đã được khóa sổ (Locked). Tuyệt đối không được phép chỉnh sửa hoặc tính toán lại!")
    {
    }

    public PayslipLockedException(string message) : base(message)
    {
    }
}

public class EmployeeNotFoundException : Exception
{
    public EmployeeNotFoundException(int employeeId) 
        : base($"Không tìm thấy thông tin nhân viên với mã định danh #{employeeId}.")
    {
    }
}

public class ActiveContractNotFoundException : Exception
{
    public ActiveContractNotFoundException(string employeeCode) 
        : base($"Nhân viên [{employeeCode}] không có hợp đồng lao động nào đang hiệu lực để tính lương!")
    {
    }
}
