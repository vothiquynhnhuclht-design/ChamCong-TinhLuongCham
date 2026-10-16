# BÁO CÁO TIẾN ĐỘ & BẰNG CHỨNG CHẤM ĐIỂM (GRADING EVIDENCE)
## ĐỀ TÀI 17: CHẤM CÔNG & TÍNH LƯƠNG (TIMESHEET & PAYROLL SYSTEM)

---

## 🎯 ĐỐI CHIẾU 100% YÊU CẦU CAPSTONE ĐỀ TÀI 17 CỦA GIẢNG VIÊN

| Mục | Yêu Cầu Của Giảng Viên | Triển Khai Thực Tế Trong Đồ Án | Trạng Thái |
| :--- | :--- | :--- | :---: |
| **Độ khó** | ★★★ | Kiến trúc Clean Architecture 4 tầng, ACID Transaction, Row-Level Security, AI Copilot | ✅ Đạt |
| **Bối cảnh** | Doanh nghiệp nhỏ 20–50 nhân viên tính lương hàng tháng bằng Excel, hay sai. | Xây dựng hệ thống tự động hóa hoàn toàn luồng Chấm công $\rightarrow$ Tính lương $\rightarrow$ Khóa sổ $\rightarrow$ Xuất báo cáo, thay thế file Excel thủ công. | ✅ Đạt |
| **Vai trò** | **Nhân viên**; **Kế toán / Nhân sự** | Tích hợp **Bộ chuyển vai trò (Role Switcher)** ngay trên thanh Header bar: Kế toán quản trị toàn quyền vs Nhân viên chỉ xem dữ liệu chính mình. | ✅ Đạt |
| **Entities** | `Department`, `Employee`, `Contract` (lương cơ bản, phụ cấp), `Timesheet`, `PayrollPeriod`, `Payslip`, `PayslipLine`, `DeductionRate` | Đầy đủ 8 Entities chuẩn 3NF: <br>• `Department.cs`<br>• `Employee.cs`<br>• `Contract.cs` (tách riêng lương, phụ cấp)<br>• `Timesheet.cs` / `TimeAttendance.cs`<br>• `PayrollPeriod.cs`<br>• `Payslip.cs`<br>• `PayslipLine.cs`<br>• `DeductionRate.cs` (kèm nguồn quy định luật) | ✅ Đạt |
| **Ánh xạ eShop** | • Product $\rightarrow$ Nhân viên / hợp đồng<br>• Giỏ $\rightarrow$ Bảng công nháp<br>• Order $\rightarrow$ Bảng lương kỳ<br>• LineItem $\rightarrow$ Phiếu lương và các khoản<br>• Admin xử lý đơn $\rightarrow$ Duyệt công, chốt kỳ | • `Employee` & `Contract` quản lý như Product<br>• `AttendanceDraftService` (Scoped ShoppingCart pattern)<br>• `Payslip` tổng hợp Header kỳ lương (Order)<br>• `PayslipLine` chi tiết từng khoản thu nhập/khấu trừ (LineItem)<br>• Action Chốt duyệt công & Khóa sổ kỳ lương | ✅ Đạt |
| **Use case Người dùng** | Xem bảng công; xem phiếu lương của chính mình | Khi chọn vai trò **Nhân viên**, hệ thống kích hoạt **Row-Level Security**: Chỉ hiển thị bảng công và phiếu lương của chính mình (chặn xem của người khác). | ✅ Đạt |
| **Use case Quản trị** | CRUD nhân viên, hợp đồng; nhập/duyệt bảng công; tính lương kỳ; chốt kỳ; **xuất bảng lương** | Đầy đủ giao diện quản trị:<br>• CRUD Nhân viên & Hợp đồng lao động (`/employees`)<br>• Nhập/Duyệt bảng công nháp (`/attendance`)<br>• Động cơ tính lương trong Transaction (`/payroll`)<br>• Khóa sổ kỳ lương (`Status = Locked`)<br>• **Nút "Xuất Bảng Lương (CSV)"** chuẩn UTF-8 BOM mở bằng Excel. | ✅ Đạt |
| **Luật nghiệp vụ (1)** | Lương = lương cơ bản x ngày công thực tế / ngày công chuẩn + phụ cấp - các khoản trích; tỷ lệ trích lưu trong `DeductionRate`, không hardcode, nhóm ghi nguồn quy định đã dùng. | Cài đặt chính xác trong `CalculatePayrollUseCase.cs`. Bảng `DeductionRate` lưu động tỷ lệ trích và **Nguồn quy định pháp luật**:<br>• BHXH 8%: Luật BHXH số 58/2014/QH13, Điều 85<br>• BHYT 1.5%: Luật BHYT số 46/2014/QH13, Điều 13<br>• BHTN 1%: Luật Việc làm số 38/2013/QH13, Điều 57<br>• Giảm trừ bản thân (11tr) & Phụ thuộc (4.4tr): Nghị quyết 954/2020/UBTVQH14 | ✅ Đạt |
| **Luật nghiệp vụ (2)** | Kỳ đã chốt không tính lại; nhân viên chỉ xem được phiếu lương của mình. | • Kỳ đã khóa sổ (`Locked`) ném ngoại lệ `PayslipLockedException` chặn đứng mọi thao tác tính lại.<br>• Phân quyền cấp dòng (Row-Level Security) lọc phiếu lương theo `EmployeeId`. | ✅ Đạt |
| **Điểm nhấn kỹ thuật** | Phân quyền theo dữ liệu (không chỉ theo trang); tính toán tài chính trong transaction | • `UserSessionService` quản lý phân quyền theo dữ liệu người dùng đang đăng nhập.<br>• `SqlUnitOfWork` bọc toàn bộ quá trình tính lương trong Database Transaction (`IDbContextTransaction`), tự động rollback nếu có lỗi. | ✅ Đạt |
| **Mở rộng AI** | Trợ lý trả lời "vì sao lương tháng này giảm?" dựa trên chính phiếu lương của nhân viên | Trang `AiAssistant.razor` inject `IChatClient` (chuẩn `Microsoft.Extensions.AI`) đối soát trực tiếp giữa 2 kỳ lương liền kề, bóc tách chính xác nguyên nhân giảm lương (công, OT, bảo hiểm, thuế). | ✅ Đạt |

