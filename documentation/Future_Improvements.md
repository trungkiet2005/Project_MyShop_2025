# Định hướng Phát triển (Future Improvements)

Nếu có thêm thời gian hoặc phát triển lên phiên bản 2.0, đây là những tính năng em sẽ bổ sung:

## 1. Mở rộng Kiến trúc
*   **Chuyển sang Client-Server thực thụ**:
    *   Hiện tại dùng SQLite (Local). Tương lai sẽ tách Backend thành **ASP.NET Core Web API** và Database là **SQL Server/PostgreSQL**.
    *   App WinUI sẽ gọi API thay vì truy xuất trực tiếp DB.
    *   Lợi ích: Nhiều máy tính có thể bán hàng cùng lúc, dữ liệu đồng bộ.

## 2. Tính năng Nâng cao
*   **Phân quyền (RBAC)**:
    *   Thêm role: `Admin`, `Staff`, `Manager`.
    *   Nhân viên chỉ được tạo đơn, không được xem báo cáo doanh thu hay xóa sản phẩm.
*   **Thanh toán Online**:
    *   Tích hợp VietQR hoặc MoMo API để quét mã thanh toán tự động cập nhật trạng thái đơn hàng.
*   **In hóa đơn thật**:
    *   Kết nối máy in nhiệt (POS Printer) qua cổng USB/LAN thay vì chỉ xuất PDF.

## 3. Trải nghiệm người dùng (UX)
*   **Dark Mode hoàn chỉnh**: Đồng bộ toàn bộ giao diện theo theme hệ thống.
*   **Phím tắt (Shortcuts)**: F1 tạo đơn, F2 tìm kiếm... để thao tác nhanh hơn mà không cần chuột.

## 4. Cloud & Mobile
*   **Mobile App**: Viết thêm app MAUI cho điện thoại để chủ cửa hàng xem báo cáo từ xa.
*   **Cloud Backup**: Backup database lên Google Drive/OneDrive tự động hàng ngày.
