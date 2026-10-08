using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.Plugins.DataStore.InMemory;

public class InMemoryUnitOfWork : IUnitOfWork
{
    private class DummyTransaction : IDisposable
    {
        public void Dispose() { }
    }

    public Task<IDisposable> BeginTransactionAsync()
    {
        return Task.FromResult<IDisposable>(new DummyTransaction());
    }

    public Task CommitAsync() => Task.CompletedTask;
    public Task RollbackAsync() => Task.CompletedTask;
    public Task SaveChangesAsync() => Task.CompletedTask;
}

public class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly InMemoryDataStore _db;
    public InMemoryEmployeeRepository(InMemoryDataStore db) => _db = db;

    public Task<IEnumerable<Employee>> GetAllEmployeesAsync(bool includeInactive = false)
    {
        var list = includeInactive ? _db.Employees : _db.Employees.Where(e => e.IsActive);
        return Task.FromResult<IEnumerable<Employee>>(list.ToList());
    }

    public Task<Employee?> GetEmployeeByIdAsync(int id) =>
        Task.FromResult(_db.Employees.FirstOrDefault(e => e.Id == id));

    public Task<Employee?> GetEmployeeByCodeAsync(string code) =>
        Task.FromResult(_db.Employees.FirstOrDefault(e => string.Equals(e.EmployeeCode, code, StringComparison.OrdinalIgnoreCase)));

    public Task<Employee?> GetEmployeeByEmailAsync(string email) =>
        Task.FromResult(_db.Employees.FirstOrDefault(e => string.Equals(e.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task AddEmployeeAsync(Employee employee)
    {
        employee.Id = _db.Employees.Any() ? _db.Employees.Max(e => e.Id) + 1 : 1;
        _db.Employees.Add(employee);
        return Task.CompletedTask;
    }

    public Task UpdateEmployeeAsync(Employee employee)
    {
        var existing = _db.Employees.FirstOrDefault(e => e.Id == employee.Id);
        if (existing != null)
        {
            existing.FullName = employee.FullName;
            existing.Email = employee.Email;
            existing.PhoneNumber = employee.PhoneNumber;
            existing.Position = employee.Position;
            existing.DepartmentId = employee.DepartmentId;
            existing.DependentCount = employee.DependentCount;
            existing.IsActive = employee.IsActive;
        }
        return Task.CompletedTask;
    }

    public Task DeleteEmployeeAsync(int id)
    {
        var existing = _db.Employees.FirstOrDefault(e => e.Id == id);
        if (existing != null) existing.IsActive = false; // Soft delete
        return Task.CompletedTask;
    }
}

public class InMemoryContractRepository : IContractRepository
{
    private readonly InMemoryDataStore _db;
    public InMemoryContractRepository(InMemoryDataStore db) => _db = db;

    public Task<IEnumerable<Contract>> GetContractsByEmployeeIdAsync(int employeeId) =>
        Task.FromResult<IEnumerable<Contract>>(_db.Contracts.Where(c => c.EmployeeId == employeeId).ToList());

    public Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId) =>
        Task.FromResult(_db.Contracts.FirstOrDefault(c => c.EmployeeId == employeeId && c.IsCurrent));

    public Task AddContractAsync(Contract contract)
    {
        contract.Id = _db.Contracts.Any() ? _db.Contracts.Max(c => c.Id) + 1 : 1;
        _db.Contracts.Add(contract);
        return Task.CompletedTask;
    }

    public Task UpdateContractAsync(Contract contract)
    {
        var existing = _db.Contracts.FirstOrDefault(c => c.Id == contract.Id);
        if (existing != null)
        {
            existing.BasicSalary = contract.BasicSalary;
            existing.Allowance = contract.Allowance;
            existing.InsuranceSalary = contract.InsuranceSalary;
            existing.IsCurrent = contract.IsCurrent;
            existing.EndDate = contract.EndDate;
        }
        return Task.CompletedTask;
    }
}

public class InMemoryDepartmentRepository : IDepartmentRepository
{
    private readonly InMemoryDataStore _db;
    public InMemoryDepartmentRepository(InMemoryDataStore db) => _db = db;

    public Task<IEnumerable<Department>> GetAllDepartmentsAsync() =>
        Task.FromResult<IEnumerable<Department>>(_db.Departments.ToList());

    public Task<Department?> GetDepartmentByIdAsync(int id) =>
        Task.FromResult(_db.Departments.FirstOrDefault(d => d.Id == id));

    public Task AddDepartmentAsync(Department department)
    {
        department.Id = _db.Departments.Any() ? _db.Departments.Max(d => d.Id) + 1 : 1;
        _db.Departments.Add(department);
        return Task.CompletedTask;
    }

    public Task UpdateDepartmentAsync(Department department)
    {
        var existing = _db.Departments.FirstOrDefault(d => d.Id == department.Id);
        if (existing != null)
        {
            existing.DepartmentName = department.DepartmentName;
            existing.Description = department.Description;
        }
        return Task.CompletedTask;
    }
}

public class InMemoryAttendanceRepository : IAttendanceRepository
{
    private readonly InMemoryDataStore _db;
    public InMemoryAttendanceRepository(InMemoryDataStore db) => _db = db;

    public Task<IEnumerable<TimeAttendance>> GetAttendancesByEmployeeAsync(int employeeId, int month, int year)
    {
        var list = _db.TimeAttendances.Where(a => a.EmployeeId == employeeId && a.WorkDate.Month == month && a.WorkDate.Year == year);
        return Task.FromResult<IEnumerable<TimeAttendance>>(list.ToList());
    }

    public Task<IEnumerable<TimeAttendance>> GetAttendancesByMonthAsync(int month, int year)
    {
        var list = _db.TimeAttendances.Where(a => a.WorkDate.Month == month && a.WorkDate.Year == year);
        return Task.FromResult<IEnumerable<TimeAttendance>>(list.ToList());
    }

    public Task AddAttendanceAsync(TimeAttendance attendance)
    {
        attendance.Id = _db.TimeAttendances.Any() ? _db.TimeAttendances.Max(a => a.Id) + 1 : 1;
        _db.TimeAttendances.Add(attendance);
        return Task.CompletedTask;
    }

    public Task AddRangeAttendanceAsync(IEnumerable<TimeAttendance> attendances)
    {
        foreach (var att in attendances)
        {
            att.Id = _db.TimeAttendances.Any() ? _db.TimeAttendances.Max(a => a.Id) + 1 : 1;
            _db.TimeAttendances.Add(att);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAttendanceAsync(TimeAttendance attendance)
    {
        var existing = _db.TimeAttendances.FirstOrDefault(a => a.Id == attendance.Id);
        if (existing != null)
        {
            existing.CheckIn = attendance.CheckIn;
            existing.CheckOut = attendance.CheckOut;
            existing.ActualWorkDays = attendance.ActualWorkDays;
            existing.OvertimeHours = attendance.OvertimeHours;
            existing.LateMinutes = attendance.LateMinutes;
            existing.Status = attendance.Status;
            existing.Note = attendance.Note;
        }
        return Task.CompletedTask;
    }
}

public class InMemoryDeductionRateRepository : IDeductionRateRepository
{
    private readonly InMemoryDataStore _db;
    public InMemoryDeductionRateRepository(InMemoryDataStore db) => _db = db;

    public Task<IEnumerable<DeductionRate>> GetActiveDeductionRatesAsync() =>
        Task.FromResult<IEnumerable<DeductionRate>>(_db.DeductionRates.Where(r => r.IsActive).ToList());

    public Task<DeductionRate?> GetByCodeAsync(string code) =>
        Task.FromResult(_db.DeductionRates.FirstOrDefault(r => string.Equals(r.Code, code, StringComparison.OrdinalIgnoreCase)));

    public Task UpdateDeductionRateAsync(DeductionRate rate)
    {
        var existing = _db.DeductionRates.FirstOrDefault(r => r.Id == rate.Id);
        if (existing != null)
        {
            existing.RatePercentage = rate.RatePercentage;
            existing.FlatAmount = rate.FlatAmount;
            existing.IsActive = rate.IsActive;
        }
        return Task.CompletedTask;
    }
}

public class InMemoryPayslipRepository : IPayslipRepository
{
    private readonly InMemoryDataStore _db;
    public InMemoryPayslipRepository(InMemoryDataStore db) => _db = db;

    public Task<IEnumerable<Payslip>> GetPayslipsByPeriodAsync(int month, int year)
    {
        var list = _db.Payslips.Where(p => p.Month == month && p.Year == year);
        foreach (var slip in list)
        {
            slip.Employee ??= _db.Employees.FirstOrDefault(e => e.Id == slip.EmployeeId);
        }
        return Task.FromResult<IEnumerable<Payslip>>(list.ToList());
    }

    public Task<Payslip?> GetPayslipByIdAsync(int id)
    {
        var slip = _db.Payslips.FirstOrDefault(p => p.Id == id);
        if (slip != null)
        {
            slip.Employee ??= _db.Employees.FirstOrDefault(e => e.Id == slip.EmployeeId);
        }
        return Task.FromResult(slip);
    }

    public Task<Payslip?> GetPayslipByEmployeeAndPeriodAsync(int employeeId, int month, int year)
    {
        var slip = _db.Payslips.FirstOrDefault(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);
        if (slip != null)
        {
            slip.Employee ??= _db.Employees.FirstOrDefault(e => e.Id == slip.EmployeeId);
        }
        return Task.FromResult(slip);
    }

    public Task<IEnumerable<Payslip>> GetPayslipsByEmployeeEmailAsync(string email)
    {
        var emp = _db.Employees.FirstOrDefault(e => string.Equals(e.Email, email, StringComparison.OrdinalIgnoreCase));
        if (emp == null) return Task.FromResult<IEnumerable<Payslip>>(new List<Payslip>());

        var list = _db.Payslips.Where(p => p.EmployeeId == emp.Id).ToList();
        foreach (var slip in list)
        {
            slip.Employee = emp;
        }
        return Task.FromResult<IEnumerable<Payslip>>(list);
    }

    public Task<bool> IsPeriodLockedAsync(int month, int year)
    {
        return Task.FromResult(_db.Payslips.Any(p => p.Month == month && p.Year == year && p.Status == PayslipStatus.Locked));
    }

    public Task AddPayslipAsync(Payslip payslip)
    {
        payslip.Id = _db.Payslips.Any() ? _db.Payslips.Max(p => p.Id) + 1 : 1;
        int lineId = 1;
        foreach (var line in payslip.Lines)
        {
            line.Id = lineId++;
            line.PayslipId = payslip.Id;
        }
        _db.Payslips.Add(payslip);
        return Task.CompletedTask;
    }

    public Task AddPayslipsBatchAsync(IEnumerable<Payslip> payslips)
    {
        int nextId = _db.Payslips.Any() ? _db.Payslips.Max(p => p.Id) + 1 : 1;
        foreach (var slip in payslips)
        {
            slip.Id = nextId++;
            int lineId = 1;
            foreach (var line in slip.Lines)
            {
                line.Id = lineId++;
                line.PayslipId = slip.Id;
            }
            _db.Payslips.Add(slip);
        }
        return Task.CompletedTask;
    }

    public Task UpdatePayslipAsync(Payslip payslip)
    {
        var existing = _db.Payslips.FirstOrDefault(p => p.Id == payslip.Id);
        if (existing != null)
        {
            existing.Status = payslip.Status;
            existing.LockedAt = payslip.LockedAt;
            existing.LockedBy = payslip.LockedBy;
            existing.NetSalary = payslip.NetSalary;
            existing.GrossSalary = payslip.GrossSalary;
            existing.TotalDeductions = payslip.TotalDeductions;
        }
        return Task.CompletedTask;
    }

    public Task DeletePayslipsByPeriodAsync(int month, int year)
    {
        _db.Payslips.RemoveAll(p => p.Month == month && p.Year == year && p.Status != PayslipStatus.Locked);
        return Task.CompletedTask;
    }
}