---

## 🏛️ CHI TIẾT BẰNG CHỨNG MÃ NGUỒN CÁC TẦNG KIẾN TRÚC

### 1. Tầng CoreBusiness (Domain Entity 3NF, độc lập 100%)
* [`Department.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/Department.cs): Phòng ban cơ cấu tổ chức.
* [`Employee.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/Employee.cs): Thông tin nhân sự, số người phụ thuộc.
* [`Contract.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/Contract.cs): Tách lương cơ bản, phụ cấp, lương BHXH khỏi bảng nhân viên.
* [`Timesheet.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/Timesheet.cs) & [`TimeAttendance.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/TimeAttendance.cs): Dữ liệu chấm công thực tế, giờ OT.
* [`PayrollPeriod.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/PayrollPeriod.cs): Kỳ tính lương và trạng thái chốt sổ.
* [`Payslip.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/Payslip.cs): Header phiếu lương (Gross, Deductions, Net).
* [`PayslipLine.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/PayslipLine.cs): Dòng chi tiết thu nhập (+) và khấu trừ (-).
* [`DeductionRate.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.CoreBusiness/Models/DeductionRate.cs): Tỷ lệ trích động kèm thuộc tính `RegulationSource`.

### 2. Tầng UseCases & Application Logic
* [`CalculatePayrollUseCase.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.UseCases/Payroll/CalculatePayrollUseCase.cs): Cài đặt **Luật 1**, tính lương trong Database Transaction.
* [`PayslipSecurityUseCases.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.UseCases/Payroll/PayslipSecurityUseCases.cs): Cài đặt **Luật 2**, khóa sổ kỳ lương và bảo mật dữ liệu cấp dòng.
* [`AttendanceDraftService.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.UseCases/Services/AttendanceDraftService.cs): Scoped State Management theo ShoppingCart pattern.
* [`UserSessionService.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.UseCases/Services/UserSessionService.cs): Phân quyền dữ liệu theo vai trò Kế toán / Nhân viên.
* [`ExplainPayslipWithAIUseCase.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.UseCases/AI/ExplainPayslipWithAIUseCase.cs): Mở rộng AI đối soát phiếu lương.

