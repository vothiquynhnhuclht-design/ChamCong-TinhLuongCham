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
    private readonly IUnitOfWork _unitOfWork;

    public CalculatePayrollUseCase(
        IEmployeeRepository employeeRepo,
        IContractRepository contractRepo,
        IAttendanceRepository attendanceRepo,
        IDeductionRateRepository deductionRepo,
        IPayslipRepository payslipRepo,
        IUnitOfWork unitOfWork)
    {
        _employeeRepo = employeeRepo;
        _contractRepo = contractRepo;
        _attendanceRepo = attendanceRepo;
        _deductionRepo = deductionRepo;
        _payslipRepo = payslipRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollCalculationResultDto> ExecuteAsync(int month, int year, double standardWorkDays = 22.0)
    {
        // LUẬT NGHIỆP VỤ 2: Kiểm tra kỳ lương đã khóa sổ chưa
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

        // LUẬT NGHIỆP VỤ 1: Toàn bộ quá trình tính và lưu bảng lương nằm trọn trong 1 Database Transaction
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

                    // 2. Tính toán Lương cơ bản theo ngày công: Lương cơ bản * Ngày công thực tế / Ngày công chuẩn
                    decimal basicSalary = contract.BasicSalary;
                    decimal workSalary = standardWorkDays > 0 
                        ? Math.Round((basicSalary / (decimal)standardWorkDays) * (decimal)actualWorkDays, 0)
                        : 0m;

                    // 3. Tính toán Lương tăng ca (OT 150%)
                    decimal hourlyRate = standardWorkDays > 0 ? (basicSalary / (decimal)(standardWorkDays * 8.0)) : 0m;
                    decimal otSalary = Math.Round(hourlyRate * (decimal)overtimeHours * 1.5m, 0);

                    // 4. Phụ cấp cố định từ hợp đồng
                    decimal allowance = contract.Allowance;

                    // Tổng thu nhập (Gross Salary)
                    decimal grossSalary = workSalary + otSalary + allowance;

                    // 5. Khởi tạo Header phiếu lương
                    var payslip = new Payslip
                    {
                        EmployeeId = emp.Id,
                        Month = month,
                        Year = year,
                        StandardWorkDays = standardWorkDays,
                        ActualWorkDays = actualWorkDays,
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
                                LineType = PayslipLineType.Deduction,
                                ItemCode = rate.Code,
                                ItemName = rate.Name,
                                Amount = deductionAmt,
                                RateApplied = rate.RatePercentage,
                                CalculationNote = $"{rate.RatePercentage * 100:N1}% của mức lương đóng BH ({insuranceSalary:N0} đ)"
                            });
                        }
                    }

                    // 8. Tính thuế TNCN (Tạm tính đơn giản theo luật: Thu nhập chịu thuế = Gross - BH - Giảm trừ bản thân 11tr - Người phụ thuộc 4.4tr)
                    decimal selfRelief = deductionRates.FirstOrDefault(r => r.Code == "TNCN_BAN_THAN")?.FlatAmount ?? 11000000m;
                    decimal dependentRelief = (deductionRates.FirstOrDefault(r => r.Code == "TNCN_PHU_THUOC")?.FlatAmount ?? 4400000m) * emp.DependentCount;
                    
                    decimal taxableIncome = grossSalary - totalDeductions - selfRelief - dependentRelief;
                    decimal incomeTax = 0m;
                    if (taxableIncome > 0)
                    {
                        // Bậc 1: <= 5tr -> 5%
                        // Bậc 2: 5tr -> 10tr -> 10%
                        if (taxableIncome <= 5000000m)
                            incomeTax = Math.Round(taxableIncome * 0.05m, 0);
                        else if (taxableIncome <= 10000000m)
                            incomeTax = Math.Round(250000m + (taxableIncome - 5000000m) * 0.10m, 0);
                        else
                            incomeTax = Math.Round(750000m + (taxableIncome - 10000000m) * 0.15m, 0);

                        totalDeductions += incomeTax;
                        payslip.Lines.Add(new PayslipLine
                        {
                            LineType = PayslipLineType.Deduction,
                            ItemCode = "TAX_TNCN",
                            ItemName = "Thuế thu nhập cá nhân (TNCN)",
                            Amount = incomeTax,
                            CalculationNote = $"Thu nhập tính thuế: {taxableIncome:N0} đ (sau giảm trừ {selfRelief + dependentRelief:N0} đ)"
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
}
