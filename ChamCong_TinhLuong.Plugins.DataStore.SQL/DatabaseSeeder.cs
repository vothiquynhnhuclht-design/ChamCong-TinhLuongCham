using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;
using Microsoft.EntityFrameworkCore;

namespace ChamCong_TinhLuong.Plugins.DataStore.SQL;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ChamCongDbContext context)
    {
        // Đảm bảo Database đã được tạo hoặc migrate
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Phòng Ban nếu chưa có
        if (!await context.Departments.AnyAsync())
        {
            var itDept = new Department { DepartmentCode = "IT", DepartmentName = "Phòng Công Nghệ Thông Tin", Description = "Phát triển phần mềm & Hạ tầng" };
            var hrDept = new Department { DepartmentCode = "HR", DepartmentName = "Phòng Nhân Sự - Kế Toán", Description = "Tuyển dụng, Chấm công, Tính lương" };
            var mktDept = new Department { DepartmentCode = "MKT", DepartmentName = "Phòng Marketing & Kinh Doanh", Description = "Phát triển thị trường" };
            await context.Departments.AddRangeAsync(itDept, hrDept, mktDept);
            await context.SaveChangesAsync();
        }

        // 2. Seed Cấu hình tỷ lệ trích khấu trừ (DeductionRate)
        if (!await context.DeductionRates.AnyAsync())
        {
            await context.DeductionRates.AddRangeAsync(
                new DeductionRate { Code = "BHXH", Name = "Bảo hiểm xã hội (8%)", RatePercentage = 0.08m, ApplyOn = "InsuranceSalary", IsActive = true },
                new DeductionRate { Code = "BHYT", Name = "Bảo hiểm y tế (1.5%)", RatePercentage = 0.015m, ApplyOn = "InsuranceSalary", IsActive = true },
                new DeductionRate { Code = "BHTN", Name = "Bảo hiểm thất nghiệp (1%)", RatePercentage = 0.01m, ApplyOn = "InsuranceSalary", IsActive = true },
                new DeductionRate { Code = "TNCN_BAN_THAN", Name = "Giảm trừ gia cảnh bản thân", FlatAmount = 11000000m, ApplyOn = "TaxableIncome", IsActive = true },
                new DeductionRate { Code = "TNCN_PHU_THUOC", Name = "Giảm trừ người phụ thuộc", FlatAmount = 4400000m, ApplyOn = "TaxableIncome", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Nhân Viên và Hợp Đồng nếu chưa có
        if (!await context.Employees.AnyAsync())
        {
            var itDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "IT");
            var hrDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "HR");
            var mktDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "MKT");

            var emp1 = new Employee
            {
                EmployeeCode = "NV001",
                FullName = "Nguyễn Văn An",
                Email = "an.nguyen@company.com",
                PhoneNumber = "0901234567",
                Position = "Senior Fullstack Developer",
                DepartmentId = itDept.Id,
                JoinDate = new DateTime(2023, 1, 15),
                DependentCount = 1,
                IsActive = true
            };

            var emp2 = new Employee
            {
                EmployeeCode = "NV002",
                FullName = "Trần Thị Bình",
                Email = "binh.tran@company.com",
                PhoneNumber = "0912345678",
                Position = "HR Manager",
                DepartmentId = hrDept.Id,
                JoinDate = new DateTime(2022, 5, 10),
                DependentCount = 0,
                IsActive = true
            };

            var emp3 = new Employee
            {
                EmployeeCode = "NV003",
                FullName = "Lê Hoàng Cường",
                Email = "cuong.le@company.com",
                PhoneNumber = "0923456789",
                Position = "Marketing Specialist",
                DepartmentId = mktDept.Id,
                JoinDate = new DateTime(2024, 2, 1),
                DependentCount = 0,
                IsActive = true
            };

            await context.Employees.AddRangeAsync(emp1, emp2, emp3);
            await context.SaveChangesAsync();

            // Seed Hợp Đồng (Contract) tương ứng
            await context.Contracts.AddRangeAsync(
                new Contract
                {
                    EmployeeId = emp1.Id,
                    ContractNumber = "HD-2023/001",
                    ContractType = ContractType.FullTime,
                    BasicSalary = 20000000m,
                    Allowance = 2500000m,
                    InsuranceSalary = 20000000m,
                    StartDate = new DateTime(2023, 1, 15),
                    IsCurrent = true
                },
                new Contract
                {
                    EmployeeId = emp2.Id,
                    ContractNumber = "HD-2022/045",
                    ContractType = ContractType.FullTime,
                    BasicSalary = 16000000m,
                    Allowance = 2000000m,
                    InsuranceSalary = 16000000m,
                    StartDate = new DateTime(2022, 5, 10),
                    IsCurrent = true
                },
                new Contract
                {
                    EmployeeId = emp3.Id,
                    ContractNumber = "HD-2024/012",
                    ContractType = ContractType.FullTime,
                    BasicSalary = 12000000m,
                    Allowance = 1500000m,
                    InsuranceSalary = 12000000m,
                    StartDate = new DateTime(2024, 2, 1),
                    IsCurrent = true
                }
            );
            await context.SaveChangesAsync();

            // Seed Chấm công mẫu cho Tháng 10/2026
            var attendances = new List<TimeAttendance>();
            int currentMonth = 10;
            int currentYear = 2026;

            for (int day = 1; day <= 22; day++)
            {
                var date = new DateTime(currentYear, currentMonth, day);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                // NV1: Đi làm đủ, một số ngày OT
                attendances.Add(new TimeAttendance
                {
                    EmployeeId = emp1.Id,
                    WorkDate = date,
                    CheckIn = new TimeSpan(8, 0, 0),
                    CheckOut = new TimeSpan(17, 30, 0),
                    ActualWorkDays = 1.0,
                    OvertimeHours = (day % 5 == 0) ? 2.0 : 0.0,
                    Status = AttendanceStatus.Present
                });

                // NV2: Đi làm đủ
                attendances.Add(new TimeAttendance
                {
                    EmployeeId = emp2.Id,
                    WorkDate = date,
                    CheckIn = new TimeSpan(8, 5, 0),
                    CheckOut = new TimeSpan(17, 30, 0),
                    ActualWorkDays = 1.0,
                    Status = AttendanceStatus.Present
                });

                // NV3: 1 ngày nghỉ không phép, 1 ngày trễ
                if (day == 15)
                {
                    attendances.Add(new TimeAttendance
                    {
                        EmployeeId = emp3.Id,
                        WorkDate = date,
                        ActualWorkDays = 0.0,
                        Status = AttendanceStatus.Absent,
                        Note = "Nghỉ không phép"
                    });
                }
                else
                {
                    attendances.Add(new TimeAttendance
                    {
                        EmployeeId = emp3.Id,
                        WorkDate = date,
                        CheckIn = (day == 8) ? new TimeSpan(8, 45, 0) : new TimeSpan(8, 0, 0),
                        CheckOut = new TimeSpan(17, 30, 0),
                        ActualWorkDays = 1.0,
                        LateMinutes = (day == 8) ? 45 : 0,
                        Status = (day == 8) ? AttendanceStatus.Late : AttendanceStatus.Present
                    });
                }
            }

            await context.TimeAttendances.AddRangeAsync(attendances);
            await context.SaveChangesAsync();
        }
    }
}
