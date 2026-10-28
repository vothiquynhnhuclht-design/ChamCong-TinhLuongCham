using ChamCong_TinhLuong.UseCases.PluginInterfaces;
using Microsoft.Extensions.Logging;

namespace ChamCong_TinhLuong.UseCases.AI;

/// <summary>
/// Human-in-the-loop: chỉ được gọi khi người dùng ĐÃ XEM, (có thể) CHỈNH SỬA và BẤM XÁC NHẬN.
/// Use Case này không biết gì về AI — nó chỉ lưu nội dung do người dùng xác nhận.
/// </summary>
public sealed class SaveConfirmedAttendanceNoteUseCase
{
    private const int MaxNoteLength = 1000;

    private readonly IAttendanceRepository _attendanceRepo;
    private readonly ILogger<SaveConfirmedAttendanceNoteUseCase> _logger;

    public SaveConfirmedAttendanceNoteUseCase(IAttendanceRepository attendanceRepo, ILogger<SaveConfirmedAttendanceNoteUseCase> logger)
    {
        _attendanceRepo = attendanceRepo;
        _logger = logger;
    }

    /// <param name="currentEmployeeId">Nhân viên đang đăng nhập — chỉ được sửa bản ghi của chính mình.</param>
    public async Task<(bool Ok, string Message)> ExecuteAsync(int attendanceId, int currentEmployeeId, string confirmedNote)
    {
        if (string.IsNullOrWhiteSpace(confirmedNote))
            return (false, "Nội dung ghi chú trống.");

        try
        {
            var record = await _attendanceRepo.GetAttendanceByIdAsync(attendanceId);
            if (record is null || record.EmployeeId != currentEmployeeId)
                return (false, "Không tìm thấy bản ghi chấm công hợp lệ.");

            var note = confirmedNote.Trim();
            record.Note = note.Length > MaxNoteLength ? note[..MaxNoteLength] : note;
            await _attendanceRepo.UpdateAttendanceAsync(record);
            return (true, "Đã lưu ghi chú.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu ghi chú chấm công {AttendanceId}", attendanceId);
            return (false, "Không thể lưu ghi chú. Vui lòng thử lại.");
        }
    }
}
