using ChamCong_TinhLuong.CoreBusiness.Enums;
using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChamCong_TinhLuong.Plugins.DataStore.SQL;

public class SqlUnitOfWork : IUnitOfWork
{
    private readonly ChamCongDbContext _context;
    private IDbContextTransaction? _transaction;

    public SqlUnitOfWork(ChamCongDbContext context)
    {
        _context = context;
    }

    public async Task<IDisposable> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction;
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        _context.ChangeTracker.Clear();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

public class SqlEmployeeRepository : IEmployeeRepository
{
    private readonly ChamCongDbContext _db;
    public SqlEmployeeRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(bool includeInactive = false)
    {
        // Parameterized EF Core Query
        var query = _db.Employees.Include(e => e.Department).AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }
        return await query.OrderBy(e => e.EmployeeCode).ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Contracts)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string code)
    {
        return await _db.Employees
            .Include(e => e.Department)
            .Include(e => e.Contracts)
            .FirstOrDefaultAsync(e => e.EmployeeCode == code);
    }

    public async Task<Employee?> GetEmployeeByEmailAsync(string email)
    {
        return await _db.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        await _db.Employees.AddAsync(employee);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp != null)
        {
            emp.IsActive = false; // Soft delete
            await _db.SaveChangesAsync();
        }
    }
}

public class SqlContractRepository : IContractRepository
{
    private readonly ChamCongDbContext _db;
    public SqlContractRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<Contract>> GetContractsByEmployeeIdAsync(int employeeId)
    {
        return await _db.Contracts
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId)
    {
        return await _db.Contracts
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.IsCurrent);
    }

    public async Task AddContractAsync(Contract contract)
    {
        await _db.Contracts.AddAsync(contract);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateContractAsync(Contract contract)
    {
        _db.Contracts.Update(contract);
        await _db.SaveChangesAsync();
    }
}

public class SqlDepartmentRepository : IDepartmentRepository
{
    private readonly ChamCongDbContext _db;
    public SqlDepartmentRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _db.Departments.AsNoTracking().ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _db.Departments.FindAsync(id);
    }

    public async Task AddDepartmentAsync(Department department)
    {
        await _db.Departments.AddAsync(department);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        _db.Departments.Update(department);
        await _db.SaveChangesAsync();
    }
}

