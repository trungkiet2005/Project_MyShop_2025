# Bộ Câu Hỏi & Trả Lời Vấn Đáp (Viva Q&A)

Chuẩn bị sẵn các câu trả lời này để tự tin khi demo và trả lời giáo viên.

---

### Q1: Kiến trúc dự án em sử dụng là gì? Tại sao?
**A**: Em sử dụng kiến trúc **MVVM (Model-View-ViewModel)**.
*   **Lý do**: Tách biệt rõ ràng giữa giao diện (View) và code xử lý (ViewModel/Model). Giúp code dễ đọc, dễ bảo trì, dễ test và phù hợp với các ứng dụng XAML như WinUI/WPF. Nó cũng hỗ trợ Binding dữ liệu 2 chiều rất mạnh mẽ.

### Q2: Dependency Injection (DI) là gì? Em dùng nó ở đâu?
**A**: DI là kỹ thuật tiêm các sự phụ thuộc vào đối tượng thay vì để đối tượng tự khởi tạo.
*   Em dùng thư viện có sẵn của .NET (`Microsoft.Extensions.DependencyInjection`).
*   Example: `DashboardViewModel` cần `IProductService`. Thay vì `new ProductService()` trong ViewModel, em khai báo nó trong Constructor, và hệ thống sẽ tự đưa Service vào. Giúp em dễ dàng thay thế Service (ví dụ MockService để test) mà không sửa code ViewModel.

### Q3: Em xử lý Import Excel như thế nào?
**A**: Em dùng thư viện **MiniExcel**.
*   Khi người dùng chọn file, em đọc file đó thành danh sách các object động.
*   Sau đó em duyệt qua từng dòng, validate dữ liệu (check trùng SKU), convert dữ liệu sang `Product` entity.
*   Cuối cùng em dùng `DbContext.Products.Add()` và `SaveChanges` để lưu vào SQLite.

### Q4: Tính năng AutoSave hoạt động ra sao? Nó lưu trử ở đâu?
**A**: AutoSave hoạt động dựa trên sự kiện thay đổi text (`Publisher-Subscriber`).
*   Khi người dùng gõ, em lưu dữ liệu đó ra một file JSON tạm trong thư mục `ApplicationData.Current.LocalFolder` (AppData của máy).
*   Lần sau mở lên, em đọc file này và điền lại vào form.
*   Lưu vào Database chỉ xảy ra khi người dùng bấm nút "Save" hoặc "Add".

### Q5: Tại sao em dùng SQLite mà không dùng SQL Server?
**A**:
*   Yêu cầu đề bài là "Database tùy chọn".
*   SQLite là dạng **Embedded Database**, không cần cài đặt server (Serverless), phù hợp với ứng dụng Desktop nhỏ/vừa (ứng dụng bán hàng cho 1 máy).
*   Dễ dàng đóng gói, backup/restore (chỉ là copy file `.db`).

### Q6: Làm sao em vẽ được biểu đồ trong phần Báo cáo?
**A**: Em tự vẽ bằng các **SHAPES** của XAML (Rectangle, Line, Polygon) trên một Canvas.
*   Em tính toán toạ độ (X, Y) và kích thước (Width, Height) của từng cột/điểm dựa trên dữ liệu doanh thu thực tế.
*   Em không dùng thư viện thứ 3 để đảm bảo nắm vững kỹ thuật thao tác giao diện.

### Q7: Em đã test chương trình chưa?
**A**: Em đã thực hiện:
*   **Unit/Integration Test**: Có project `Project_MyShop_2025.Tests` sử dụng XUnit và In-memory Database để test logic thêm sửa xóa và import.
*   **Manual Test**: Đã kiểm tra kỹ các luồng chính như Tạo đơn, Nhập kho, Báo cáo số liệu.

### Q8: Câu lệnh LINQ nào em tâm đắc nhất?
**A**: Câu lệnh trong phần Báo cáo (Reports) hoặc Phân trang.
*   Ví dụ logic Group By trong báo cáo để gom nhóm doanh thu theo ngày/tháng:
    ```csharp
    var revenueByDate = orders
        .GroupBy(o => o.CreatedAt.Date)
        .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalPrice) });
    ```
