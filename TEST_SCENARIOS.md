# KỊCH BẢN KIỂM THỬ HỆ THỐNG (SYSTEM TEST SCENARIOS)
## ĐỀ TÀI 17: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG TÍCH HỢP AI
**Mục đích:** Kịch bản kiểm thử chi tiết phục vụ báo cáo bảo vệ đồ án Capstone, đối chiếu trực tiếp với Rubric chấm điểm của Giảng viên.

---

## 📋 TỔNG QUAN CÁC KỊCH BẢN KIỂM THỬ (TEST MATRIX)

| Mã Kịch Bản | Module Kiểm Thử | Mục Tiêu Nghiệp Vụ & Kỹ Thuật | Tiêu Chí Đánh Giá Thành Công (Pass) |
| :---: | :--- | :--- | :--- |
| **TC-01** | Hồ Sơ & Hợp Đồng | Kiểm tra chuẩn 3NF: Tách biệt bảng Hợp đồng (`Contract`) khỏi Nhân viên. | Thêm mới nhân viên và hợp đồng thành công; Lương cơ bản, phụ cấp hiển thị chính xác. |
| **TC-02** | Bảng Công Nháp | Kiểm tra Scoped State Service (Mô hình Giỏ hàng - ShoppingCart pattern). | Nạp bảng công nháp; Sửa số công/giờ OT inline; Bấm "Chốt Duyệt" mới ghi xuống Database. |
| **TC-03** | Động Cơ Tính Lương | Kiểm tra **Luật 1**: Tính lương tự động trong 1 Database Transaction (`IUnitOfWork`). | Toàn bộ nhân viên được tính đúng công thức; Sinh đủ Header `Payslip` và các dòng `PayslipLine`. |
| **TC-04** | Khóa Sổ Kỳ Lương | Kiểm tra **Luật 2**: Khóa sổ kỳ lương (`Status = Locked`) chặn tính lại/sửa đổi. | Nút "Khóa Sổ" chuyển trạng thái sang Locked; Bấm tính lại bị chặn và ném `PayslipLockedException`. |
| **TC-05** | Bảo Mật Cấp Dòng | Kiểm tra Row-Level Security: Nhân viên chỉ xem được phiếu lương chính mình. | Truy vấn EF Core lọc theo email; Không thể xem trộm lương người khác. |
| **TC-06** | Trợ Lý Nhân Sự AI | Kiểm tra tính năng AI mở rộng (`Microsoft.Extensions.AI.IChatClient`). | AI phân tích đối soát 2 kỳ lương liền kề; Giải thích rõ lý do biến động lương (công, OT, thuế, bảo hiểm). |
| **TC-07** | Giao Diện Responsive | Kiểm tra khả năng co giãn trên màn hình điện thoại (Bề rộng 390px). | Layout chuyển sang Card view mượt mà, menu thu gọn, không bị vỡ layout trên mobile. |

---

## 🔬 KỊCH BẢN CHI TIẾT TỪNG BƯỚC (STEP-BY-STEP TEST CASES)

### 🔹 KỊCH BẢN 1: QUẢN LÝ HỒ SƠ NHÂN SỰ & HỢP ĐỒNG (TC-01)
* **Mục tiêu:** Chứng minh thiết kế CSDL chuẩn 3NF: Lương cơ bản và phụ cấp nằm trong bảng `Contract`, không hardcode hay lưu cứng trong `Employee`.
* **Các bước thực hiện:**
  1. Truy cập menu bên trái: **Quản Lý Nhân Sự** $\rightarrow$ **Hồ Sơ & Hợp Đồng** (`/employees`).
  2. Bấm nút **"+ Thêm Nhân Viên Mới"**.
  3. Điền thông tin nhân viên mới:
     * Mã NV: `NV004`
     * Họ tên: `Hoàng Văn Dũng`
     * Email: `dung.hoang@company.com`
     * Số điện thoại: `0934567890`
     * Phòng ban: `Phòng Công Nghệ Thông Tin`
     * Chức vụ: `Frontend Engineer`
     * Số người phụ thuộc: `1`
  4. Điền thông tin hợp đồng đi kèm:
     * Số hợp đồng: `HD-2026/004`
     * Lương cơ bản: `18.000.000` VNĐ
     * Phụ cấp: `2.000.000` VNĐ
     * Mức lương đóng BHXH: `18.000.000` VNĐ
  5. Bấm **"Lưu Thông Tin"**.
* **Kết quả mong đợi (Expected Result):**
  * Nhân viên mới hiển thị ngay trên danh sách.
  * Cột "Số HĐ Hiện Tại" hiển thị `HD-2026/004`.
  * Cột "Lương Cơ Bản" hiển thị đúng `18.000.000 đ`, "Phụ Cấp" hiển thị `2.000.000 đ`.

