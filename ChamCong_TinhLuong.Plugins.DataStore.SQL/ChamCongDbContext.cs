using ChamCong_TinhLuong.CoreBusiness.Models;
using Microsoft.EntityFrameworkCore;

namespace ChamCong_TinhLuong.Plugins.DataStore.SQL;

public class ChamCongDbContext : DbContext
{
    public ChamCongDbContext(DbContextOptions<ChamCongDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<DeductionRate> DeductionRates => Set<DeductionRate>();
    public DbSet<TimeAttendance> TimeAttendances => Set<TimeAttendance>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<PayslipLine> PayslipLines => Set<PayslipLine>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.EmployeeCode).IsUnique(); // Ràng buộc mã nhân viên là duy nhất
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 2. Contract Configuration (Tách biệt khỏi Employee)
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ContractNumber).IsRequired().HasMaxLength(50);
            entity.Property(c => c.BasicSalary).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Allowance).HasColumnType("decimal(18,2)");
            entity.Property(c => c.InsuranceSalary).HasColumnType("decimal(18,2)");

            entity.HasOne(c => c.Employee)
                  .WithMany(e => e.Contracts)
                  .HasForeignKey(c => c.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. DeductionRate Configuration (Cấu hình trích nộp động)
        modelBuilder.Entity<DeductionRate>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.Code).IsUnique();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(150);
            entity.Property(r => r.RatePercentage).HasColumnType("decimal(6,4)");
            entity.Property(r => r.FlatAmount).HasColumnType("decimal(18,2)");
            entity.Property(r => r.ApplyOn).HasMaxLength(50);
            entity.Property(r => r.RegulationSource).HasMaxLength(255);
        });

        // 4. WorkShift Configuration
        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.ShiftCode).IsRequired().HasMaxLength(50);
            entity.Property(w => w.ShiftName).IsRequired().HasMaxLength(100);
        });

        // 5. TimeAttendance Configuration
        modelBuilder.Entity<TimeAttendance>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.Employee)
                  .WithMany(e => e.TimeAttendances)
                  .HasForeignKey(t => t.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Nối WorkShifts 1 - N TimeAttendances
            entity.HasOne(t => t.WorkShift)
                  .WithMany(w => w.TimeAttendances)
                  .HasForeignKey(t => t.WorkShiftId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(t => t.PhotoPath).HasMaxLength(500);
            entity.Property(t => t.CheckOutPhotoPath).HasMaxLength(500);

            entity.HasIndex(t => new { t.EmployeeId, t.WorkDate });
        });

        // 6. Payslip Header Configuration
        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.BasicSalary).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Allowance).HasColumnType("decimal(18,2)");
            entity.Property(p => p.GrossSalary).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TotalDeductions).HasColumnType("decimal(18,2)");
            entity.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");
            entity.Property(p => p.LockedBy).HasMaxLength(100);

            entity.HasOne(p => p.Employee)
                  .WithMany(e => e.Payslips)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PayrollPeriod)
                  .WithMany(period => period.Payslips)
                  .HasForeignKey(p => p.PayrollPeriodId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(p => new { p.EmployeeId, p.Month, p.Year }).IsUnique();
        });

        // 7. PayslipLine Detail Configuration (Header - Line)
        modelBuilder.Entity<PayslipLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ItemCode).IsRequired().HasMaxLength(50);
            entity.Property(l => l.ItemName).IsRequired().HasMaxLength(150);
            entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.RateApplied).HasColumnType("decimal(6,4)");
            entity.Property(l => l.CalculationNote).HasMaxLength(255);

            entity.HasOne(l => l.Payslip)
                  .WithMany(p => p.Lines)
                  .HasForeignKey(l => l.PayslipId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Nối DeductionRates 1 - N PayslipLines
            entity.HasOne(l => l.DeductionRate)
                  .WithMany(d => d.PayslipLines)
                  .HasForeignKey(l => l.DeductionRateId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 8. PayrollPeriod Configuration
        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.StandardWorkDays).HasColumnType("decimal(4,1)");
            entity.Property(p => p.LockedBy).HasMaxLength(100);
            entity.Property(p => p.Note).HasMaxLength(255);
            entity.HasIndex(p => new { p.Month, p.Year }).IsUnique();
        });

        // 9. AppUser Configuration (Authentication & Authorization)
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);

            entity.HasOne(u => u.Employee)
                  .WithOne(e => e.AppUser)
                  .HasForeignKey<AppUser>(u => u.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 10. LeaveRequest Configuration (Đơn xin nghỉ phép)
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(lr => lr.Id);
            entity.Property(lr => lr.Reason).IsRequired().HasMaxLength(500);
            entity.Property(lr => lr.ApprovedBy).HasMaxLength(100);
            entity.Property(lr => lr.Note).HasMaxLength(500);

            entity.HasOne(lr => lr.Employee)
                  .WithMany(e => e.LeaveRequests)
                  .HasForeignKey(lr => lr.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(lr => new { lr.EmployeeId, lr.StartDate, lr.EndDate });
        });

        // 11. NotificationLog Configuration (Lịch sử gửi email/thông báo)
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.RecipientEmail).IsRequired().HasMaxLength(150);
            entity.Property(n => n.Subject).IsRequired().HasMaxLength(255);
            entity.Property(n => n.ErrorMessage).HasMaxLength(1000);

            entity.HasOne(n => n.Employee)
                  .WithMany(e => e.NotificationLogs)
                  .HasForeignKey(n => n.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Payslip)
                  .WithMany(p => p.NotificationLogs)
                  .HasForeignKey(n => n.PayslipId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
