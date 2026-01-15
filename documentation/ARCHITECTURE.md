# MyShop 2025 - Architecture Documentation

## Tổng quan kiến trúc

### Tech Stack
- **Framework**: WinUI 3 (Windows App SDK 1.8)
- **Target Framework**: .NET 9.0 (Windows 10.0.19041)
- **Database**: SQLite với Entity Framework Core 9.0
- **DI Container**: Microsoft.Extensions.DependencyInjection 10.0.1
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.4.0

### Cấu trúc Solution

```
Project_MyShop_2025.sln
├── Project_MyShop_2025/           # UI Project (WinUI 3)
│   ├── App.xaml / App.xaml.cs     # Entry point, DI configuration
│   ├── MainWindow.xaml            # Main window container
│   ├── Views/                     # Pages (code-behind pattern)
│   │   ├── LoginPage.xaml(.cs)
│   │   ├── ShellPage.xaml(.cs)    # Navigation host
│   │   ├── DashboardPage.xaml(.cs)
│   │   ├── ProductsPage.xaml(.cs)
│   │   ├── OrdersPage.xaml(.cs)
│   │   ├── ReportsPage.xaml(.cs)
│   │   ├── ConfigPage.xaml(.cs)
│   │   └── ChatbotPage.xaml(.cs)
│   ├── ViewModels/                # ViewModels (minimal)
│   │   └── MainViewModel.cs
│   └── Assets/                    # Images, icons
│       └── product_image/         # Product images
│
└── Project_MyShop_2025.Core/      # Core Library (Business Logic)
    ├── Models/                    # Entity classes
    │   ├── Category.cs
    │   ├── Product.cs
    │   ├── ProductImage.cs
    │   ├── Order.cs
    │   ├── OrderItem.cs
    │   └── User.cs
    ├── Data/                      # Database layer
    │   ├── ShopDbContext.cs       # EF Core DbContext
    │   ├── ShopDbContextFactory.cs
    │   ├── DbSeeder.cs            # Demo data seeding
    │   └── DatabasePathHelper.cs
    └── Helpers/
        └── PasswordHelper.cs      # Password hashing (SHA256)
```

## Luồng điều hướng (Navigation Flow)

```
MainWindow
    └── Frame (content host)
            ├── LoginPage
            │       ├── → ShellPage (login success)
            │       └── → ConfigPage (settings)
            │
            └── ShellPage (NavigationView host)
                    └── ContentFrame
                            ├── DashboardPage
                            ├── ProductsPage
                            ├── OrdersPage
                            ├── ReportsPage
                            ├── ConfigPage (Settings)
                            └── ChatbotPage
```

## Database Schema (ER Diagram)

```
┌─────────────────┐       ┌─────────────────┐
│    CATEGORY     │       │     PRODUCT     │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │──────<│ Id (PK)         │
│ Name            │       │ Name            │
│ Description     │       │ SKU             │
└─────────────────┘       │ Description     │
                          │ Price           │
                          │ ImportPrice     │
                          │ Image           │
                          │ Quantity        │
                          │ CategoryId (FK) │
                          └────────┬────────┘
                                   │
                          ┌────────┴────────┐
                          │  PRODUCT_IMAGE  │
                          ├─────────────────┤
                          │ Id (PK)         │
                          │ ImagePath       │
                          │ DisplayOrder    │
                          │ ProductId (FK)  │
                          └─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│      ORDER      │       │   ORDER_ITEM    │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │──────<│ Id (PK)         │
│ CreatedAt       │       │ OrderId (FK)    │
│ TotalPrice      │       │ ProductId (FK)  │>────┐
│ Status          │       │ Quantity        │     │
│ CustomerName    │       │ Price           │     │
│ CustomerPhone   │       │ TotalPrice      │     │
│ CustomerAddress │       └─────────────────┘     │
└─────────────────┘                               │
                                                  │
                          ┌─────────────────┐     │
                          │     PRODUCT     │<────┘
                          └─────────────────┘

┌─────────────────┐
│      USER       │
├─────────────────┤
│ Id (PK)         │
│ Username        │
│ Password (Hash) │
│ FullName        │
│ Email           │
│ Role            │
└─────────────────┘
```

### Order Status Enum
```csharp
public enum OrderStatus
{
    Created = 0,    // Mới tạo
    Paid = 1,       // Đã thanh toán
    Cancelled = 2   // Đã hủy
}
```

## Dependency Injection - Hiện tại

```csharp
// App.xaml.cs
private void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<MainWindow>();
    services.AddTransient<ViewModels.MainViewModel>();
    services.AddDbContext<ShopDbContext>(options => options.UseSqlite(...));
}
```

**Vấn đề hiện tại:**
- Chỉ DI cơ bản cho DbContext và MainWindow
- Các Pages sử dụng code-behind, không inject services
- Logic nghiệp vụ nằm trực tiếp trong code-behind

## Các chức năng đã hoàn thành (Part A & B)

### B1. Đăng nhập ✅
- Auto-login với Remember Me
- Password hash bằng SHA256
- Hiển thị phiên bản ứng dụng
- Cấu hình server từ ConfigPage

### B2. Dashboard ✅
- Tổng số sản phẩm
- Top 5 sản phẩm sắp hết hàng
- Top 5 sản phẩm bán chạy
- Tổng đơn hàng/doanh thu trong ngày
- 3 đơn hàng gần nhất
- Biểu đồ doanh thu theo ngày

### B3. Quản lí sản phẩm ✅
- Xem danh sách theo loại, phân trang
- Sắp xếp theo tên/giá/tồn kho
- Lọc theo khoảng giá
- Tìm kiếm theo tên/SKU
- Thêm/Sửa/Xóa sản phẩm
- Thêm loại sản phẩm
- Import từ Excel/Access (skeleton)

### B4. Quản lí đơn hàng ✅
- CRUD đơn hàng
- Phân trang, lọc theo ngày
- Lọc theo trạng thái
- Cập nhật trạng thái đơn

### B5. Báo cáo thống kê ✅
- Biểu đồ sản phẩm bán theo thời gian
- Biểu đồ doanh thu/lợi nhuận
- Lọc theo ngày/tuần/tháng/năm

### B6. Cấu hình ✅
- Số sản phẩm/đơn hàng mỗi trang
- Ghi nhớ trang cuối
- Cấu hình database
- Lưu API key Gemini

## Các chức năng cần làm (Part C)

| STT | Chức năng | Điểm | Trạng thái |
|-----|-----------|------|------------|
| 1 | MVVM Architecture | 0.5 | ❌ |
| 2 | Khuyến mãi giảm giá | 1.0 | ❌ |
| 3 | Tìm kiếm nâng cao | 1.0 | ❌ |
| 4 | In đơn hàng (PDF/XPS) | 0.5 | ❌ |
| 5 | Quản lí khách hàng | 0.5 | ❌ |
| 6 | Test cases | 0.5 | ❌ |
| 7 | Dependency Injection | 0.5 | ❌ |
| 8 | Backup/Restore database | 0.25 | ❌ |
| 9 | Auto save | 0.25 | ❌ |
