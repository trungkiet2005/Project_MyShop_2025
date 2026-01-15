# Tính năng & Giải thích Kỹ thuật (Technical Deep Dive)

Tài liệu này giải thích chi tiết **CÁCH LÀM** (How-to) của các tính năng khó, phục vụ cho việc trả lời vấn đáp kỹ thuật.

---

## 1. Import Excel (B3) - `ProductsPage.xaml.cs`
*   **Yêu cầu**: Thêm nhiều sản phẩm cùng lúc từ file Excel.
*   **Cách làm**:
    1.  Dùng thư viện `MiniExcel` (nhẹ, nhanh, không cần cài Excel trên máy).
    2.  `FileOpenPicker`: Mở hộp thoại chọn file `.xlsx`.
    3.  `MiniExcel.Query(path)`: Đọc file Excel thành danh sách các dòng dữ liệu (dynamic row).
    4.  Vòng lặp `foreach`:
        *   Đọc cột Name, Price, SKU.
        *   Kiểm tra logic: Nếu SKU trùng thì bỏ qua (hoặc cập nhật tùy logic).
        *   Tự động tạo Category "Imported" nếu chưa có.
        *   `_context.Products.Add(newProduct)`.
    5.  `_context.SaveChangesAsync()`: Lưu 1 lần xuống DB (Batch insert) để tối ưu hiệu năng.

## 2. Promotions (Khuyến mãi - C2) - `OrdersPage.xaml.cs` & `PromotionService`
*   **Yêu cầu**: Nhập mã code -> Giảm giá tiền.
*   **Logic**:
    *   Bảng `Promotions` trong DB chứa: `Code`, `DiscountPercent`, `StartDate`, `EndDate`, `Status`.
    *   Khi nhập code và nhấn Apply:
        1.  Gọi `_promotionService.ValidateCodeAsync(code)`.
        2.  Kiểm tra: Code tồn tại? Còn hạn sử dụng? Đang Active?
        3.  Nếu hợp lệ: Trả về object Promotion.
        4.  Tính toán: `DiscountAmount = Total * (Percent / 100)`.
        5.  Lưu `PromotionId` vào `Order` khi tạo đơn hàng.
        6.  Giảm `UsageLimit` của Promotion (nếu có giới hạn số lần dùng).

## 3. AutoSave (Tự động lưu nháp - C9) - `AutoSaveService`
*   **Yêu cầu**: Đang nhập liệu mà tắt app/mất điện, mở lại vẫn còn.
*   **Cách làm**:
    *   Sử dụng cơ chế lưu file JSON cục bộ (`LocalState`).
    *   Mỗi khi người dùng gõ phím (`TextChanged` event) hoặc chọn item (`SelectionChanged`):
        *   Gọi `_autoSaveService.SaveDraftAsync(key, dataObject)`.
        *   Dữ liệu `OrderDraft` hoặc `ProductDraft` được Serialize thành chuỗi JSON và ghi đè lên file.
    *   Khi mở lại dialog (Load):
        *   Gọi `LoadDraftAsync`.
        *   Nếu file tồn tại -> Fill lại dữ liệu vào các ô TextBox.
    *   Khi Save thành công -> Xóa file draft (`ClearDraftAsync`).

## 4. Reports (Báo cáo & Biểu đồ - B5) - `ReportsPage.xaml.cs`
*   **Vẽ biểu đồ**: Không dùng thư viện bên thứ 3 (để tối ưu điểm số "tự làm"), mà dùng **WinUI Shapes** (`Rectangle`, `Line`, `Polygon`, `Canvas`).
*   **Biểu đồ Doanh thu (Cột)**:
    *   Tính toán tỷ lệ chiều cao: `Height = (Revenue / MaxRevenue) * ChartHeight`.
    *   Vẽ từng hình chữ nhật (`Rectangle`) và đặt lên `Canvas`.
*   **Biểu đồ Sản phẩm (Đường/Vùng)**:
    *   Nối các điểm dữ liệu (`Point`) bằng `Line` hoặc `Polygon` (để tô màu nền gradient).
*   **KPIs**: Sử dụng LINQ để tính toán `Sum`, `Count`, `Average` từ DB dựa trên `FromDate` và `ToDate`.

## 5. Security (Bảo mật Login - B1) - `AuthenticationService`
*   **Lưu mật khẩu**: **KHÔNG BAO GIỜ** lưu plain text.
*   **Cách làm**:
    *   Khi đăng ký/tạo user: `Hash = SHA256(Password + Salt)`.
    *   Khi đăng nhập: Hash password người dùng nhập vào -> So sánh với Hash trong DB.
    *   Sử dụng `System.Security.Cryptography.SHA256`.

## 6. Pagination (Phân trang)
*   Logic phân trang nằm ở câu lệnh LINQ:
    ```csharp
    var pagedData = allData
        .Skip((CurrentPage - 1) * PageSize) // Bỏ qua trang trước
        .Take(PageSize)                     // Lấy số lượng trang hiện tại
        .ToList();
    ```
*   Tối ưu: Chỉ query DB những dòng cần lấy (phân trang phía Server/DB) thay vì load hết về RAM rồi mới phân trang (đã implement trong `ProductService` và `OrderService`).
