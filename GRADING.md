# BÁO CÁO TIẾN ĐỘ & BẰNG CHỨNG CHẤM ĐIỂM (GRADING EVIDENCE)
## ĐỀ TÀI 17: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG TÍCH HỢP AI (HR & PAYROLL SYSTEM)

---

## 🏆 TỔNG HỢP TIÊU CHÍ RUBRIC & ĐỐI CHIẾU MÃ NGUỒN CHI TIẾT

| Tiêu Chí Rubric | Trạng Thái | Vị Trí File & Bằng Chứng Mã Nguồn | Diễn Giải Chi Tiết |
| :--- | :---: | :--- | :--- |
| **1. Clean Architecture 4 Tầng** | ✅ Hoàn thành | • `ChamCong_TinhLuong.CoreBusiness`<br>• `ChamCong_TinhLuong.UseCases`<br>• `ChamCong_TinhLuong.Plugins.DataStore.InMemory`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL`<br>• `ChamCong_TinhLuong` (WebApp) | Tách thành các project độc lập theo chuẩn Dependency Inversion. CoreBusiness độc lập 0 tham chiếu; UseCases chỉ tham chiếu CoreBusiness; WebApp & Plugins liên kết thông qua Interfaces. |
| **2. Tách Hợp Đồng (`Contract`)** | ✅ Hoàn thành | `ChamCong_TinhLuong.CoreBusiness/Models/Contract.cs` | Không lưu cứng lương cơ bản trong bảng Nhân viên. Lương cơ bản, phụ cấp cố định, mức lương đóng BHXH nằm riêng trong bảng Hợp Đồng (quan hệ 1-Nhiều). |
| **3. Cấu hình Trích nộp Động (`DeductionRate`)** | ✅ Hoàn thành | `ChamCong_TinhLuong.CoreBusiness/Models/DeductionRate.cs` | Tỷ lệ trích BHXH (8%), BHYT (1.5%), BHTN (1%), mức giảm trừ gia cảnh (11tr/4.4tr) được lưu trong bảng CSDL, không hardcode trong mã C#. |
| **4. Mô hình Header - Line Bảng Lương** | ✅ Hoàn thành | • `ChamCong_TinhLuong.CoreBusiness/Models/Payslip.cs`<br>• `ChamCong_TinhLuong.CoreBusiness/Models/PayslipLine.cs` | Cặp bảng Header (`Payslip`) và Line Items (`PayslipLine`) phân tách rành mạch các dòng thu nhập (+) và khấu trừ (-) kèm công thức và ghi chú chi tiết. |
| **5. Luật 1: Tính Lương trong Transaction** | ✅ Hoàn thành | • `ChamCong_TinhLuong.UseCases/Payroll/CalculatePayrollUseCase.cs`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/Repositories.cs` (`SqlUnitOfWork`) | Thuật toán tính lương tự động, quét công, áp tỷ lệ DeductionRate và sinh Payslip/PayslipLine trọn vẹn trong 1 Database Transaction (`IDbContextTransaction` / `IUnitOfWork`), tự động Rollback nếu lỗi. |
| **6. Luật 2: Khóa Sổ & Row-Level Security** | ✅ Hoàn thành | • `ChamCong_TinhLuong.UseCases/Payroll/PayslipSecurityUseCases.cs`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/Repositories.cs` (`SqlPayslipRepository.GetPayslipsByEmployeeEmailAsync`) | Kỳ lương đã khóa sổ (`Status = Locked`) ném ngoại lệ `PayslipLockedException` chặn tính lại; Phân quyền dữ liệu cấp dòng kiểm tra người dùng chỉ xem được phiếu lương chính mình. |
| **7. Quản lý Bảng Công Nháp (State Management)** | ✅ Hoàn thành | `ChamCong_TinhLuong.UseCases/Services/AttendanceDraftService.cs` | Service dạng **`Scoped`** theo mô hình Giỏ hàng (ShoppingCart Pattern), cho phép xem trước và hiệu chỉnh công trước khi chốt lưu vào DB. |
| **8. Tầng SQL Server & EF Core Migrations** | ✅ Hoàn thành | • `ChamCong_TinhLuong.Plugins.DataStore.SQL/ChamCongDbContext.cs`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/Migrations/`<br>• `ChamCong_TinhLuong.Plugins.DataStore.SQL/DatabaseSeeder.cs` | Khởi tạo DbContext với Fluent API, Migrations chuẩn, 100% Parameterized LINQ Queries (chống SQL Injection), tự động Seed dữ liệu khi khởi chạy. |
| **9. Giao Diện Blazor Interactive Server (390px Mobile)** | ✅ Hoàn thành | • `Components/Layout/NavMenu.razor`<br>• `Components/Pages/Home.razor`<br>• `Components/Pages/Employees.razor`<br>• `Components/Pages/Attendance.razor`<br>• `Components/Pages/Payroll.razor` | Giao diện Blazor Interactive Server, Bootstrap 5 co giãn Responsive hoàn hảo trên thiết bị di động (bề rộng 390px), Sidebar phân nhóm trực quan. |
| **10. Điểm Thưởng AI: Trợ Lý Nhân Sự AI (`IChatClient`)** | ✅ Hoàn thành | • `ChamCong_TinhLuong.UseCases/AI/DefaultHrChatClient.cs`<br>• `ChamCong_TinhLuong.UseCases/AI/ExplainPayslipWithAIUseCase.cs`<br>• `Components/Pages/AiAssistant.razor` | Ứng dụng chuẩn `Microsoft.Extensions.AI.IChatClient` đóng vai trò Trợ lý ảo AI (HR Copilot) tự động đối soát 2 kỳ lương, giải thích lý do biến động thu nhập: *"Vì sao lương tháng này giảm?"*, giải thích bảo hiểm và thuế TNCN. |

