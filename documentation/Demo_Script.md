# Kịch bản Demo (Demo Script)

⚠️ **Quan trọng**: Hãy thực tập theo kịch bản này ít nhất 2 lần trước khi lên bảng. Đảm bảo demo mượt mà, không khựng.

---

## Phần 1: Giới thiệu & Đăng nhập (1 phút)
1.  **Mở app**: Show Logo, giao diện Login (B1).
    *   *Nói*: "Chào thầy/cô, đây là ứng dụng MyShop quản lý bán hàng. Em xin phép bắt đầu từ màn hình Đăng nhập. Hệ thống có cơ chế bảo mật mật khẩu bằng SHA256."
2.  **Cấu hình Server**: Bấm vào nút "Server Config" ở góc (B1).
    *   *Nói*: "Người dùng có thể cấu hình chuỗi kết nối Database tại đây."
3.  **Đăng nhập**: Nhập user/pass (admin/admin).
    *   *Nói*: "Hệ thống hỗ trợ ghi nhớ đăng nhập cho lần sau."

## Phần 2: Dashboard & Quản lý Sản phẩm (2 phút)
4.  **Dashboard (B2)**:
    *   *Nói*: "Đây là Dashboard tổng quan. Thầy cô có thể thấy các chỉ số KPIs như Doanh thu hôm nay, Top sản phẩm bán chạy, và cảnh báo sắp hết hàng (Low stock)."
5.  **Vào màn hình Products (B3)**:
    *   *Thao tác*: Lọc theo Category -> Tìm kiếm "Laptop" -> Xóa bộ lọc.
    *   *Import Excel*: Bấm nút **Import**. Chọn file Excel mẫu.
    *   *Nói*: "Đây là tính năng Import Excel. Em sử dụng thư viện MiniExcel để đọc dữ liệu và thêm hàng loạt vào kho."
    *   Show kết quả: Sản phẩm mới hiện ra.

## Phần 3: Tạo Đơn hàng & Khuyến mãi (3 phút) - **QUAN TRỌNG**
6.  **Vào màn hình Orders (B4)**:
    *   *Thao tác*: Bấm nút **Add Order**.
7.  **Chọn Khách hàng (C5)**:
    *   *Thao tác*: Gõ "Nguyen" vào ô tìm kiếm -> Chọn khách hàng gợi ý.
    *   *Nói*: "Hệ thống hỗ trợ tìm kiếm khách hàng nhanh hoặc tự động tạo mới nếu chưa có."
8.  **Thêm sản phẩm**:
    *   *Thao tác*: Chọn 2-3 sản phẩm, chỉnh số lượng.
    *   *AutoSave*: Tắt dialog đi, mở lại.
    *   *Nói*: "Như thầy cô thấy, dữ liệu em vừa nhập vẫn còn nguyên nhờ tính năng AutoSave (C9)."
9.  **Áp dụng Khuyến mãi (C2)**:
    *   *Thao tác*: Nhập code `SALE50` (hoặc code mẫu bạn tạo) -> Bấm Apply.
    *   *Nói*: "Code giảm giá đã được áp dụng và trừ tiền trực tiếp."
10. **Lưu đơn hàng**: Bấm Create.

## Phần 4: Báo cáo & Cấu hình (1 phút)
11. **Vào màn hình Reports (B5)**:
    *   Cho xem biểu đồ Cột (Doanh thu) và Đường (Sản phẩm).
    *   *Nói*: "Biểu đồ này em tự vẽ bằng thư viện đồ họa cơ bản của WinUI, không dùng thư viện ngoài."
12. **Vào Cấu hình (B6/C8)**:
    *   Show chức năng **Backup Database**.
    *   Show chỉnh sửa **Items per page**.

## Phần 5: Kết thúc
*   *Nói*: "Dạ đó là toàn bộ các tính năng chính của ứng dụng, bao gồm cả các tính năng nâng cao như Promotions, AutoSave và Import Excel. Em xin hết và sẵn sàng trả lời câu hỏi ạ."