public class SqlAttendanceRepository : IAttendanceRepository
{
    private readonly ChamCongDbContext _db;
    public SqlAttendanceRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<TimeAttendance>> GetAttendancesByEmployeeAsync(int employeeId, int month, int year)
    {
        return await _db.TimeAttendances
            .Include(a => a.WorkShift)
            .Where(a => a.EmployeeId == employeeId && a.WorkDate.Month == month && a.WorkDate.Year == year)
            .OrderBy(a => a.WorkDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeAttendance>> GetAttendancesByMonthAsync(int month, int year)
    {
        return await _db.TimeAttendances
            .Include(a => a.Employee)
            .Include(a => a.WorkShift)
            .Where(a => a.WorkDate.Month == month && a.WorkDate.Year == year)
            .OrderBy(a => a.WorkDate)
            .ToListAsync();
    }

    public async Task<TimeAttendance?> GetAttendanceByIdAsync(int id)
    {
        return await _db.TimeAttendances
            .Include(a => a.Employee)
            .Include(a => a.WorkShift)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<TimeAttendance?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateTime workDate)
    {
        return await _db.TimeAttendances
            .Include(a => a.Employee)
            .Include(a => a.WorkShift)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.WorkDate.Date == workDate.Date);
    }

    public async Task<IEnumerable<TimeAttendance>> GetPendingAttendancesAsync()
    {
        return await _db.TimeAttendances
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
            .Include(a => a.WorkShift)
            .Where(a => a.Status == AttendanceStatus.PendingApproval)
            .OrderByDescending(a => a.WorkDate)
            .ThenByDescending(a => a.CheckIn)
            .ToListAsync();
    }

    public async Task AddAttendanceAsync(TimeAttendance attendance)
    {
        await _db.TimeAttendances.AddAsync(attendance);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAttendanceAsync(IEnumerable<TimeAttendance> attendances)
    {
        await _db.TimeAttendances.AddRangeAsync(attendances);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAttendanceAsync(TimeAttendance attendance)
    {
        _db.TimeAttendances.Update(attendance);
        await _db.SaveChangesAsync();
    }
}

public class SqlDeductionRateRepository : IDeductionRateRepository
{
    private readonly ChamCongDbContext _db;
    public SqlDeductionRateRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<DeductionRate>> GetActiveDeductionRatesAsync()
    {
        return await _db.DeductionRates
            .Where(r => r.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DeductionRate?> GetByCodeAsync(string code)
    {
        return await _db.DeductionRates.FirstOrDefaultAsync(r => r.Code == code);
    }

    public async Task UpdateDeductionRateAsync(DeductionRate rate)
    {
        _db.DeductionRates.Update(rate);
        await _db.SaveChangesAsync();
    }
}

public class SqlPayslipRepository : IPayslipRepository
{
    private readonly ChamCongDbContext _db;
    public SqlPayslipRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<Payslip>> GetPayslipsByPeriodAsync(int month, int year)
    {
        return await _db.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
            .Include(p => p.Lines)
            .Where(p => p.Month == month && p.Year == year)
            .OrderBy(p => p.Employee!.EmployeeCode)
            .ToListAsync();
    }

    public async Task<Payslip?> GetPayslipByIdAsync(int id)
    {
        return await _db.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payslip?> GetPayslipByEmployeeAndPeriodAsync(int employeeId, int month, int year)
    {
        return await _db.Payslips
            .Include(p => p.Employee)
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);
    }

    public async Task<IEnumerable<Payslip>> GetPayslipsByEmployeeEmailAsync(string email)
    {
        // Row-Level Security parameterized query
        return await _db.Payslips
            .Include(p => p.Employee)
            .Include(p => p.Lines)
            .Where(p => p.Employee != null && p.Employee.Email == email)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }

    public async Task<bool> IsPeriodLockedAsync(int month, int year)
    {
        return await _db.Payslips
            .AnyAsync(p => p.Month == month && p.Year == year && p.Status == PayslipStatus.Locked);
    }

    public async Task AddPayslipAsync(Payslip payslip)
    {
        await _db.Payslips.AddAsync(payslip);
        await _db.SaveChangesAsync();
    }

    public async Task AddPayslipsBatchAsync(IEnumerable<Payslip> payslips)
    {
        await _db.Payslips.AddRangeAsync(payslips);
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePayslipAsync(Payslip payslip)
    {
        _db.Payslips.Update(payslip);
        await _db.SaveChangesAsync();
    }

    public async Task DeletePayslipsByPeriodAsync(int month, int year)
    {
        var existing = await _db.Payslips
            .Where(p => p.Month == month && p.Year == year && p.Status != PayslipStatus.Locked)
            .ToListAsync();

        if (existing.Any())
        {
            _db.Payslips.RemoveRange(existing);
            await _db.SaveChangesAsync();
        }
    }
}

public class SqlAppUserRepository : IAppUserRepository
{
    private readonly ChamCongDbContext _db;
    public SqlAppUserRepository(ChamCongDbContext db) => _db = db;

    public async Task<AppUser?> GetByUsernameAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return null;
        var term = identifier.Trim().ToLower();

        return await _db.AppUsers
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
            .FirstOrDefaultAsync(u => u.IsActive && 
                (u.Username.ToLower() == term || 
                 (u.Employee != null && (u.Employee.EmployeeCode.ToLower() == term || 
                                         (u.Employee.Email != null && u.Employee.Email.ToLower() == term)))));
    }

    public async Task<AppUser?> GetByEmployeeIdAsync(int employeeId)
    {
        return await _db.AppUsers
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
    }

    public async Task AddUserAsync(AppUser user)
    {
        await _db.AppUsers.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(AppUser user)
    {
        _db.AppUsers.Update(user);
        await _db.SaveChangesAsync();
    }
}

public class SqlLeaveRequestRepository : ILeaveRequestRepository
{
    private readonly ChamCongDbContext _db;
    public SqlLeaveRequestRepository(ChamCongDbContext db) => _db = db;

    public async Task<IEnumerable<LeaveRequest>> GetApprovedLeaveRequestsByMonthAsync(int employeeId, int month, int year)
    {
        return await _db.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId &&
                         lr.Status == LeaveRequestStatus.Approved &&
                         (lr.StartDate.Month == month && lr.StartDate.Year == year ||
                          lr.EndDate.Month == month && lr.EndDate.Year == year))
            .OrderBy(lr => lr.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync(int? employeeId = null)
    {
        var query = _db.LeaveRequests.Include(lr => lr.Employee).AsQueryable();
        if (employeeId.HasValue && employeeId.Value > 0)
        {
            query = query.Where(lr => lr.EmployeeId == employeeId.Value);
        }
        return await query.OrderByDescending(lr => lr.CreatedAt).ToListAsync();
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id)
    {
        return await _db.LeaveRequests.Include(lr => lr.Employee).FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task AddLeaveRequestAsync(LeaveRequest request)
    {
        await _db.LeaveRequests.AddAsync(request);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLeaveRequestAsync(LeaveRequest request)
    {
        _db.LeaveRequests.Update(request);
        await _db.SaveChangesAsync();
    }
}

public class SqlNotificationLogRepository : INotificationLogRepository
{
    private readonly ChamCongDbContext _db;
    public SqlNotificationLogRepository(ChamCongDbContext db) => _db = db;

    public async Task AddLogAsync(NotificationLog log)
    {
        await _db.NotificationLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationLog>> GetLogsByPeriodAsync(int month, int year)
    {
        return await _db.NotificationLogs
            .Include(n => n.Employee)
            .Include(n => n.Payslip)
            .Where(n => n.SentDate.Month == month && n.SentDate.Year == year)
            .OrderByDescending(n => n.SentDate)
            .ToListAsync();
    }
}