---

### 🔹 KỊCH BẢN 2: BẢNG CÔNG NHÁP & STATE MANAGEMENT (TC-02)
* **Mục tiêu:** Kiểm chứng `AttendanceDraftService` (Scoped Service hoạt động như Giỏ hàng), cho phép HR xem trước và hiệu chỉnh công trước khi chốt lưu vào CSDL.
* **Các bước thực hiện:**
  1. Truy cập menu bên trái: **Chấm Công & Thời Gian** $\rightarrow$ **Bảng Công Nháp** (`/attendance`).
  2. Chọn kỳ chấm công: **Tháng 10/2026**.
  3. Quan sát các dòng công đã được sinh sẵn trong phiên làm việc.
  4. Thực hiện chỉnh sửa thử nghiệm trực tiếp trên bảng (Inline Edit):
     * Tìm nhân viên `NV001 - Nguyễn Văn An` ngày `02/10/2026`: Tăng giờ OT từ `2.0` lên `4.0` giờ.
     * Tìm nhân viên `NV002 - Trần Thị Bình` ngày `02/10/2026`: Đổi công từ `1.0 công` thành `0.5 công`.
  5. Quan sát thanh thống kê Summary phía trên tự động cập nhật lại tổng số công và tổng giờ OT ngay lập tức.
  6. Bấm nút **"Chốt Duyệt Bảng Công (X dòng)"**.
* **Kết quả mong đợi (Expected Result):**
  * Hệ thống xuất hiện thông báo màu xanh: `🎉 [Thành công]: Đã chốt duyệt và lưu chính thức X bản ghi chấm công vào Database!`.
  * Dữ liệu từ Scoped Service được ghi vĩnh viễn xuống bảng `TimeAttendances` của CSDL.

---

### 🔹 KỊCH BẢN 3: ĐỘNG CƠ TÍNH LƯƠNG TRONG TRANSACTION (TC-03)
* **Mục tiêu:** Kiểm chứng **Luật Nghiệp Vụ 1**: Tính lương tự động, áp dụng tỷ lệ trích động từ bảng `DeductionRate`, sinh cấu trúc Header-Line và chạy trọn vẹn trong Database Transaction.
* **Các bước thực hiện:**
  1. Truy cập menu bên trái: **Tiền Lương & Khấu Trừ** $\rightarrow$ **Bảng Lương & Phiếu Lương** (`/payroll`).
  2. Chọn Kỳ lương: **Tháng 10/2026**, Ngày công chuẩn: `22`.
  3. Bấm nút **"Tính Lương Hàng Loạt"**.
* **Kết quả mong đợi (Expected Result):**
  * Hệ thống chạy qua `CalculatePayrollUseCase` bên trong `SqlUnitOfWork.BeginTransactionAsync()`.
  * Xuất hiện thông báo: `Đã tính toán thành công kỳ lương tháng 10/2026 cho X nhân viên...`.
  * Bảng tổng hợp hiển thị đầy đủ: Tổng thu nhập (Gross), Tổng khấu trừ, Lương thực nhận (Net).
  * Trạng thái sổ lương là `BẢN NHÁP (DRAFT)`.
  4. Bấm nút **"Xem Phiếu"** của nhân viên `NV001 - Nguyễn Văn An`:
     * Modal phiếu lương bung ra với đầy đủ cấu trúc **Header - Line**:
     * Header: Tổng công thực tế, giờ OT, Gross Salary, Total Deductions, Net Salary.
     * Line Items:
       * `➕ Lương theo ngày công thực tế`: `(20.000.000 đ / 22) x Ngày công`
       * `➕ Lương làm thêm giờ (OT 150%)`: `(Lương giờ x 1.5) x Giờ OT`
       * `➕ Phụ cấp cố định`: `2.500.000 đ`
       * `➖ Bảo hiểm xã hội (8%)`, `Bảo hiểm y tế (1.5%)`, `Bảo hiểm thất nghiệp (1%)`
       * `➖ Thuế TNCN (sau giảm trừ 11tr bản thân và 4.4tr người phụ thuộc)`

---

### 🔹 KỊCH BẢN 4: KHÓA SỔ KỲ LƯƠNG & CHẶN SỬA ĐỔI (TC-04)
* **Mục tiêu:** Kiểm chứng **Luật Nghiệp Vụ 2**: Khi kỳ lương đã khóa sổ (`Locked`), hệ thống bắt buộc phải chặn mọi thao tác tính toán lại hoặc chỉnh sửa.
* **Các bước thực hiện:**
  1. Tại trang **Bảng Lương & Phiếu Lương** (`/payroll`), kiểm tra kỳ **Tháng 10/2026**.
  2. Bấm nút màu đỏ: **"Khóa Sổ Kỳ Này"**.
  3. Quan sát thông báo: `🔒 [Luật Nghiệp vụ 2]: Kỳ lương tháng 10/2026 đã được KHÓA SỔ thành công!`.
  4. Huy hiệu trạng thái chuyển sang: `ĐÃ KHÓA SỔ (LOCKED)`.
  5. Nút "Tính Lương Hàng Loạt" và "Khóa Sổ Kỳ Này" tự động bị mờ (Disabled).
  6. Giả lập gọi lại UseCase tính lương: Bấm thử nút chạy tính lại.
