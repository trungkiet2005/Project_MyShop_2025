# 🎓 VIVA Q&A CHUẨN BỊ ĐẦY ĐỦ - MyShop 2025

> **Mục tiêu**: Trả lời được BẤT KỲ câu hỏi nào về project này

---

## 📚 MỤC LỤC

1. [Tổng quan dự án](#1-tổng-quan-dự-án)
2. [Kiến trúc & Design Patterns](#2-kiến-trúc--design-patterns)
3. [Database & Entity Framework](#3-database--entity-framework)
4. [WinUI 3 & XAML](#4-winui-3--xaml)
5. [Các tính năng chính (B1-B7)](#5-các-tính-năng-chính-b1-b7)
6. [Các tính năng tự chọn (C1-C9)](#6-các-tính-năng-tự-chọn-c1-c9)
7. [Security & Authentication](#7-security--authentication)
8. [Testing & Quality](#8-testing--quality)
9. [Deployment & Packaging](#9-deployment--packaging)
10. [Câu hỏi nâng cao](#10-câu-hỏi-nâng-cao)

---

## 1. TỔNG QUAN DỰ ÁN

### Q1.1: Dự án của em là gì? Mô tả tổng quan?
**A**: MyShop 2025 là ứng dụng **quản lý bán hàng desktop** được xây dựng bằng:
- **Frontend**: WinUI 3 (Windows App SDK 1.8)
- **Backend**: .NET 9, Entity Framework Core 9
- **Database**: SQLite (embedded, serverless)
- **Pattern**: MVVM + Dependency Injection

**Các module chính**:
1. Dashboard - Tổng quan kinh doanh
2. Products - Quản lý sản phẩm & danh mục
3. Orders - Quản lý đơn hàng & in hóa đơn
4. Customers - Quản lý khách hàng
5. Promotions - Quản lý khuyến mãi
6. Reports - Báo cáo doanh thu với biểu đồ
7. Settings - Cấu hình hệ thống

### Q1.2: Tại sao chọn WinUI 3 thay vì WPF?
**A**:
- WinUI 3 là **thế hệ mới nhất** của UI framework Microsoft
- Hỗ trợ **Fluent Design System** với hiệu ứng Acrylic, Mica
- Chạy trên **Windows 10 1809+** và Windows 11
- Được **đóng gói riêng** (không phụ thuộc vào Windows version)
- WPF đã cũ (2006), WinUI 3 ra mắt 2021

### Q1.3: Cấu trúc thư mục project như thế nào?
**A**:
```
Project_MyShop_2025/
├── Project_MyShop_2025/          # Main WinUI App
│   ├── Views/                     # XAML Pages + Code-behind
│   ├── ViewModels/                # MVVM ViewModels
│   ├── Assets/                    # Images, Icons
│   └── App.xaml.cs                # DI Configuration
│
├── Project_MyShop_2025.Core/      # Business Logic Layer
│   ├── Models/                    # Entity Classes
│   ├── Data/                      # DbContext, Seeders
│   ├── Services/                  # Business Services
│   │   ├── Interfaces/            # Service Contracts
│   │   └── Implementations/       # Service Logic
│   ├── Helpers/                   # Utilities (Password, etc.)
│   └── Migrations/                # EF Core Migrations
│
├── Project_MyShop_2025.Tests/     # Unit Tests
├── documentation/                 # Technical Docs
└── installer.iss                  # Inno Setup Script
```

### Q1.4: Điểm mạnh/khác biệt của project so với các project khác?
**A**:
1. **UI hiện đại**: Sử dụng thiết kế card-based, gradient, shadows
2. **Biểu đồ tự vẽ**: Không dùng thư viện, tự vẽ bằng XAML Shapes
3. **AutoSave**: Tự động lưu draft khi đang nhập liệu
4. **Secure Password**: Hash HMACSHA512 + Salt, không lưu plain text
5. **Export Excel**: Xuất báo cáo ra file .xlsx với MiniExcel
6. **Print to PDF/XPS**: In đơn hàng

---

## 2. KIẾN TRÚC & DESIGN PATTERNS

### Q2.1: MVVM là gì? Tại sao dùng MVVM?
**A**: **Model-View-ViewModel** là pattern tách biệt:
- **Model**: Dữ liệu (Product, Order, Customer entities)
- **View**: Giao diện XAML (ProductsPage.xaml)
- **ViewModel**: Logic kết nối View với Model

**Lợi ích**:
1. **Testable**: Test ViewModel không cần UI
2. **Maintainable**: Thay đổi UI không ảnh hưởng logic
3. **Data Binding**: 2-way binding tự động sync data
4. **Separation of Concerns**: Mỗi layer có trách nhiệm riêng

### Q2.2: Dependency Injection (DI) là gì? Dùng ở đâu?
**A**: DI là kỹ thuật "tiêm" dependency từ bên ngoài thay vì tự tạo.

**Cấu hình trong `App.xaml.cs`**:
```csharp
var services = new ServiceCollection();
services.AddDbContext<ShopDbContext>(options => 
    options.UseSqlite(connectionString));
services.AddScoped<IProductService, ProductService>();
services.AddScoped<ICustomerService, CustomerService>();
services.AddScoped<IPromotionService, PromotionService>();
Services = services.BuildServiceProvider();
```

**Sử dụng trong Page**:
```csharp
var app = (App)Application.Current;
using var scope = app.Services.CreateScope();
var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
```

**Lợi ích**:
1. **Loose Coupling**: Không phụ thuộc implementation cụ thể
2. **Testable**: Dễ mock service cho unit test
3. **Configurable**: Thay đổi implementation chỉ cần sửa 1 chỗ

### Q2.3: Repository Pattern là gì? Có dùng không?
**A**: Có, **ngầm định qua DbContext**. EF Core DbContext đã là Unit of Work + Repository.
- `_context.Products` = ProductRepository
- `_context.Orders` = OrderRepository
- `_context.SaveChanges()` = Commit unit of work

### Q2.4: Service Layer pattern?
**A**: Có! Tất cả business logic nằm trong `Services/`:
- `IProductService` / `ProductService`
- `ICustomerService` / `CustomerService`  
- `IPromotionService` / `PromotionService`
- `IAutoSaveService` / `AutoSaveService`
- `IPrintService` / `PrintService`

**Ví dụ `PromotionService`**:
```csharp
public async Task<int> CalculateDiscountAsync(string code, int subtotal)
{
    var promo = await _context.Promotions
        .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);
    
    if (promo == null) return 0;
    
    return promo.DiscountType switch
    {
        DiscountType.Percentage => subtotal * promo.DiscountValue / 100,
        DiscountType.FixedAmount => promo.DiscountValue,
        _ => 0
    };
}
```

---

## 3. DATABASE & ENTITY FRAMEWORK

### Q3.1: Tại sao chọn SQLite?
**A**:
1. **Embedded/Serverless**: Không cần cài đặt server
2. **Portable**: Chỉ 1 file `.db`, dễ backup/restore
3. **Cross-platform**: Chạy được trên Windows/Linux/Mac
4. **Lightweight**: Phù hợp ứng dụng desktop đơn lẻ
5. **Đề bài cho phép**: "Database tùy chọn"

### Q3.2: Database schema có những bảng nào?
**A**: 8 bảng chính:
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  Categories │    │  Products   │    │ProductImages│
│─────────────│    │─────────────│    │─────────────│
│* Id         │◄───│* CategoryId │    │* ProductId  │
│  Name       │    │  Name       │───►│  ImagePath  │
│  Description│    │  SKU        │    │  DisplayOrder│
└─────────────┘    │  Price      │    └─────────────┘
                   │  ImportPrice│
                   │  Quantity   │
                   └─────────────┘
                          │
┌─────────────┐    ┌──────┴──────┐    ┌─────────────┐
│  Customers  │    │   Orders    │    │ Promotions  │
│─────────────│    │─────────────│    │─────────────│
│* Id         │◄───│* CustomerId │───►│* Id         │
│  Name       │    │  TotalPrice │    │  Code       │
│  Phone      │    │  Status     │    │  DiscountType│
│  Address    │    │  PromotionId│───►│  DiscountValue│
│  LoyaltyPts │    │  CreatedAt  │    │  StartDate  │
└─────────────┘    └──────┬──────┘    │  EndDate    │
                          │           └─────────────┘
                   ┌──────┴──────┐    ┌─────────────┐
                   │ OrderItems  │    │   Users     │
                   │─────────────│    │─────────────│
                   │* OrderId    │    │* Id         │
                   │* ProductId  │    │  Username   │
                   │  Quantity   │    │  PasswordHash│
                   │  Price      │    │  PasswordSalt│
                   │  TotalPrice │    │  Role       │
                   └─────────────┘    └─────────────┘
```

### Q3.3: EF Core Code First là gì? Migration hoạt động ra sao?
**A**: 
- **Code First**: Viết C# class trước, EF tự tạo database
- **Migration**: Theo dõi thay đổi schema

**Các lệnh Migration**:
```bash
# Tạo migration mới
dotnet ef migrations add AddCustomerTable

# Áp dụng migration vào database
dotnet ef database update

# Tạo script SQL
dotnet ef migrations script
```

### Q3.4: Giải thích một Entity relationship?
**A**: Ví dụ **Order - OrderItem - Product**:
```csharp
public class Order
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }        // Navigation
    public List<OrderItem> Items { get; set; }     // 1-to-many
    public int? PromotionId { get; set; }
    public Promotion? Promotion { get; set; }      // Navigation
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }               // FK to Order
    public int ProductId { get; set; }
    public Product Product { get; set; }           // FK to Product
    public int Quantity { get; set; }
    public int Price { get; set; }
}
```

### Q3.5: Eager Loading vs Lazy Loading?
**A**: Em dùng **Eager Loading** với `.Include()`:
```csharp
var orders = await _context.Orders
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .Include(o => o.Customer)
    .Where(o => o.CreatedAt >= startDate)
    .ToListAsync();
```
- **Eager**: Load tất cả related data trong 1 query
- **Lazy**: Load khi access property (cần cấu hình, có thể gây N+1 problem)

---

## 4. WINUI 3 & XAML

### Q4.1: Data Binding là gì? Các mode binding?
**A**: Liên kết dữ liệu giữa XAML và C#.

**Các mode**:
- `OneWay`: Source → Target (mặc định)
- `TwoWay`: Source ↔ Target (cho input)
- `OneTime`: Chỉ lần đầu

**Ví dụ**:
```xml
<TextBox Text="{x:Bind ViewModel.ProductName, Mode=TwoWay}"/>
<TextBlock Text="{x:Bind ViewModel.TotalPrice}"/>
```

### Q4.2: x:Bind vs Binding? Khác nhau gì?
**A**:
| Feature | x:Bind | Binding |
|---------|--------|---------|
| Performance | Compile-time, nhanh hơn | Runtime, chậm hơn |
| Type Safety | Có | Không |
| Default Mode | OneTime | OneWay |
| DataContext | Không cần | Cần set |

Em dùng **x:Bind** cho performance tốt hơn.

### Q4.3: INotifyPropertyChanged là gì?
**A**: Interface để notify UI khi property thay đổi.

```csharp
public class ProductDisplayModel : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();  // Notify UI
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### Q4.4: ContentDialog là gì? Dùng ở đâu?
**A**: Modal dialog của WinUI 3 để hiển thị popup.

**Ví dụ Add Product**:
```csharp
var dialog = new ContentDialog
{
    Title = "Add New Product",
    PrimaryButtonText = "Add",
    CloseButtonText = "Cancel",
    XamlRoot = this.XamlRoot,
    Content = new ScrollViewer { Content = content, MaxHeight = 500 }
};

var result = await dialog.ShowAsync();
if (result == ContentDialogResult.Primary)
{
    // Save product
}
```

### Q4.5: Cách vẽ biểu đồ Revenue Chart?
**A**: Tự vẽ bằng **XAML Shapes** trên **Canvas**:

```csharp
// Vẽ bar chart
var bar = new Rectangle
{
    Width = barWidth,
    Height = (revenue / maxRevenue) * chartHeight,
    Fill = new SolidColorBrush(Colors.Blue),
    RadiusX = 4, RadiusY = 4
};
Canvas.SetLeft(bar, x);
Canvas.SetTop(bar, chartHeight - barHeight);
RevenueChart.Children.Add(bar);

// Vẽ line chart
var polyline = new Polyline
{
    Points = pointCollection,
    Stroke = new SolidColorBrush(Colors.Orange),
    StrokeThickness = 3
};
RevenueChart.Children.Add(polyline);
```

---

## 5. CÁC TÍNH NĂNG CHÍNH (B1-B7)

### Q5.1: B1 - Đăng nhập: Làm sao hash password?
**A**: Dùng **HMACSHA512** với Salt:
```csharp
public static void CreatePasswordHash(string password, 
    out string passwordHash, out string passwordSalt)
{
    using var hmac = new HMACSHA512();
    passwordSalt = Convert.ToBase64String(hmac.Key);  // Random salt
    passwordHash = Convert.ToBase64String(
        hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
}

public static bool VerifyPasswordHash(string password, 
    string storedHash, string storedSalt)
{
    var saltBytes = Convert.FromBase64String(storedSalt);
    using var hmac = new HMACSHA512(saltBytes);
    var computedHash = Convert.ToBase64String(
        hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    return computedHash == storedHash;
}
```

**Tại sao Salt?** Chống rainbow table attack.

### Q5.2: B2 - Dashboard hiển thị gì?
**A**:
1. **KPI Cards**: Tổng sản phẩm, đơn hôm nay, doanh thu hôm nay
2. **Low Stock Alert**: Top 5 sản phẩm sắp hết hàng (< 5 units)
3. **Best Sellers**: Top 5 sản phẩm bán chạy nhất
4. **Recent Orders**: 5 đơn hàng gần nhất
5. **Revenue Chart**: Biểu đồ doanh thu theo ngày/tuần/tháng/năm
6. **Sparklines**: Mini charts cho trend 7 ngày

### Q5.3: B3 - Quản lý sản phẩm có những gì?
**A**:
1. **CRUD**: Thêm, sửa, xóa sản phẩm
2. **Phân trang**: 20/40/60 items per page
3. **Tìm kiếm**: Theo tên, SKU
4. **Lọc nâng cao**: Theo category, giá, tồn kho
5. **Sắp xếp**: Theo tên, giá, số lượng
6. **Import Excel**: Nhập hàng loạt từ file .xlsx
7. **Quản lý Category**: Thêm/sửa danh mục
8. **Upload ảnh**: Chọn ảnh sản phẩm từ máy

### Q5.4: B4 - Quản lý đơn hàng flow như thế nào?
**A**: 
```
Tạo đơn → Chọn khách hàng → Thêm sản phẩm → Áp dụng khuyến mãi → Lưu
    │
    ▼
Status: Created → Paid → Cancelled
    │               │
    ▼               ▼
  In đơn         Hủy đơn
```

**Order Status**:
- `Created`: Mới tạo
- `Paid`: Đã thanh toán
- `Cancelled`: Đã hủy

### Q5.5: B5 - Báo cáo có những loại nào?
**A**:
1. **KPI Summary**: Tổng doanh thu, lợi nhuận, số đơn
2. **Revenue + Profit Chart**: Biểu đồ cột
3. **Product Sales Chart**: Biểu đồ đường
4. **Top 5 Products**: Sản phẩm bán chạy
5. **Orders by Status**: Phân bố trạng thái
6. **Export to Excel**: Xuất ra file .xlsx

### Q5.6: B6 - Settings lưu ở đâu?
**A**: Dùng `Windows.Storage.ApplicationData.Current.LocalSettings`:
```csharp
var localSettings = ApplicationData.Current.LocalSettings;
localSettings.Values["ItemsPerPage"] = 20;
localSettings.Values["RememberMe_Username"] = "admin";
```
**Đường dẫn**: `%LOCALAPPDATA%\Packages\[AppId]\LocalState\`

### Q5.7: B7 - Cách tạo file Installer?
**A**: Dùng **Inno Setup**:
1. File `installer.iss` định nghĩa:
   - AppName, Version
   - Source files (từ Release/Portable)
   - Destination (Program Files)
   - Shortcuts, Icons
2. Compile → Tạo `MyShop2025_Setup_1.0.0.exe`

---

## 6. CÁC TÍNH NĂNG TỰ CHỌN (C1-C9)

### Q6.1: C1 - MVVM đã implement ở đâu?
**A**: 
- **Models**: `Product`, `Order`, `Customer` trong Core/Models
- **Views**: Các Pages trong Views/
- **ViewModels**: Các DisplayModel (ProductDisplayModel, OrderDisplayModel)
- **Binding**: x:Bind trong XAML

### Q6.2: C2 - Promotion System hoạt động thế nào?
**A**: 3 loại khuyến mãi:
1. **Percentage**: Giảm X% (VD: 10% off)
2. **FixedAmount**: Giảm cố định (VD: 50,000đ)
3. **BuyXGetY**: Mua X tặng Y (VD: Mua 2 tặng 1)

**Validation**:
```csharp
public async Task<Promotion?> ValidateCodeAsync(string code, int subtotal)
{
    var promo = await _context.Promotions.FirstOrDefaultAsync(p => 
        p.Code == code && 
        p.IsActive && 
        p.StartDate <= DateTime.Now && 
        p.EndDate >= DateTime.Now &&
        subtotal >= p.MinOrderValue &&
        (p.UsageLimit == null || p.UsageCount < p.UsageLimit));
    return promo;
}
```

### Q6.3: C3 - Tìm kiếm nâng cao có gì?
**A**:
- **Price Range Filter**: Min/Max price
- **Stock Range Filter**: Min/Max quantity
- **Category Filter**: Dropdown + Pills
- **Multi-sort**: Tên, giá, tồn kho (ASC/DESC)
- **Quick Filters**: Low Stock, Out of Stock

### Q6.4: C4 - In đơn hàng dùng gì?
**A**: Không dùng thư viện ngoài, dùng **PrintDocument API**:
```csharp
public async Task PrintOrderAsync(Order order)
{
    var printManager = PrintManagerInterop.GetForWindow(hwnd);
    printManager.PrintTaskRequested += (sender, args) =>
    {
        var printTask = args.Request.CreatePrintTask("Order", 
            sourceRequest => {
                // Tạo print content từ XAML
            });
    };
    await PrintManagerInterop.ShowPrintUIAsync(hwnd);
}
```

### Q6.5: C5 - Customer có loyalty points không?
**A**: Có! Mỗi khách hàng có:
- `LoyaltyPoints`: Điểm tích lũy
- `Notes`: Ghi chú riêng
- `IsActive`: Trạng thái hoạt động
- Lịch sử đơn hàng liên kết qua `CustomerId`

### Q6.6: C6 - Test cases có bao nhiêu? Chạy thế nào?
**A**: 
- **Project**: `Project_MyShop_2025.Tests`
- **Framework**: xUnit
- **Số lượng**: 6 test cases
- **Chạy**: `dotnet test`

**Test types**:
1. Product CRUD tests
2. Order creation tests
3. Promotion validation tests

### Q6.7: C7 - Dependency Injection configured ở đâu?
**A**: Trong `App.xaml.cs`:
```csharp
private void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ShopDbContext>(options => 
        options.UseSqlite(connectionString));
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IPromotionService, PromotionService>();
    services.AddScoped<IAutoSaveService, AutoSaveService>();
    services.AddScoped<IPrintService, PrintService>();
}
```

### Q6.8: C8 - Backup/Restore database?
**A**: Vì SQLite là single file:
```csharp
// Backup
var sourceFile = "myshop.db";
var backupFile = await savePicker.PickSaveFileAsync();
File.Copy(sourceFile, backupFile.Path, overwrite: true);

// Restore
var restoreFile = await openPicker.PickSingleFileAsync();
File.Copy(restoreFile.Path, sourceFile, overwrite: true);
```

### Q6.9: C9 - AutoSave hoạt động thế nào?
**A**: Lưu JSON vào LocalFolder khi user typing:
```csharp
public class AutoSaveService : IAutoSaveService
{
    public async Task SaveDraftAsync<T>(string key, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync($"{key}.json", 
            CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, json);
    }

    public async Task<T?> LoadDraftAsync<T>(string key)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.TryGetItemAsync($"{key}.json");
        if (file is StorageFile f)
        {
            var json = await FileIO.ReadTextAsync(f);
            return JsonSerializer.Deserialize<T>(json);
        }
        return default;
    }
}
```

---

## 7. SECURITY & AUTHENTICATION

### Q7.1: Password được bảo mật như thế nào?
**A**:
1. **Never store plain text**: Chỉ lưu hash
2. **HMACSHA512**: Thuật toán hash mạnh
3. **Unique Salt**: Mỗi user có salt riêng
4. **Timing-safe comparison**: Tránh timing attack

### Q7.2: Tại sao không dùng SHA256 mà dùng HMACSHA512?
**A**:
- SHA256 là hash function, không có key
- HMACSHA512 = Hash + Key (Salt), chống rainbow table
- 512-bit output dài hơn, khó brute force hơn

### Q7.3: Session được quản lý thế nào?
**A**: Dùng LocalSettings:
```csharp
// Login với Remember Me
if (RememberMeCheckBox.IsChecked == true)
    localSettings.Values["RememberMe_Username"] = username;

// Auto-login khi mở app
if (localSettings.Values.ContainsKey("RememberMe_Username"))
{
    var user = context.Users.FirstOrDefault(u => u.Username == username);
    if (user != null) NavigateToMain();
}

// Logout
localSettings.Values.Remove("RememberMe_Username");
```

---

## 8. TESTING & QUALITY

### Q8.1: Các loại test đã thực hiện?
**A**:
1. **Unit Tests**: Test service methods isolated
2. **Integration Tests**: Test với in-memory SQLite
3. **Manual Testing**: Kiểm tra UI flows

### Q8.2: Cách test với database?
**A**: Dùng **In-Memory SQLite**:
```csharp
[Fact]
public async Task CreateProduct_ShouldAddToDatabase()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ShopDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    using var context = new ShopDbContext(options);
    
    // Act
    context.Products.Add(new Product { Name = "Test", Price = 100 });
    await context.SaveChangesAsync();
    
    // Assert
    Assert.Equal(1, await context.Products.CountAsync());
}
```

### Q8.3: Code quality tools?
**A**:
- **Nullable reference types**: Enable trong .csproj
- **EditorConfig**: Coding conventions
- **Git**: Version control
- **Documentation**: 14 markdown files

---

## 9. DEPLOYMENT & PACKAGING

### Q9.1: WindowsPackageType=None là gì?
**A**: Cho phép chạy app **không cần MSIX package**:
```xml
<WindowsPackageType>None</WindowsPackageType>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
```
- App chạy như traditional .exe
- Không cần sign certificate
- Dễ distribute

### Q9.2: Self-contained vs Framework-dependent?
**A**:
| Feature | Self-contained | Framework-dependent |
|---------|---------------|---------------------|
| Size | Lớn (~100MB) | Nhỏ (~10MB) |
| Requires .NET | Không | Có |
| Portable | Cao | Thấp |

Em dùng **Self-contained** để user không cần cài .NET.

### Q9.3: Cách publish app?
**A**:
```bash
dotnet publish -c Release -o Release/Portable --self-contained true
```

---

## 10. CÂU HỎI NÂNG CAO

### Q10.1: Nếu có 1 triệu sản phẩm, performance sẽ như thế nào?
**A**: Cần optimize:
1. **Pagination**: Đã có, chỉ load page cần thiết
2. **Index**: Thêm index cho columns hay query
3. **Async/Await**: Đã dùng, không block UI
4. **Virtual scrolling**: Có thể thêm cho ListView

### Q10.2: Làm sao scale lên multi-user?
**A**: Cần thay đổi:
1. **Database**: Chuyển SQLite → SQL Server/PostgreSQL
2. **Backend**: Thêm ASP.NET Core Web API
3. **Authentication**: JWT tokens
4. **Concurrency**: Optimistic locking

### Q10.3: Nếu được làm lại, em sẽ thay đổi gì?
**A**:
1. Dùng **CommunityToolkit.Mvvm source generators** cho ViewModel
2. Thêm **logging** với Serilog
3. Implement **Unit of Work** pattern rõ ràng hơn
4. Dùng **FluentValidation** cho validation phức tạp

### Q10.4: Async/Await hoạt động thế nào?
**A**: Non-blocking asynchronous programming:
```csharp
// Không block UI thread
var products = await _context.Products.ToListAsync();

// Task-based Asynchronous Pattern
public async Task<List<Product>> GetProductsAsync()
{
    return await _context.Products
        .Include(p => p.Category)
        .OrderBy(p => p.Name)
        .ToListAsync();
}
```

### Q10.5: LINQ Query vs Method Syntax?
**A**: Em dùng **Method Syntax** vì dễ chain:
```csharp
// Method Syntax (dùng)
var result = products
    .Where(p => p.Price > 1000)
    .OrderBy(p => p.Name)
    .Select(p => new { p.Name, p.Price });

// Query Syntax (không dùng)
var result = from p in products
             where p.Price > 1000
             orderby p.Name
             select new { p.Name, p.Price };
```

---

## 📝 TIPS CHO VIVA

1. **Tự tin**: Bạn đã xây dựng project này, bạn hiểu nó!
2. **Trả lời ngắn gọn**: Đi thẳng vào điểm chính
3. **Nếu không biết**: "Em chưa research phần này, nhưng em nghĩ..."
4. **Demo**: Mở app, show code khi được hỏi
5. **Giải thích code**: Có thể mở file và chỉ trực tiếp

---

**Chúc bạn VIVA thành công! 🎓🎉**
