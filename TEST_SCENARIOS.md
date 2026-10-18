# KỊCH BẢN KIỂM THỬ HỆ THỐNG (SYSTEM TEST SCENARIOS)
## ĐỀ TÀI 17: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG TÍCH HỢP AI (TIMESHEET & PAYROLL)
**Mục đích:** Kịch bản kiểm thử chi tiết phục vụ báo cáo bảo vệ đồ án Capstone, đối chiếu trực tiếp với Rubric chấm điểm của Giảng viên.

---

## 📋 TỔNG QUAN CÁC KỊCH BẢN KIỂM THỬ (TEST MATRIX)

| Mã Kịch Bản | Module Kiểm Thử | Mục Tiêu Nghiệp Vụ & Kỹ Thuật | Tiêu Chí Đánh Giá Thành Công (Pass) |
| :---: | :--- | :--- | :--- |
| **TC-00** | Màn Hình Đăng Nhập | Kiểm tra xác thực tài khoản và phân quyền tự động theo vai trò (Kế toán vs Nhân viên). | Đăng nhập đúng username/mật khẩu; Phân quyền chính xác vào giao diện tương ứng; Hỗ trợ 1-click Demo. |
| **TC-01** | Hồ Sơ & Hợp Đồng | Kiểm tra chuẩn 3NF: Tách biệt bảng Hợp đồng (`Contract`) khỏi Nhân viên. | Thêm mới nhân viên và hợp đồng thành công; Lương cơ bản, phụ cấp hiển thị chính xác. |
| **TC-02** | Bảng Công Nháp | Kiểm tra Scoped State Service (Mô hình Giỏ hàng - ShoppingCart pattern). | Nạp bảng công nháp; Sửa số công/giờ OT inline; Bấm "Chốt Duyệt" mới ghi xuống Database. |
| **TC-03** | Động Cơ Tính Lương | Kiểm tra **Luật 1**: Tính lương tự động trong 1 Database Transaction (`IUnitOfWork`). | Toàn bộ nhân viên được tính đúng công thức; Sinh đủ Header `Payslip` và các dòng `PayslipLine`. |
| **TC-04** | Khóa Sổ Kỳ Lương | Kiểm tra **Luật 2**: Khóa sổ kỳ lương (`Status = Locked`) chặn tính lại/sửa đổi. | Nút "Khóa Sổ" chuyển trạng thái sang Locked; Bấm tính lại bị chặn và ném `PayslipLockedException`. |
| **TC-05** | Bảo Mật Cấp Dòng | Kiểm tra Row-Level Security: Nhân viên chỉ xem được bảng công và phiếu lương chính mình. | Đăng nhập bằng tài khoản nhân viên (NV001); Hệ thống chặn xem trộm dữ liệu người khác; Khóa menu quản trị. |
| **TC-06** | Xuất Bảng Lương | Kiểm tra tính năng Xuất báo cáo bảng lương ra file CSV/Excel cho Kế toán. | Bấm "Xuất Bảng Lương (CSV)" tải về file CSV chuẩn UTF-8 BOM, mở bằng Excel tiếng Việt có dấu. |
| **TC-07** | Trợ Lý Nhân Sự AI | Kiểm tra tính năng AI mở rộng (`Microsoft.Extensions.AI.IChatClient`). | AI phân tích đối soát 2 kỳ lương liền kề; Giải thích rõ lý do biến động lương (công, OT, thuế, bảo hiểm). |
| **TC-08** | Giao Diện Responsive | Kiểm tra khả năng co giãn trên màn hình điện thoại (Bề rộng 390px). | Layout chuyển sang Card view mượt mà, menu thu gọn, không bị vỡ layout trên mobile. |

---

## 🔬 KỊCH BẢN CHI TIẾT TỪNG BƯỚC (STEP-BY-STEP TEST CASES)

