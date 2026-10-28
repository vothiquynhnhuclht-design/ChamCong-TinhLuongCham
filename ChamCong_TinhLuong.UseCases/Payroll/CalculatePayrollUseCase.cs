using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Exceptions;
using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.DTOs;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.UseCases.Payroll;

public class CalculatePayrollUseCase
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IContractRepository _contractRepo;
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IDeductionRateRepository _deductionRepo;
    private readonly IPayslipRepository _payslipRepo;
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CalculatePayrollUseCase(
        IEmployeeRepository employeeRepo,
        IContractRepository contractRepo,
        IAttendanceRepository attendanceRepo,
        IDeductionRateRepository deductionRepo,
        IPayslipRepository payslipRepo,
        ILeaveRequestRepository leaveRepo,
        IUnitOfWork unitOfWork)
    {
        _employeeRepo = employeeRepo;
        _contractRepo = contractRepo;
        _attendanceRepo = attendanceRepo;
        _deductionRepo = deductionRepo;
        _payslipRepo = payslipRepo;
        _leaveRepo = leaveRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollCalculationResultDto> ExecuteAsync(int month, int year, double standardWorkDays = 22.0)
    {
        // Kiểm tra kỳ lương đã khóa sổ chưa
        bool isLocked = await _payslipRepo.IsPeriodLockedAsync(month, year);
        if (isLocked)
        {
            throw new PayslipLockedException(month, year);
        }

        var employees = (await _employeeRepo.GetAllEmployeesAsync(includeInactive: false)).ToList();
        var deductionRates = (await _deductionRepo.GetActiveDeductionRatesAsync()).ToList();
        var allAttendances = (await _attendanceRepo.GetAttendancesByMonthAsync(month, year)).ToList();

        var result = new PayrollCalculationResultDto();
        var generatedPayslips = new List<Payslip>();

        using (await _unitOfWork.BeginTransactionAsync())
        {
            try
            {
                // Xóa các bảng lương nháp (Draft) cũ của kỳ này trước khi tính lại
                await _payslipRepo.DeletePayslipsByPeriodAsync(month, year);

                foreach (var emp in employees)
                {
                    var contract = await _contractRepo.GetActiveContractByEmployeeIdAsync(emp.Id);
                    if (contract == null)
                    {
                        result.Warnings.Add($"Nhân viên {emp.FullName} ({emp.EmployeeCode}) chưa có hợp đồng hiệu lực. Đã bỏ qua.");
                        continue;
                    }

                    // 1. Tổng hợp công tháng của nhân viên
                    var empAttendances = allAttendances.Where(a => a.EmployeeId == emp.Id).ToList();
                    double actualWorkDays = empAttendances.Sum(a => a.ActualWorkDays);
                    double overtimeHours = empAttendances.Sum(a => a.OvertimeHours);

                    // 1b. Tổng hợp đơn xin nghỉ phép đã được phê duyệt trong tháng
                    var approvedLeaves = (await _leaveRepo.GetApprovedLeaveRequestsByMonthAsync(emp.Id, month, year)).ToList();
                    double paidLeaveDays = approvedLeaves.Where(l => l.LeaveType == LeaveType.Paid).Sum(l => l.NumberOfDays);
                    double unpaidLeaveDays = approvedLeaves.Where(l => l.LeaveType == LeaveType.Unpaid).Sum(l => l.NumberOfDays);

                    // 2. Tính toán Lương cơ bản theo ngày công: Lương cơ bản * Ngày công thực tế / Ngày công chuẩn
                    decimal basicSalary = contract.BasicSalary;
                    decimal workSalary = standardWorkDays > 0 
                        ? Math.Round((basicSalary / (decimal)standardWorkDays) * (decimal)actualWorkDays, 0)
                        : 0m;

                    // 2b. Tính toán Lương nghỉ phép hưởng nguyên lương (Paid Leave)
                    decimal paidLeaveSalary = standardWorkDays > 0 && paidLeaveDays > 0
                        ? Math.Round((basicSalary / (decimal)standardWorkDays) * (decimal)paidLeaveDays, 0)
                        : 0m;

                    // 3. Tính toán Lương tăng ca (OT 150%)
                    decimal hourlyRate = standardWorkDays > 0 ? (basicSalary / (decimal)(standardWorkDays * 8.0)) : 0m;
                    decimal otSalary = Math.Round(hourlyRate * (decimal)overtimeHours * 1.5m, 0);

                    // 4. Phụ cấp cố định từ hợp đồng
                    decimal allowance = contract.Allowance;

                    // Tổng thu nhập (Gross Salary bao gồm Lương thực làm + Lương phép + OT + Phụ cấp)
                    decimal grossSalary = workSalary + paidLeaveSalary + otSalary + allowance;

                    // 5. Khởi tạo Header phiếu lương
                    var payslip = new Payslip
                    {
                        EmployeeId = emp.Id,
                        Month = month,
                        Year = year,
                        StandardWorkDays = standardWorkDays,
                        ActualWorkDays = actualWorkDays + paidLeaveDays, // Tổng số ngày tính lương
                        OvertimeHours = overtimeHours,
                        BasicSalary = basicSalary,
                        Allowance = allowance,
                        GrossSalary = grossSalary,
                        Status = PayslipStatus.Draft,
                        CreatedAt = DateTime.Now
                    };

                    // 6. Thêm các dòng chi tiết Thu nhập (Earnings)
                    payslip.Lines.Add(new PayslipLine
                    {
                        LineType = PayslipLineType.Earning,
                        ItemCode = "WORK_SALARY",
                        ItemName = "Lương theo ngày công thực tế",
                        Amount = workSalary,
                        CalculationNote = $"({basicSalary:N0} đ / {standardWorkDays} ngày) x {actualWorkDays:N1} ngày"
                    });

                    if (paidLeaveDays > 0)
                    {
                        payslip.Lines.Add(new PayslipLine
                        {
                            LineType = PayslipLineType.Earning,
                            ItemCode = "PAID_LEAVE_SALARY",
                            ItemName = "Lương nghỉ phép hưởng nguyên lương",
                            Amount = paidLeaveSalary,
                            CalculationNote = $"({basicSalary:N0} đ / {standardWorkDays} ngày) x {paidLeaveDays:N1} ngày phép duyệt"
                        });
                    }

                    if (otSalary > 0)
                    {
                        payslip.Lines.Add(new PayslipLine
                        {
                            LineType = PayslipLineType.Earning,
                            ItemCode = "OT_SALARY",
                            ItemName = "Lương làm thêm giờ (OT 150%)",
                            Amount = otSalary,
                            RateApplied = 1.5m,
                            CalculationNote = $"({hourlyRate:N0} đ/h x 1.5) x {overtimeHours:N1} giờ"
                        });
                    }

                    if (allowance > 0)
                    {
                        payslip.Lines.Add(new PayslipLine
                        {
                            LineType = PayslipLineType.Earning,
                            ItemCode = "ALLOWANCE",
                            ItemName = "Phụ cấp trách nhiệm & ăn trưa",
                            Amount = allowance,
                            CalculationNote = "Theo phụ cấp ghi nhận trên hợp đồng"
                        });
                    }

                    // 7. Tính các khoản trích khấu trừ từ bảng cấu hình DeductionRate (Bảo hiểm, Thuế...)
                    decimal totalDeductions = 0m;
                    decimal insuranceSalary = contract.InsuranceSalary > 0 ? contract.InsuranceSalary : basicSalary;

                    foreach (var rate in deductionRates)
                    {
                        if (rate.RatePercentage > 0 && rate.ApplyOn == "InsuranceSalary")
                        {
                            decimal deductionAmt = Math.Round(insuranceSalary * rate.RatePercentage, 0);
                            totalDeductions += deductionAmt;

                            payslip.Lines.Add(new PayslipLine
                            {
                                DeductionRateId = rate.Id,
                                LineType = PayslipLineType.Deduction,
                                ItemCode = rate.Code,
                                ItemName = rate.Name,
                                Amount = deductionAmt,
                                RateApplied = rate.RatePercentage,
                                CalculationNote = $"{rate.RatePercentage * 100:N1}% của mức lương đóng BH ({insuranceSalary:N0} đ)"
                            });
                        }
                    }

                    // 8. Tính thuế TNCN theo Biểu thuế lũy tiến từng phần 7 bậc (Luật Thuế TNCN Việt Nam)
                    // Thu nhập tính thuế = Gross - Bảo hiểm bắt buộc - Giảm trừ bản thân (11tr) - Giảm trừ người phụ thuộc (4.4tr/người)
                    var selfRate = deductionRates.FirstOrDefault(r => r.Code == "TNCN_BAN_THAN");
                    decimal selfRelief = selfRate?.FlatAmount ?? 11000000m;
                    decimal dependentRelief = (deductionRates.FirstOrDefault(r => r.Code == "TNCN_PHU_THUOC")?.FlatAmount ?? 4400000m) * emp.DependentCount;
                    
                    decimal taxableIncome = grossSalary - totalDeductions - selfRelief - dependentRelief;
                    decimal incomeTax = CalculateProgressiveIncomeTax(taxableIncome);
                    if (incomeTax > 0)
                    {
                        totalDeductions += incomeTax;
                        payslip.Lines.Add(new PayslipLine
                        {
                            DeductionRateId = selfRate?.Id,
                            LineType = PayslipLineType.Deduction,
                            ItemCode = "TAX_TNCN",
                            ItemName = "Thuế thu nhập cá nhân (TNCN lũy tiến 7 bậc)",
                            Amount = incomeTax,
                            CalculationNote = $"TN tính thuế: {taxableIncome:N0} đ (Giảm trừ: {selfRelief + dependentRelief:N0} đ, {emp.DependentCount} NPT)"
                        });
                    }

                    // 9. Cập nhật tổng khấu trừ và Lương thực nhận (Net Salary)
                    payslip.TotalDeductions = totalDeductions;
                    payslip.NetSalary = grossSalary - totalDeductions;

                    generatedPayslips.Add(payslip);
                }

                // Lưu toàn bộ phiếu lương và chi tiết vào DB
                await _payslipRepo.AddPayslipsBatchAsync(generatedPayslips);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                result.Success = true;
                result.TotalProcessed = generatedPayslips.Count;
                result.TotalPayrollAmount = generatedPayslips.Sum(p => p.NetSalary);
                result.Message = $"Đã tính toán thành công kỳ lương tháng {month}/{year} cho {result.TotalProcessed} nhân viên. Tổng chi trả: {result.TotalPayrollAmount:N0} VNĐ.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception($"Lỗi trong quá trình tính lương: {ex.Message}", ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Biểu thuế TNCN lũy tiến từng phần 7 bậc đối với thu nhập từ tiền lương, tiền công (Thông tư 111/2013/TT-BTC):
    /// Bậc 1: Đến 5 triệu             -> 5%
    /// Bậc 2: Trên 5 đến 10 triệu     -> 10% - 0.25 triệu
    /// Bậc 3: Trên 10 đến 18 triệu    -> 15% - 0.75 triệu
    /// Bậc 4: Trên 18 đến 32 triệu    -> 20% - 1.65 triệu
    /// Bậc 5: Trên 32 đến 52 triệu    -> 25% - 3.25 triệu
    /// Bậc 6: Trên 52 đến 80 triệu    -> 30% - 5.85 triệu
    /// Bậc 7: Trên 80 triệu           -> 35% - 9.85 triệu
    /// </summary>
    public static decimal CalculateProgressiveIncomeTax(decimal taxableIncome)
    {
        if (taxableIncome <= 0m) return 0m;

        decimal tax = taxableIncome switch
        {
            <= 5_000_000m  => taxableIncome * 0.05m,
            <= 10_000_000m => taxableIncome * 0.10m - 250_000m,
            <= 18_000_000m => taxableIncome * 0.15m - 750_000m,
            <= 32_000_000m => taxableIncome * 0.20m - 1_650_000m,
            <= 52_000_000m => taxableIncome * 0.25m - 3_250_000m,
            <= 80_000_000m => taxableIncome * 0.30m - 5_850_000m,
            _              => taxableIncome * 0.35m - 9_850_000m
        };

        return Math.Round(tax, 0);
    }
}
