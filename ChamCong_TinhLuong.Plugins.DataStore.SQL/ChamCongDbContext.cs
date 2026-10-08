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
        });

        // 4. TimeAttendance Configuration
        modelBuilder.Entity<TimeAttendance>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.Employee)
                  .WithMany(e => e.TimeAttendances)
                  .HasForeignKey(t => t.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => new { t.EmployeeId, t.WorkDate });
        });

        // 5. Payslip Header Configuration
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
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => new { p.EmployeeId, p.Month, p.Year });
        });

        // 6. PayslipLine Detail Configuration (Header - Line)
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
        });
    }
}
