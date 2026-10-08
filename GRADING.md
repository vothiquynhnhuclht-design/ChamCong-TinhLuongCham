# BÁO CÁO TIẾN ĐỘ & BẰNG CHỨNG CHẤM ĐIỂM (GRADING EVIDENCE)
## ĐỀ TÀI 17: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG TÍCH HỢP AI

---

## 🏆 TỔNG HỢP TIÊU CHÍ RUBRIC & ĐỐI CHIẾU MÃ NGUỒN (TUẦN 05 & TUẦN 06)

| Tiêu Chí Rubric | Trạng Thái | Vị Trí File & Bằng Chứng Mã Nguồn | Diễn Giải Chi Tiết |
| :--- | :---: | :--- | :--- |
| **1. Clean Architecture 4 Tầng** | ✅ Hoàn thành | • `ChamCong_TinhLuong.CoreBusiness`<br>• `ChamCong_TinhLuong.UseCases`<br>• `ChamCong_TinhLuong.Plugins.DataStore.InMemory`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL`<br>• `ChamCong_TinhLuong` (WebApp) | Tách thành các project độc lập, tuân thủ nguyên tắc Dependency Inversion. CoreBusiness độc lập 0 tham chiếu; UseCases chỉ tham chiếu CoreBusiness; WebApp & Plugins thực thi qua Interfaces. |
| **2. Tách Hợp Đồng (`Contract`)** | ✅ Hoàn thành | `ChamCong_TinhLuong.CoreBusiness/Models/Contract.cs` | Không lưu cứng lương cơ bản trong bảng Nhân viên. Lương cơ bản, phụ cấp, mức đóng BHXH nằm riêng trong Contract, liên kết 1-Nhiều. |
| **3. Cấu hình Trích nộp Động (`DeductionRate`)** | ✅ Hoàn thành | `ChamCong_TinhLuong.CoreBusiness/Models/DeductionRate.cs` | Tỷ lệ trích BHXH (8%), BHYT (1.5%), BHTN (1%), mức giảm trừ gia cảnh (11tr/4.4tr) được lưu trong bảng CSDL, không hardcode trong C#. |
| **4. Mô hình Header - Line Bảng Lương** | ✅ Hoàn thành | • `ChamCong_TinhLuong.CoreBusiness/Models/Payslip.cs`<br>• `ChamCong_TinhLuong.CoreBusiness/Models/PayslipLine.cs` | Cặp bảng Header (`Payslip`) và Line Items (`PayslipLine`) phân tách rành mạch các dòng thu nhập (+) và khấu trừ (-) kèm công thức và ghi chú. |
| **5. Luật 1: Tính Lương trong Transaction** | ✅ Hoàn thành | • `ChamCong_TinhLuong.UseCases/Payroll/CalculatePayrollUseCase.cs`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/Repositories.cs` (`SqlUnitOfWork`) | Thuật toán tính lương tự động, quét công, áp tỷ lệ DeductionRate và sinh Payslip/PayslipLine trọn vẹn trong 1 Database Transaction (`IDbContextTransaction` / `IUnitOfWork`). |
| **6. Luật 2: Khóa Sổ & Row-Level Security** | ✅ Hoàn thành | • `ChamCong_TinhLuong.UseCases/Payroll/PayslipSecurityUseCases.cs`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/Repositories.cs` (`SqlPayslipRepository.GetPayslipsByEmployeeEmailAsync`) | Kỳ lương đã khóa sổ (`Status = Locked`) ném ngoại lệ `PayslipLockedException` chặn tính lại; Phân quyền dữ liệu cấp dòng kiểm tra người dùng chỉ xem được phiếu lương chính mình. |
| **7. Quản lý Bảng Công Nháp (State Management)** | ✅ Hoàn thành | `ChamCong_TinhLuong.UseCases/Services/AttendanceDraftService.cs` | Service dạng **`Scoped`** theo mô hình Giỏ hàng (ShoppingCart Pattern), cho phép xem trước và hiệu chỉnh công trước khi chốt lưu vào DB. |
| **8. Tầng SQL Server & EF Core Migrations** | ✅ Hoàn thành | • `ChamCong_TinhLuong.Plugins.DataStore.SQL/ChamCongDbContext.cs`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/Migrations/`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/DatabaseSeeder.cs` | Khởi tạo DbContext với Fluent API, Migrations chuẩn, 100% Parameterized LINQ Queries (chống SQL Injection), tự động Seed dữ liệu khi khởi chạy. |
| **9. Giao Diện Blazor Interactive Server (390px Mobile)** | ✅ Hoàn thành | • `Components/Layout/NavMenu.razor`<br>• `Components/Pages/Home.razor`<br>• `Components/Pages/Employees.razor`<br>• `Components/Pages/Attendance.razor`<br>• `Components/Pages/Payroll.razor` | Giao diện Blazor Interactive Server, Bootstrap 5 co giãn Responsive hoàn hảo trên thiết bị di động (bề rộng 390px), Sidebar phân nhóm trực quan. |

---

## 📅 LỘ TRÌNH 3 TUẦN (SPRINT MILESTONES)

* **Tuần 05 (Sprint 1):** ✅ **HOÀN THÀNH XUẤT SẮC**
  * Khởi tạo kiến trúc Clean Architecture 4 tầng (.NET 8).
  * Xây dựng đầy đủ Domain Model 3NF (Contract, DeductionRate, Header-Line Payslip).
  * Xây dựng UseCases cốt lõi, State Management Bảng công nháp Scoped Service.
  * Tích hợp In-Memory DataStore + Seed Data và 4 trang giao diện Blazor.
* **Tuần 06 (Sprint 2):** ✅ **HOÀN THÀNH XUẤT SẮC**
  * Xây dựng project hạ tầng `ChamCong_TinhLuong.Plugins.DataStore.SQL`.
  * Thiết lập EF Core `ChamCongDbContext` với Fluent API & Decimal Precision.
  * Tạo Migration `InitialCreate` cho SQL Server.
  * Cài đặt 100% Parameterized Queries trong các Repository (chống SQL Injection).
  * Cài đặt `SqlUnitOfWork` bọc Database Transaction (`IDbContextTransaction`) cho Luật 1.
  * Tích hợp `DatabaseSeeder` tự động khởi tạo dữ liệu mẫu khi chạy ứng dụng.
  * Tối ưu thanh Menu Bar (Sidebar) phân nhóm rõ ràng, chuẩn phong cách SaaS Dashboard.
* **Tuần 07 (Sprint 3):** 🔜 *Tiếp theo - Điểm thưởng AI*
  * Tích hợp `IChatClient` (Gemini / Ollama / OpenAI) làm Trợ lý ảo AI phân tích phiếu lương ("Vì sao lương tháng này giảm?").
  * Hoàn thiện bằng chứng chấm điểm chi tiết số dòng code vào file `GRADING.md`.