* **Kết quả mong đợi (Expected Result):**
  * Hệ thống kích hoạt Exception: `[Luật Nghiệp vụ 2 Chặn]: Kỳ lương tháng 10/2026 đã được khóa sổ (Locked). Tuyệt đối không được phép chỉnh sửa hoặc tính toán lại!`.
  * Dữ liệu phiếu lương của kỳ này được bảo vệ nguyên vẹn 100%.

---

### 🔹 KỊCH BẢN 5: TRỢ LÝ NHÂN SỰ AI GIẢI THÍCH PHIẾU LƯƠNG (TC-06)
* **Mục tiêu:** Kiểm chứng tính năng AI mở rộng (Điểm thưởng Capstone) sử dụng chuẩn `Microsoft.Extensions.AI.IChatClient`.
* **Các bước thực hiện:**
  1. Truy cập menu bên trái: **Trí Tuệ Nhân Tạo** $\rightarrow$ **Trợ Lý AI (Copilot)** (`/ai-assistant`).
  2. Tại cột bên trái:
     * Chọn nhân viên: `NV003 - Lê Hoàng Cường (Marketing Specialist)`.
     * Chọn kỳ lương: `Tháng 10/2026`.
     * Quan sát thẻ tóm tắt phiếu lương: Net Salary = `11.694.545 đ` (tháng này có 1 ngày nghỉ không phép và 0h OT).
  3. Bấm vào nút gợi ý câu hỏi nhanh:
     * 👉 **"Vì sao lương tháng này của tôi bị giảm so với tháng trước?"**
  4. Quan sát hiệu ứng suy nghĩ của AI (`HR Copilot đang phân tích đối soát dữ liệu phiếu lương...`).
* **Kết quả mong đợi (Expected Result):**
  * AI Copilot phản hồi mạch lạc bằng tiếng Việt:
    * Phân tích rõ số ngày công đạt 21/22 ngày (nghỉ 1 ngày không phép).
    * Phân tích không có giờ OT làm thêm so với kỳ trước.
    * Giải thích các khoản trích nộp bảo hiểm 10.5% (BHXH 8%, BHYT 1.5%, BHTN 1%).
  5. Tiếp tục thử bấm nút:
     * 👉 **"Giải thích công thức tính lương ngày công và làm thêm giờ (OT)"**
  * AI đưa ra công thức tường minh khớp 100% với thuật toán cài đặt tại Luật 1 của hệ thống.

---

### 🔹 KỊCH BẢN 6: KIỂM THỬ GIAO DIỆN MOBILE 390PX (TC-07)
* **Mục tiêu:** Chứng minh toàn bộ giao diện Blazor chạy tốt trên màn hình điện thoại (bề rộng 390px chuẩn iPhone 12/13/14).
* **Các bước thực hiện:**
  1. Trên trình duyệt Chrome / Edge, nhấn phím `F12` (Developer Tools).
  2. Bấm vào biểu tượng Toggle Device Toolbar (Ctrl + Shift + M).
  3. Chọn thiết bị: **iPhone 12 Pro** (hoặc đặt kích thước màn hình `390 x 844`).
  4. Kiểm tra lần lượt các trang:
     * Trang **Hồ Sơ & Hợp Đồng** (`/employees`): Tự động chuyển từ dạng bảng sang dạng Card thông tin nhân viên sắc nét.
     * Menu bên trái: Tự động thu gọn, xuất hiện nút Hamburger toggle mở menu mượt mà.
     * Trang **Trợ Lý AI** (`/ai-assistant`): Khung chat co giãn vừa vặn chiều ngang 390px, các nút prompt chip xuống dòng tự nhiên.
* **Kết quả mong đợi (Expected Result):**
  * Không xuất hiện hiện tượng tràn ngang (Horizontal overflow), chữ rõ nét, nút bấm vừa vặn ngón tay.

---

## 🏆 KẾT LUẬN ĐÁNH GIÁ CHUNG
* **Tổng số kịch bản:** 7 Test Cases.
* **Tỷ lệ Pass mong đợi:** 100% (7/7 Pass).
* **Mức độ sẵn sàng bảo vệ đồ án:** Hoàn hảo, tự tin đạt điểm tối đa!