### 3. Tầng SQL Server & Entity Framework Core
* [`ChamCongDbContext.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.Plugins.DataStore.SQL/ChamCongDbContext.cs): Cấu hình Fluent API, precision decimal, index duy nhất.
* [`Repositories.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.Plugins.DataStore.SQL/Repositories.cs): Cài đặt `SqlUnitOfWork` và 100% Parameterized EF Core Queries.
* [`DatabaseSeeder.cs`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong.Plugins.DataStore.SQL/DatabaseSeeder.cs): Khởi tạo tự động dữ liệu mẫu chuẩn và nguồn quy định luật.

### 4. Tầng Giao Diện Blazor WebApp (Interactive Server)
* [`MainLayout.razor`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong/Components/Layout/MainLayout.razor): Header bar tích hợp bộ chuyển vai trò Kế toán vs Nhân viên.
* [`NavMenu.razor`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong/Components/Layout/NavMenu.razor): Menu phân nhóm đổi động theo vai trò.
* [`Employees.razor`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong/Components/Pages/Employees.razor): CRUD nhân sự & hợp đồng, chặn nhân viên xem trộm.
* [`Attendance.razor`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong/Components/Pages/Attendance.razor): Bảng công nháp, duyệt công.
* [`Payroll.razor`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong/Components/Pages/Payroll.razor): Tính lương, khóa sổ, xem Header-Line, **Xuất bảng lương CSV/Excel**.
* [`AiAssistant.razor`](file:///c:/Users/Admin/Documents/ChamCong_TinhLuong/ChamCong_TinhLuong/Components/Pages/AiAssistant.razor): Trợ lý AI giải thích nguyên nhân giảm lương.

---

## 🚀 HƯỚNG DẪN KHỞI CHẠY VÀ DEMO ĐỒ ÁN

1. **Khởi chạy ứng dụng:**
   ```powershell
   cd ChamCong_TinhLuong
   dotnet run
   ```
2. Mở trình duyệt tại: `http://localhost:5002` (hoặc port thông báo trên terminal).
3. **Kịch bản Demo trực tiếp:**
   * **Bước 1 (Đóng vai Kế toán / Admin):** Truy cập `/employees` xem chuẩn 3NF tách hợp đồng $\rightarrow$ Vào `/attendance` hiệu chỉnh công và bấm "Chốt Duyệt" $\rightarrow$ Vào `/payroll` bấm "Tính Lương Hàng Loạt" trong Transaction $\rightarrow$ Bấm "Xuất Bảng Lương (CSV)" để tải file bảng lương về máy $\rightarrow$ Bấm "Khóa Sổ Kỳ Này".
   * **Bước 2 (Đóng vai Nhân viên):** Trên thanh header góc trên bên phải, đổi vai trò sang **"Nhân viên: NV001 - Nguyễn Văn An"** $\rightarrow$ Hệ thống tự động ẩn menu Admin, chuyển sang xem bảng công và phiếu lương của riêng anh An (Row-Level Security) $\rightarrow$ Vào `/ai-assistant` hỏi Trợ lý AI *"Vì sao lương tháng này giảm?"* để trải nghiệm phân tích tự động.
