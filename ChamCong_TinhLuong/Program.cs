using ChamCong_TinhLuong.Components;
using ChamCong_TinhLuong.Plugins.DataStore.InMemory;
using ChamCong_TinhLuong.UseCases.Payroll;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;
using ChamCong_TinhLuong.UseCases.Services;

namespace ChamCong_TinhLuong
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // 1. Register DataStore (In-Memory for Week 05 milestone)
            builder.Services.AddSingleton<InMemoryDataStore>();
            builder.Services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
            builder.Services.AddScoped<IEmployeeRepository, InMemoryEmployeeRepository>();
            builder.Services.AddScoped<IContractRepository, InMemoryContractRepository>();
            builder.Services.AddScoped<IDepartmentRepository, InMemoryDepartmentRepository>();
            builder.Services.AddScoped<IAttendanceRepository, InMemoryAttendanceRepository>();
            builder.Services.AddScoped<IDeductionRateRepository, InMemoryDeductionRateRepository>();
            builder.Services.AddScoped<IPayslipRepository, InMemoryPayslipRepository>();

            // 2. Register Scoped State Management (Draft Timesheet - ShoppingCart pattern)
            builder.Services.AddScoped<AttendanceDraftService>();

            // 3. Register UseCases
            builder.Services.AddTransient<CalculatePayrollUseCase>();
            builder.Services.AddTransient<LockPayrollPeriodUseCase>();
            builder.Services.AddTransient<ViewEmployeePayslipUseCase>();

            var app = builder.Build();

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

            app.Run();
        }
    }
}
