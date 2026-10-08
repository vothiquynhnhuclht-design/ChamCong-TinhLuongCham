using ChamCong_TinhLuong.Components;
using ChamCong_TinhLuong.Plugins.DataStore.SQL;
using ChamCong_TinhLuong.UseCases.Payroll;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;
using ChamCong_TinhLuong.UseCases.Services;
using Microsoft.EntityFrameworkCore;

namespace ChamCong_TinhLuong
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // 1. Configure EF Core SQL Server DbContext (Sprint 2 - Week 06)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=ChamCong_TinhLuong_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            builder.Services.AddDbContext<ChamCongDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Register SQL Server Repositories & UnitOfWork (Parameterized Queries)
            builder.Services.AddScoped<IUnitOfWork, SqlUnitOfWork>();
            builder.Services.AddScoped<IEmployeeRepository, SqlEmployeeRepository>();
            builder.Services.AddScoped<IContractRepository, SqlContractRepository>();
            builder.Services.AddScoped<IDepartmentRepository, SqlDepartmentRepository>();
            builder.Services.AddScoped<IAttendanceRepository, SqlAttendanceRepository>();
            builder.Services.AddScoped<IDeductionRateRepository, SqlDeductionRateRepository>();
            builder.Services.AddScoped<IPayslipRepository, SqlPayslipRepository>();

            // 3. Register Scoped State Management (Draft Timesheet - ShoppingCart pattern)
            builder.Services.AddScoped<AttendanceDraftService>();

            // 4. Register UseCases
            builder.Services.AddTransient<CalculatePayrollUseCase>();
            builder.Services.AddTransient<LockPayrollPeriodUseCase>();
            builder.Services.AddTransient<ViewEmployeePayslipUseCase>();

            var app = builder.Build();

            // Auto-migrate and Seed Database on startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ChamCongDbContext>();
                    await DatabaseSeeder.SeedAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Lỗi xảy ra khi khởi tạo và seed dữ liệu SQL Server.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            await app.RunAsync();
        }
    }
}