### 🔹 KỊCH BẢN 0: MÀN HÌNH ĐĂNG NHẬP & PHÂN QUYỀN TÀI KHOẢN (TC-00)
* **Mục tiêu:** Chứng minh hệ thống xác thực từ màn hình đăng nhập độc quyền, phân quyền chính xác theo tài khoản đăng nhập (không dùng dropdown chọn tài khoản thủ công).
* **Các bước thực hiện:**
  1. Mở trình duyệt tại `http://localhost:5002`.
  2. Quan sát: Hệ thống hiển thị **Màn hình Đăng Nhập (Login Screen)** với giao diện hiện đại, che giấu toàn bộ dữ liệu nội bộ khi chưa đăng nhập.
  3. Thử nghiệm đăng nhập sai mật khẩu: Nhập `admin` / `111111` $\rightarrow$ Bấm "ĐĂNG NHẬP" $\rightarrow$ Xuất hiện thông báo lỗi: `Mật khẩu không chính xác`.
  4. Bấm vào nút Demo 1-Click: **"👑 Kế toán / Quản trị HR (admin / 123456)"**.
  5. Hệ thống xác thực thành công và chuyển ngay vào giao diện làm việc chính thức.
  6. Quan sát thanh Header góc trên bên phải: Hiển thị avatar `HR`, tên `Võ Thị Quỳnh Như (Kế toán Trưởng)`, huy hiệu `Kế toán / HR Admin` và nút `Đăng Xuất`.
  7. Bấm **"Đăng Xuất"** $\rightarrow$ Hệ thống xóa phiên làm việc và quay trở về màn hình đăng nhập an toàn.

---

### 🔹 KỊCH BẢN 1: QUẢN LÝ HỒ SƠ NHÂN SỰ & HỢP ĐỒNG (TC-01)
* **Mục tiêu:** Chứng minh thiết kế CSDL chuẩn 3NF: Lương cơ bản và phụ cấp nằm trong bảng `Contract`, không hardcode hay lưu cứng trong `Employee`.
* **Các bước thực hiện:**
  1. Đăng nhập với tài khoản Kế toán (`admin` / `123456`).
  2. Truy cập menu bên trái: **Quản Lý Nhân Sự** $\rightarrow$ **Hồ Sơ & Hợp Đồng** (`/employees`).
  3. Bấm nút **"+ Thêm Nhân Viên Mới"**.
  4. Điền thông tin nhân viên mới:
     * Mã NV: `NV004`
     * Họ tên: `Hoàng Văn Dũng`
     * Email: `dung.hoang@company.com`
     * Số điện thoại: `0934567890`
     * Phòng ban: `Phòng Công Nghệ Thông Tin`
     * Chức vụ: `Frontend Engineer`
     * Số người phụ thuộc: `1`
  5. Điền thông tin hợp đồng đi kèm:
     * Số hợp đồng: `HD-2026/004`
     * Lương cơ bản: `18.000.000` VNĐ
     * Phụ cấp: `2.000.000` VNĐ
     * Mức lương đóng BHXH: `18.000.000` VNĐ
  6. Bấm **"Lưu Thông Tin"**.
* **Kết quả mong đợi (Expected Result):**
  * Nhân viên mới hiển thị ngay trên danh sách.
  * Cột "Số HĐ Hiện Tại" hiển thị `HD-2026/004`.
  * Cột "Lương Cơ Bản" hiển thị đúng `18.000.000 đ`, "Phụ Cấp" hiển thị `2.000.000 đ`.

---

### 🔹 KỊCH BẢN 2: BẢNG CÔNG NHÁP & STATE MANAGEMENT (TC-02)
* **Mục tiêu:** Kiểm chứng `AttendanceDraftService` (Scoped Service hoạt động như Giỏ hàng - ShoppingCart pattern), cho phép HR xem trước và hiệu chỉnh công trước khi chốt lưu vào CSDL.
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
  * Hệ thống xuất hiện thông báo: `🎉 [Thành công]: Đã chốt duyệt và lưu chính thức X bản ghi chấm công vào Database!`.
  * Dữ liệu từ Scoped Service được ghi vĩnh viễn xuống bảng `TimeAttendances` của SQL Server.

