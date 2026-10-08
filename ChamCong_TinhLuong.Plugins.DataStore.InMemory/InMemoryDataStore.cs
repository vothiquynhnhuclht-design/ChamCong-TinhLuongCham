using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;

namespace ChamCong_TinhLuong.Plugins.DataStore.InMemory;

public class InMemoryDataStore
{
    public List<Department> Departments { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<Contract> Contracts { get; set; } = new();
    public List<WorkShift> WorkShifts { get; set; } = new();
    public List<DeductionRate> DeductionRates { get; set; } = new();
    public List<TimeAttendance> TimeAttendances { get; set; } = new();
    public List<Payslip> Payslips { get; set; } = new();

    public InMemoryDataStore()
    {
        SeedData();
    }

    private void SeedData()
    {
        // 1. Seed Phòng Ban
        var itDept = new Department { Id = 1, DepartmentCode = "IT", DepartmentName = "Phòng Công Nghệ Thông Tin", Description = "Phát triển phần mềm & Hạ tầng" };
        var hrDept = new Department { Id = 2, DepartmentCode = "HR", DepartmentName = "Phòng Nhân Sự - Kế Toán", Description = "Tuyển dụng, Chấm công, Tính lương" };
        var mktDept = new Department { Id = 3, DepartmentCode = "MKT", DepartmentName = "Phòng Marketing & Kinh Doanh", Description = "Phát triển thị trường" };
        Departments.AddRange(new[] { itDept, hrDept, mktDept });

        // 2. Seed Cấu hình Tỷ lệ trích khấu trừ (DeductionRate - Không hardcode)
        DeductionRates.AddRange(new[]
        {
            new DeductionRate { Id = 1, Code = "BHXH", Name = "Bảo hiểm xã hội (8%)", RatePercentage = 0.08m, ApplyOn = "InsuranceSalary", IsActive = true },
            new DeductionRate { Id = 2, Code = "BHYT", Name = "Bảo hiểm y tế (1.5%)", RatePercentage = 0.015m, ApplyOn = "InsuranceSalary", IsActive = true },
            new DeductionRate { Id = 3, Code = "BHTN", Name = "Bảo hiểm thất nghiệp (1%)", RatePercentage = 0.01m, ApplyOn = "InsuranceSalary", IsActive = true },
            new DeductionRate { Id = 4, Code = "TNCN_BAN_THAN", Name = "Giảm trừ gia cảnh bản thân", FlatAmount = 11000000m, ApplyOn = "TaxableIncome", IsActive = true },
            new DeductionRate { Id = 5, Code = "TNCN_PHU_THUOC", Name = "Giảm trừ người phụ thuộc", FlatAmount = 4400000m, ApplyOn = "TaxableIncome", IsActive = true }
        });

        // 3. Seed Nhân Viên
        var emp1 = new Employee
        {
            Id = 1,
            EmployeeCode = "NV001",
            FullName = "Nguyễn Văn An",
            Email = "an.nguyen@company.com",
            PhoneNumber = "0901234567",
            Position = "Senior Fullstack Developer",
            DepartmentId = 1,
            Department = itDept,
            JoinDate = new DateTime(2023, 1, 15),
            DependentCount = 1,
            IsActive = true
        };

        var emp2 = new Employee
        {
            Id = 2,
            EmployeeCode = "NV002",
            FullName = "Trần Thị Bình",
            Email = "binh.tran@company.com",
            PhoneNumber = "0912345678",
            Position = "HR Manager",
            DepartmentId = 2,
            Department = hrDept,
            JoinDate = new DateTime(2022, 5, 10),
            DependentCount = 0,
            IsActive = true
        };

        var emp3 = new Employee
        {
            Id = 3,
            EmployeeCode = "NV003",
            FullName = "Lê Hoàng Cường",
            Email = "cuong.le@company.com",
            PhoneNumber = "0923456789",
            Position = "Marketing Specialist",
            DepartmentId = 3,
            Department = mktDept,
            JoinDate = new DateTime(2024, 2, 1),
            DependentCount = 0,
            IsActive = true
        };
        Employees.AddRange(new[] { emp1, emp2, emp3 });

        // 4. Seed Hợp đồng lao động (Tách rời khỏi Employee)
        var c1 = new Contract
        {
            Id = 1,
            EmployeeId = 1,
            Employee = emp1,
            ContractNumber = "HD-2023/001",
            ContractType = ContractType.FullTime,
            BasicSalary = 20000000m,   // 20 triệu
            Allowance = 2500000m,      // Phụ cấp 2.5 triệu
            InsuranceSalary = 20000000m,
            StartDate = new DateTime(2023, 1, 15),
            IsCurrent = true
        };

        var c2 = new Contract
        {
            Id = 2,
            EmployeeId = 2,
            Employee = emp2,
            ContractNumber = "HD-2022/045",
            ContractType = ContractType.FullTime,
            BasicSalary = 16000000m,   // 16 triệu
            Allowance = 2000000m,      // Phụ cấp 2 triệu
            InsuranceSalary = 16000000m,
            StartDate = new DateTime(2022, 5, 10),
            IsCurrent = true
        };

        var c3 = new Contract
        {
            Id = 3,
            EmployeeId = 3,
            Employee = emp3,
            ContractNumber = "HD-2024/012",
            ContractType = ContractType.FullTime,
            BasicSalary = 12000000m,   // 12 triệu
            Allowance = 1500000m,      // Phụ cấp 1.5 triệu
            InsuranceSalary = 12000000m,
            StartDate = new DateTime(2024, 2, 1),
            IsCurrent = true
        };
        Contracts.AddRange(new[] { c1, c2, c3 });
        emp1.Contracts.Add(c1);
        emp2.Contracts.Add(c2);
        emp3.Contracts.Add(c3);

        // 5. Seed Dữ liệu chấm công mẫu cho tháng hiện tại (Tháng 10/2026 hoặc tháng gần nhất)
        int currentMonth = 10;
        int currentYear = 2026;
        for (int day = 1; day <= 22; day++)
        {
            var date = new DateTime(currentYear, currentMonth, day);
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

            // NV1: Đi làm đầy đủ, có một số ngày OT
            TimeAttendances.Add(new TimeAttendance
            {
                Id = TimeAttendances.Count + 1,
                EmployeeId = 1,
                Employee = emp1,
                WorkDate = date,
                CheckIn = new TimeSpan(8, 0, 0),
                CheckOut = new TimeSpan(17, 30, 0),
                ActualWorkDays = 1.0,
                OvertimeHours = (day % 5 == 0) ? 2.0 : 0.0,
                Status = AttendanceStatus.Present
            });

            // NV2: Đi làm đầy đủ
            TimeAttendances.Add(new TimeAttendance
            {
                Id = TimeAttendances.Count + 1,
                EmployeeId = 2,
                Employee = emp2,
                WorkDate = date,
                CheckIn = new TimeSpan(8, 5, 0),
                CheckOut = new TimeSpan(17, 30, 0),
                ActualWorkDays = 1.0,
                OvertimeHours = 0.0,
                Status = AttendanceStatus.Present
            });

            // NV3: Có 1 ngày nghỉ không phép (day 15) và 1 ngày đi trễ (day 8)
            if (day == 15)
            {
                TimeAttendances.Add(new TimeAttendance
                {
                    Id = TimeAttendances.Count + 1,
                    EmployeeId = 3,
                    Employee = emp3,
                    WorkDate = date,
                    ActualWorkDays = 0.0,
                    Status = AttendanceStatus.Absent,
                    Note = "Nghỉ không phép"
                });
            }
            else
            {
                TimeAttendances.Add(new TimeAttendance
                {
                    Id = TimeAttendances.Count + 1,
                    EmployeeId = 3,
                    Employee = emp3,
                    WorkDate = date,
                    CheckIn = (day == 8) ? new TimeSpan(8, 45, 0) : new TimeSpan(8, 0, 0),
                    CheckOut = new TimeSpan(17, 30, 0),
                    ActualWorkDays = 1.0,
                    LateMinutes = (day == 8) ? 45 : 0,
                    Status = (day == 8) ? AttendanceStatus.Late : AttendanceStatus.Present
                });
            }
        }
    }
}
