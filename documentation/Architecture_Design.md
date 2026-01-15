# Architecture & Design Overview

## 1. High-Level Architecture
Project MyShop 2025 được xây dựng dựa trên kiến trúc **MVVM (Model-View-ViewModel)**, một mẫu thiết kế tiêu chuẩn cho các ứng dụng WPF/WinUI để tách biệt giao diện (UI) và logic nghiệp vụ.

### Các lớp chính (Layers):
1.  **Views (UI Layer)**: Chứa các file `.xaml` và `.xaml.cs` (code-behind). Chỉ chịu trách nhiệm hiển thị và nhận tương tác người dùng.
    *   *Ví dụ*: `DashboardPage.xaml`, `OrdersPage.xaml`.
2.  **ViewModels (Presentation Logic)**: Cầu nối giữa View và Model. Chứa dữ liệu (`ObservableProperty`) và lệnh (`RelayCommand`) để binding vào View. Không phụ thuộc vào UI cụ thể.
    *   *Ví dụ*: `DashboardViewModel.cs`.
3.  **Services (Business Logic)**: Xử lý logic nghiệp vụ, tính toán, và gọi xuống tầng Data.
    *   *Ví dụ*: `ProductService`, `OrderService`, `PromotionService`.
4.  **Core/Models (Data Layer)**: Định nghĩa các thực thể dữ liệu (Entity) và Context giao tiếp với Database (Entity Framework Core).
    *   *Ví dụ*: `Product`, `Order`, `ShopDbContext`.

---

## 2. Dependency Injection (DI)
Dự án sử dụng **Dependency Injection** (thư viện `Microsoft.Extensions.DependencyInjection`) để quản lý sự phụ thuộc giữa các thành phần.
*   **Tại sao dùng?**: Giúp code lỏng lẻo (loose coupling), dễ bảo trì và dễ viết Unit Test (Mocking).
*   **Cấu hình ở đâu?**: Trong `App.xaml.cs`, hàm `ConfigureServices`.
    ```csharp
    services.AddTransient<DashboardViewModel>(); // Tạo mới mỗi lần gọi
    services.AddScoped<IProductService, ProductService>(); // Sống theo scope (thường là request)
    services.AddDbContext<ShopDbContext>();
    ```

---

## 3. Cấu trúc thư mục
*   **Views/**: Màn hình giao diện.
*   **ViewModels/**: Logic cho từng màn hình.
*   **Core/Data/**: `ShopDbContext` (kết nối DB), `DatabasePathHelper` (lấy đường dẫn DB).
*   **Core/Models/**: Class đại diện bảng trong DB (`Product`, `Order`) và Enums (`OrderStatus`).
*   **Services/**: Logic xử lý chính (`IProductService`, `ProductService`).
*   **Helpers/**: Các hàm tiện ích (Format tiền tệ, chuyển đổi ảnh).

---

## 4. Công nghệ sử dụng
*   **WinUI 3 (Windows App SDK)**: Framework giao diện hiện đại nhất của Microsoft.
*   **Entity Framework Core (Sqlite)**: ORM để làm việc với Database (không cần viết SQL thủ công).
*   **CommunityToolkit.Mvvm**: Thư viện hỗ trợ viết MVVM nhanh (`[ObservableProperty]`, `[RelayCommand]`).
*   **MiniExcel**: Thư viện siêu nhẹ để đọc file Excel (Performance cao hơn EPPlus/OfficeInterop).
*   **System.Text.Json**: Xử lý AutoSave (lưu Draft ra file JSON).

---

## 5. Luồng dữ liệu (Data Flow)
Ví dụ: Khi nhấn nút "Load Dashboard":
1.  **View** (`DashboardPage`) gọi `ViewModel.LoadDataCommand`.
2.  **ViewModel** gọi `ProductService.GetBestSellers()`.
3.  **Service** gọi `ShopDbContext.Products`.
4.  **DbContext** truy vấn SQLite và trả về List Objects.
5.  **ViewModel** cập nhật property `BestSellers`.
6.  **View** tự động cập nhật UI nhờ cơ chế `INotifyPropertyChanged` (Binding).