---

### 🔹 KỊCH BẢN 3: ĐỘNG CƠ TÍNH LƯƠNG TRONG TRANSACTION (TC-03)
* **Mục tiêu:** Kiểm chứng **Luật Nghiệp Vụ 1**: Tính lương tự động, áp dụng tỷ lệ trích động từ bảng `DeductionRate` kèm nguồn quy định luật, sinh cấu trúc Header-Line và chạy trọn vẹn trong Database Transaction.
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
       * `➖ Bảo hiểm xã hội (8%)` *(Theo Luật BHXH số 58/2014/QH13, Điều 85)*
       * `➖ Bảo hiểm y tế (1.5%)` *(Theo Luật BHYT số 46/2014/QH13, Điều 13)*
       * `➖ Bảo hiểm thất nghiệp (1%)` *(Theo Luật Việc làm số 38/2013/QH13, Điều 57)*
       * `➖ Thuế TNCN` *(Theo Nghị quyết 954/2020/UBTVQH14 giảm trừ 11tr bản thân, 4.4tr người PT)*.

---

### 🔹 KỊCH BẢN 4: KHÓA SỔ KỲ LƯƠNG & CHẶN SỬA ĐỔI (TC-04)
* **Mục tiêu:** Kiểm chứng **Luật Nghiệp Vụ 2**: Khi kỳ lương đã khóa sổ (`Locked`), hệ thống bắt buộc phải chặn mọi thao tác tính toán lại hoặc chỉnh sửa.
* **Các bước thực hiện:**
  1. Tại trang **Bảng Lương & Phiếu Lương** (`/payroll`), kiểm tra kỳ **Tháng 10/2026**.
  2. Bấm nút màu đỏ: **"Khóa Sổ Kỳ Này"**.
  3. Quan sát thông báo: `🔒 [Luật Nghiệp vụ 2]: Kỳ lương tháng 10/2026 đã được KHÓA SỔ thành công!`.
  4. Huy hiệu trạng thái chuyển sang: `ĐÃ KHÓA SỔ (LOCKED)`.
  5. Nút "Tính Lương Hàng Loạt" và "Khóa Sổ Kỳ Này" tự động bị vô hiệu hóa (Disabled).
  6. Nếu gọi lại UseCase tính lương, hệ thống kích hoạt Exception: `[Luật Nghiệp vụ 2 Chặn]: Kỳ lương tháng 10/2026 đã được khóa sổ (Locked). Tuyệt đối không được phép chỉnh sửa hoặc tính toán lại!`.

---

### 🔹 KỊCH BẢN 5: BẢO MẬT CẤP DÒNG (ROW-LEVEL SECURITY) (TC-05)
* **Mục tiêu:** Kiểm chứng phân quyền theo tài khoản nhân viên đăng nhập: Nhân viên chỉ xem được bảng công và phiếu lương của chính mình, chặn xem trộm dữ liệu người khác.
* **Các bước thực hiện:**
  1. Bấm nút **"Đăng Xuất"** ở góc trên bên phải.
  2. Tại màn hình đăng nhập, bấm vào nút: **"👤 Nhân viên: Nguyễn Văn An (NV001 / 123456)"**.
  3. Quan sát sau khi đăng nhập:
     * Header hiển thị: `Nguyễn Văn An`, huy hiệu `Nhân viên (NV001)`.
     * Menu bên trái tự động thu gọn: Chỉ còn *Tổng Quan*, *Bảng Công Của Tôi*, *Phiếu Lương Của Tôi*, *Trợ Lý AI*.
     * Mục *Quản Lý Nhân Sự* (`/employees`) bị ẩn hoàn toàn khỏi menu.
  4. Thử cố ý gõ URL `http://localhost:5002/employees` vào thanh địa chỉ:
     * Hệ thống chặn lại và hiển thị cảnh báo màu đỏ: `Quyền Truy Cập Bị Hạn Chế. Trang này chỉ dành cho Kế toán / Quản trị Nhân sự`.
  5. Vào **Phiếu Lương Của Tôi** (`/payroll`):
     * Danh sách bảng lương chỉ hiển thị DUY NHẤT 1 bản ghi của anh Nguyễn Văn An.
     * Toàn bộ các nút quản trị "Tính Lương Hàng Loạt", "Khóa Sổ Kỳ Này" đều bị ẩn hoàn toàn.

