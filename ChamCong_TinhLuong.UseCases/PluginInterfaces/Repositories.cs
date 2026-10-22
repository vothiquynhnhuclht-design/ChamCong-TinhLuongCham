using ChamCong_TinhLuong.CoreBusiness.Models;

namespace ChamCong_TinhLuong.UseCases.PluginInterfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync(bool includeInactive = false);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee?> GetEmployeeByCodeAsync(string code);
    Task<Employee?> GetEmployeeByEmailAsync(string email);
    Task AddEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
    Task DeleteEmployeeAsync(int id);
}

public interface IContractRepository
{
    Task<IEnumerable<Contract>> GetContractsByEmployeeIdAsync(int employeeId);
    Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId);
    Task AddContractAsync(Contract contract);
    Task UpdateContractAsync(Contract contract);
}

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task AddDepartmentAsync(Department department);
    Task UpdateDepartmentAsync(Department department);
}

public interface IAttendanceRepository
{
    Task<IEnumerable<TimeAttendance>> GetAttendancesByEmployeeAsync(int employeeId, int month, int year);
    Task<IEnumerable<TimeAttendance>> GetAttendancesByMonthAsync(int month, int year);
    Task<TimeAttendance?> GetAttendanceByIdAsync(int id);
    Task<TimeAttendance?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateTime workDate);
    Task<IEnumerable<TimeAttendance>> GetPendingAttendancesAsync();
    Task AddAttendanceAsync(TimeAttendance attendance);
    Task AddRangeAttendanceAsync(IEnumerable<TimeAttendance> attendances);
    Task UpdateAttendanceAsync(TimeAttendance attendance);
}

public interface IDeductionRateRepository
{
    Task<IEnumerable<DeductionRate>> GetActiveDeductionRatesAsync();
    Task<DeductionRate?> GetByCodeAsync(string code);
    Task UpdateDeductionRateAsync(DeductionRate rate);
}

public interface IPayslipRepository
{
    Task<IEnumerable<Payslip>> GetPayslipsByPeriodAsync(int month, int year);
    Task<Payslip?> GetPayslipByIdAsync(int id);
    Task<Payslip?> GetPayslipByEmployeeAndPeriodAsync(int employeeId, int month, int year);
    Task<IEnumerable<Payslip>> GetPayslipsByEmployeeEmailAsync(string email);
    Task<bool> IsPeriodLockedAsync(int month, int year);
    Task AddPayslipAsync(Payslip payslip);
    Task AddPayslipsBatchAsync(IEnumerable<Payslip> payslips);
    Task UpdatePayslipAsync(Payslip payslip);
    Task DeletePayslipsByPeriodAsync(int month, int year);
}

public interface IAppUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser?> GetByEmployeeIdAsync(int employeeId);
    Task AddUserAsync(AppUser user);
    Task UpdateUserAsync(AppUser user);
}

public interface ILeaveRequestRepository
{
    Task<IEnumerable<LeaveRequest>> GetApprovedLeaveRequestsByMonthAsync(int employeeId, int month, int year);
    Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync(int? employeeId = null);
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task AddLeaveRequestAsync(LeaveRequest request);
    Task UpdateLeaveRequestAsync(LeaveRequest request);
}

public interface INotificationLogRepository
{
    Task AddLogAsync(NotificationLog log);
    Task<IEnumerable<NotificationLog>> GetLogsByPeriodAsync(int month, int year);
}

public interface IUnitOfWork
{
    Task<IDisposable> BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
}
