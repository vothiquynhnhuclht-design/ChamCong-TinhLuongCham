using ChamCong_TinhLuong.CoreBusiness.Models;
using ChamCong_TinhLuong.UseCases.PluginInterfaces;

namespace ChamCong_TinhLuong.UseCases.Employees;

/// <summary>
/// Use Case lưu thông tin Nhân viên (Header) kèm Hợp đồng lao động (Line) trong một Transaction nguyên tử (ACID).
/// Đảm bảo nếu lưu Hợp đồng thất bại thì bản ghi Nhân viên cũng được Rollback, không sinh dữ liệu mồ côi.
/// </summary>
public class SaveEmployeeWithContractUseCase
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IContractRepository _contractRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SaveEmployeeWithContractUseCase(
        IEmployeeRepository employeeRepo,
        IContractRepository contractRepo,
        IUnitOfWork unitOfWork)
    {
        _employeeRepo = employeeRepo;
        _contractRepo = contractRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Employee employee, Contract contract, bool isEditing)
    {
        using (await _unitOfWork.BeginTransactionAsync())
        {
            try
            {
                if (isEditing)
                {
                    await _employeeRepo.UpdateEmployeeAsync(employee);
                    contract.EmployeeId = employee.Id;
                    if (contract.Id > 0)
                        await _contractRepo.UpdateContractAsync(contract);
                    else
                        await _contractRepo.AddContractAsync(contract);
                }
                else
                {
                    await _employeeRepo.AddEmployeeAsync(employee);
                    contract.EmployeeId = employee.Id;
                    await _contractRepo.AddContractAsync(contract);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