---

### 🔹 KỊCH BẢN 6: XUẤT BẢNG LƯƠNG RA FILE CSV/EXCEL (TC-06)
* **Mục tiêu:** Kiểm chứng use case quản trị: "Xuất bảng lương" ra file mở bằng Microsoft Excel.
* **Các bước thực hiện:**
  1. Đăng nhập lại bằng tài khoản Kế toán (`admin` / `123456`).
  2. Vào trang **Bảng Lương & Phiếu Lương** (`/payroll`).
  3. Bấm nút màu xanh lá: **"Xuất Bảng Lương (CSV)"**.
  4. Trình duyệt tự động tải xuống file `BangLuong_Thang_10_2026.csv`.
  5. Mở file bằng Excel hoặc Notepad: Dữ liệu hiển thị chuẩn UTF-8 có dấu tiếng Việt, đầy đủ các cột Mã NV, Họ Tên, Phòng Ban, Công chuẩn, Công thực tế, Gross, Deductions, Net.

---

### 🔹 KỊCH BẢN 7: TRỢ LÝ NHÂN SỰ AI GIẢI THÍCH PHIẾU LƯƠNG (TC-07)
* **Mục tiêu:** Kiểm chứng tính năng AI mở rộng (Điểm thưởng Capstone) sử dụng chuẩn `Microsoft.Extensions.AI.IChatClient`.
* **Các bước thực hiện:**
  1. Đăng xuất và đăng nhập bằng tài khoản nhân viên `NV003 - Lê Hoàng Cường` (`NV003` / `123456`).
  2. Truy cập menu bên trái: **Trí Tuệ Nhân Tạo** $\rightarrow$ **Trợ Lý AI (Copilot)** (`/ai-assistant`).
  3. Quan sát: Cột bên trái tự động khóa chọn đúng nhân viên `NV003 - Lê Hoàng Cường`.
  4. Bấm vào nút gợi ý câu hỏi nhanh:
     * 👉 **"Vì sao lương tháng này của tôi bị giảm so với tháng trước?"**
  5. Quan sát AI Copilot phản hồi bằng tiếng Việt:
     * Phân tích rõ số ngày công đạt 21/22 ngày (nghỉ 1 ngày không phép).
     * Phân tích không có giờ OT làm thêm so với kỳ trước.
     * Giải thích các khoản trích nộp bảo hiểm 10.5% và thuế TNCN.

---

### 🔹 KỊCH BẢN 8: KIỂM THỬ GIAO DIỆN MOBILE 390PX (TC-08)
* **Mục tiêu:** Chứng minh toàn bộ giao diện Blazor chạy tốt trên màn hình điện thoại (bề rộng 390px chuẩn iPhone 12/13/14).
* **Các bước thực hiện:**
  1. Nhấn phím `F12` $\rightarrow$ Bật Device Toolbar $\rightarrow$ Chọn **iPhone 12 Pro (390 x 844)**.
  2. Kiểm tra Màn hình Đăng nhập: Căn giữa hoàn hảo, form vừa vặn chiều ngang 390px.
  3. Đăng nhập và kiểm tra các trang: Bảng tự động chuyển sang Card view sắc nét, menu thu gọn, không có thanh cuộn ngang bị vỡ layout.

---

## 🏆 KẾT LUẬN ĐÁNH GIÁ CHUNG
* **Tổng số kịch bản:** 9 Test Cases (TC-00 đến TC-08).
* **Tỷ lệ Pass mong đợi:** 100% (9/9 Pass).
* **Mức độ hoàn thiện:** Vượt mong đợi của đề bài Capstone Đề tài 17!
