using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Exceptions;
using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.UseCases.Payroll;

public class LockPayrollPeriodUseCase
{
    private readonly IPayslipRepository _payslipRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LockPayrollPeriodUseCase(IPayslipRepository payslipRepo, IUnitOfWork unitOfWork)
    {
        _payslipRepo = payslipRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExecuteAsync(int month, int year, string lockedBy)
    {
        var payslips = (await _payslipRepo.GetPayslipsByPeriodAsync(month, year)).ToList();
        if (!payslips.Any())
        {
            throw new InvalidOperationException($"Không có bảng lương nào trong kỳ tháng {month}/{year} để khóa sổ!");
        }

        foreach (var slip in payslips)
        {
            slip.Status = PayslipStatus.Locked;
            slip.LockedAt = DateTime.Now;
            slip.LockedBy = lockedBy;
            await _payslipRepo.UpdatePayslipAsync(slip);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

public class ViewEmployeePayslipUseCase
{
    private readonly IPayslipRepository _payslipRepo;

    public ViewEmployeePayslipUseCase(IPayslipRepository payslipRepo)
    {
        _payslipRepo = payslipRepo;
    }

    /// <summary>
    /// Lấy chi tiết phiếu lương kèm kiểm tra Row-Level Security:
    /// Nếu currentUserRole != "Admin" && currentUserRole != "HR", bắt buộc currentUserEmail phải khớp với Email của Employee
    /// </summary>
    public async Task<Payslip?> ExecuteAsync(int payslipId, string currentUserEmail, string currentUserRole)
    {
        var payslip = await _payslipRepo.GetPayslipByIdAsync(payslipId);
        if (payslip == null) return null;

        // BẢO MẬT DỮ LIỆU CẤP DÒNG (Row-Level Security)
        bool isPrivileged = currentUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                           currentUserRole.Equals("HR", StringComparison.OrdinalIgnoreCase);

        if (!isPrivileged)
        {
            if (payslip.Employee == null || !string.Equals(payslip.Employee.Email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin phiếu lương của nhân viên khác!");
            }
        }

        return payslip;
    }
}
