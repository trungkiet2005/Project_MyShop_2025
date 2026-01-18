# 🎓 VIVA DEFENSE CHEAT SHEET (10/10 STRATEGY)

## ⏱️ The 30-Second Elevator Pitch
"Em xin chào thầy/cô. Đồ án **MyShop 2025** là một giải pháp quản lý bán hàng toàn diện, hiện đại được xây dựng trên **WinUI 3** và **.NET 9**.
Không chỉ đáp ứng các chức năng quản lý cơ bản (Sản phẩm, Đơn hàng, Khách hàng), ứng dụng còn tích hợp các công nghệ nâng cao như **Gemini AI** để hỗ trợ nội dung, **Báo cáo thời gian thực**, và cơ chế **Auto-save/Backup** an toàn.
Kiến trúc **MVVM** và **Dependency Injection** giúp hệ thống dễ bảo trì và mở rộng. Em tự tin sản phẩm đạt chuẩn thương mại và sẵn sàng demo chi tiết."

---

## 💎 Key Selling Points (Các điểm "ăn tiền")

Khi thầy cô hỏi "Đồ án có gì đặc biệt?", hãy trả lời 3-4 ý này:

1.  **Trải nghiệm người dùng (UX) hiện đại**:
    *   Sử dụng WinUI 3 mới nhất, Fluent Design.
    *   Tốc độ phản hồi cực nhanh (Virtualization cho danh sách lớn).
    *   Dark Mode/Light Mode hoàn chỉnh.
2.  **Tính năng AI (Điểm cộng lớn)**:
    *   Tự động tạo mô tả sản phẩm hấp dẫn bằng Gemini AI.
3.  **Độ tin cậy & An toàn**:
    *   **Auto-save draft**: Đang nhập liệu mà tắt máy cũng không mất (lưu local file JSON).
    *   **Backup/Restore Database**: Sao lưu dữ liệu an toàn.
4.  **Kiến trúc sạch (Clean Architecture)**:
    *   Tách biệt rõ ràng Core (Business Logic) và UI.
    *   Sử dụng Interface cho mọi Service -> Dễ test và thay thế.

---

## ❓ Common Q&A (Hỏi xoáy đáp xoay)

### 1. Kỹ thuật (Technical)

**Q: Tại sao dùng WinUI 3 mà không phải WPF?**
*   **A:** WinUI 3 là UI framework bản địa mới nhất của Microsoft, hỗ trợ hiệu năng cao, controls hiện đại và touch-friendly. Nó là tương lai của Windows App SDK.

**Q: Em xử lý danh sách hàng nghìn sản phẩm thế nào để không lag?**
*   **A:** Em sử dụng cơ chế **Pagination** (Phân trang) ở tầng Database (không load tất cả vào RAM) và kết hợp **UI Virtualization** của ListView để chỉ render những gì người dùng đang thấy.

**Q: Dependency Injection là gì và dùng ở đâu?**
*   **A:** Là kỹ thuật giảm sự phụ thuộc giữa các lớp. Em đăng ký các Service (IProductService, etc.) trong `App.xaml.cs` và inject vào ViewModel qua Constructor. Giúp code lỏng lẻo (loose coupling) và dễ Unit Test.

### 2. Nghiệp vụ (Business)

**Q: Làm sao tính doanh thu chính xác nếu giá sản phẩm thay đổi?**
*   **A:** Trong bảng `OrderDetail`, em lưu cứng giá bán (`Price`) tại thời điểm tạo đơn hàng, chứ không tham chiếu giá hiện tại của bảng `Product`.

**Q: Nếu xóa một category thì sản phẩm thuộc category đó sẽ ra sao?**
*   **A:** (Tùy logic em đã làm, thường là:) Hệ thống sẽ chặn xóa nếu Category đang có sản phẩm (Ràng buộc khóa ngoại), hoặc chuyển sản phẩm về Category "Uncategorized" để đảm bảo toàn vẹn dữ liệu.

---

## 🏃 Demo Flow (Kịch bản Demo suôn sẻ)

1.  **Đăng nhập**: Show chức năng "Remember Me".
2.  **Dashboard**: Mở lên thấy ngay biểu đồ chạy animation -> Ấn tượng đầu tiên.
3.  **Sản phẩm**:
    *   Thêm mới 1 sản phẩm.
    *   Dùng nút **"Ask AI"** để generate mô tả (Killer feature!).
    *   Show ảnh sản phẩm (đã thêm 66 ảnh đẹp).
4.  **Đơn hàng**:
    *   Tạo đơn mới, thêm vài món.
    *   Thử tắt form đột ngột -> Mở lại -> Show **Auto-save** draft còn nguyên.
    *   "Checkout" và In hóa đơn (Show PDF).
5.  **Cài đặt**:
    *   Backup Database.
    *   Đổi theme Dark/Light.

---

## ⚠️ Emergency Tips (Xử lý sự cố)

*   **Lỗi Demo**: "Thưa thầy, đây có thể là lỗi edge case do môi trường demo, em đã test kỹ case này ở nhà. Em xin phép demo chức năng tiếp theo và quay lại sau."
*   **Quên câu trả lời**: "Câu hỏi rất hay ạ. Theo thiết kế hiện tại thì em làm theo hướng X, nhưng em ghi nhận ý kiến của thầy để tối ưu theo hướng Y trong version sau."
*   **App Crash**: Bình tĩnh mở lại. "Em xin lỗi, có thể do xung đột tài nguyên máy ảo/máy chiếu." (Chạy file exe trong folder Release cho ổn định nhất).