---

## 📅 LỘ TRÌNH TRIỂN KHAI 3 TUẦN (SPRINTS COMPLETED 100%)

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
* **Tuần 07 (Sprint 3):** ✅ **HOÀN THÀNH XUẤT SẮC (ĐIỂM THƯỞNG CAPSTONE)**
  * Cài đặt thư viện chuẩn `Microsoft.Extensions.AI.Abstractions`.
  * Triển khai `DefaultHrChatClient` thực thi interface `IChatClient`.
  * Xây dựng UseCase `ExplainPayslipWithAIUseCase` thực hiện đối soát biến động giữa 2 kỳ lương liền kề.
  * Tạo trang giao diện Chatbot thông minh `AiAssistant.razor` (`/ai-assistant`) với Prompt chips gợi ý nhanh.
  * Tích hợp nút tắt "Hỏi Trợ Lý AI về Phiếu Này" ngay trong Modal xem Phiếu lương.
  * Cập nhật đầy đủ bằng chứng kiểm thử và tài liệu chấm điểm vào `GRADING.md`.

---

## 🚀 HƯỚNG DẪN CHẠY THỬ NGHIỆM ĐỒ ÁN (FOR EXAMINERS)

1. Mở Terminal / PowerShell tại thư mục gốc:
   ```powershell
   cd ChamCong_TinhLuong
   dotnet run
   ```
2. Mở trình duyệt và truy cập: `http://localhost:5000` (hoặc `https://localhost:5001`).
3. Khám phá 5 module chức năng:
   * **Tổng Quan (`/`):** Dashboard số liệu nhân sự, kỳ lương và đối chiếu tiêu chí Rubric.
   * **Hồ Sơ & Hợp Đồng (`/employees`):** Quản lý nhân viên, hợp đồng lao động, mức lương cơ bản, phụ cấp, giảm trừ người phụ thuộc.
   * **Bảng Công Nháp (`/attendance`):** Trải nghiệm Scoped State Service (mô hình Giỏ hàng), sửa công trực tiếp, chốt duyệt lưu DB.
   * **Tính Lương & Phiếu Lương (`/payroll`):** Bấm "Tính Lương Hàng Loạt" (chạy Transaction Luật 1), xem phiếu lương Header-Line chi tiết, bấm "Khóa Sổ" (Luật 2).
   * **Trợ Lý AI (`/ai-assistant`):** Tương tác với HR Copilot giải thích lý do biến động lương, bảo hiểm và thuế TNCN.
