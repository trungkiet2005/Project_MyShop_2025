# MyShop 2025 - Implementation Plan for Part C

## Tổng quan các tính năng cần làm

| # | Feature | Points | Priority | Dependencies |
|---|---------|--------|----------|--------------|
| 1 | Dependency Injection | 0.5 | **High** | - (Foundation) |
| 2 | MVVM Architecture | 0.5 | **High** | DI |
| 3 | Customer Management | 0.5 | Medium | MVVM, DI |
| 4 | Promotion/Discount | 1.0 | **High** | Customer (optional) |
| 5 | Advanced Search | 1.0 | Medium | MVVM |
| 6 | Print Order (PDF/XPS) | 0.5 | Medium | - |
| 7 | Test Cases | 0.5 | Low | All features |
| 8 | Backup/Restore DB | 0.25 | Low | - |
| 9 | Auto Save | 0.25 | Low | - |

---

## 1. Dependency Injection (0.5đ)

### Mục tiêu
Refactor để sử dụng DI đúng cách thay vì tạo DbContext trực tiếp trong code-behind.

### Thay đổi

#### 1.1. Tạo Service Interfaces & Implementations
```
Core/
├── Services/
│   ├── Interfaces/
│   │   ├── IProductService.cs
│   │   ├── IOrderService.cs
│   │   ├── ICategoryService.cs
│   │   ├── ICustomerService.cs
│   │   └── IPromotionService.cs
│   └── Implementations/
│       ├── ProductService.cs
│       ├── OrderService.cs
│       ├── CategoryService.cs
│       ├── CustomerService.cs
│       └── PromotionService.cs
```

#### 1.2. Register Services in App.xaml.cs
```csharp
private void ConfigureServices(IServiceCollection services)
{
    // DbContext
    services.AddDbContext<ShopDbContext>(options => ...);
    
    // Services
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<ICategoryService, CategoryService>();
    // ...
    
    // ViewModels
    services.AddTransient<DashboardViewModel>();
    services.AddTransient<ProductsViewModel>();
    services.AddTransient<OrdersViewModel>();
    // ...
}
```

#### 1.3. Service Locator Pattern cho Pages
Do WinUI Pages không hỗ trợ constructor injection, sử dụng Service Locator:
```csharp
public partial class ProductsPage : Page
{
    private readonly IProductService _productService;
    
    public ProductsPage()
    {
        InitializeComponent();
        var app = (App)Application.Current;
        _productService = app.Services.GetRequiredService<IProductService>();
    }
}
```

---

## 2. MVVM Architecture (0.5đ)

### Mục tiêu
Chuyển đổi từ code-behind sang MVVM pattern sử dụng CommunityToolkit.Mvvm.

### Thay đổi

#### 2.1. Tạo ViewModels cho mỗi Page
```
ViewModels/
├── BaseViewModel.cs
├── DashboardViewModel.cs
├── ProductsViewModel.cs
├── OrdersViewModel.cs
├── ReportsViewModel.cs
├── CustomersViewModel.cs   # New
└── ConfigViewModel.cs
```

#### 2.2. BaseViewModel
```csharp
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _errorMessage;
    
    protected IServiceProvider Services { get; }
    
    public BaseViewModel(IServiceProvider services)
    {
        Services = services;
    }
}
```

#### 2.3. Example: ProductsViewModel
```csharp
public partial class ProductsViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    
    [ObservableProperty]
    private ObservableCollection<ProductDisplayModel> _products;
    
    [ObservableProperty]
    private string _searchText;
    
    [ObservableProperty]
    private Category _selectedCategory;
    
    [RelayCommand]
    private async Task LoadProductsAsync() { ... }
    
    [RelayCommand]
    private async Task AddProductAsync() { ... }
    
    [RelayCommand]
    private async Task DeleteProductAsync(int productId) { ... }
}
```

#### 2.4. Update XAML Bindings
```xml
<Page ...>
    <Grid>
        <GridView ItemsSource="{x:Bind ViewModel.Products, Mode=OneWay}">
            ...
        </GridView>
        
        <Button Command="{x:Bind ViewModel.AddProductCommand}"/>
    </Grid>
</Page>
```

---

## 3. Customer Management (0.5đ)

### Mục tiêu
Thêm entity Customer riêng và giao diện quản lý.

### Thay đổi

#### 3.1. Customer Model
```csharp
public class Customer
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Tích điểm
    public int LoyaltyPoints { get; set; }
    
    // Navigation
    public ICollection<Order> Orders { get; set; }
}
```

#### 3.2. Update Order Model
```csharp
public class Order
{
    // ... existing fields ...
    
    // Replace inline customer info
    public int? CustomerId { get; set; }
    
    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }
}
```

#### 3.3. CustomersPage
- Danh sách khách hàng với phân trang
- Tìm kiếm theo tên, SĐT
- CRUD operations
- Xem lịch sử đơn hàng của KH
- Hiển thị tổng chi tiêu, điểm tích lũy

---

## 4. Promotion/Discount (1.0đ)

### Mục tiêu
Hệ thống khuyến mãi giảm giá cho sản phẩm và đơn hàng.

### Thay đổi

#### 4.1. Promotion Model
```csharp
public enum PromotionType
{
    Percentage,     // Giảm %
    FixedAmount,    // Giảm số tiền cố định
    BuyXGetY,       // Mua X tặng Y
    FreeShipping    // Miễn phí vận chuyển
}

public class Promotion
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Code { get; set; }  // Mã khuyến mãi
    
    public string Name { get; set; }
    public string Description { get; set; }
    
    public PromotionType Type { get; set; }
    
    public decimal DiscountValue { get; set; }  // % hoặc amount tùy Type
    
    public int? MinOrderAmount { get; set; }  // Đơn tối thiểu
    public int? MaxDiscountAmount { get; set; }  // Giảm tối đa
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public bool IsActive { get; set; }
    
    public int? UsageLimit { get; set; }  // Số lần sử dụng tối đa
    public int UsedCount { get; set; }
    
    // Áp dụng cho sản phẩm cụ thể
    public int? CategoryId { get; set; }
    public int? ProductId { get; set; }
}
```

#### 4.2. Update Order Model
```csharp
public class Order
{
    // ... existing fields ...
    
    public int? PromotionId { get; set; }
    public string? PromotionCode { get; set; }
    public int DiscountAmount { get; set; }
    public int SubTotal { get; set; }  // Trước giảm giá
    // TotalPrice = SubTotal - DiscountAmount
}
```

#### 4.3. PromotionService
```csharp
public interface IPromotionService
{
    Task<Promotion?> ValidateCodeAsync(string code);
    Task<int> CalculateDiscountAsync(string code, int orderSubtotal, List<OrderItem> items);
    Task<bool> ApplyPromotionAsync(int orderId, string code);
    Task<List<Promotion>> GetActivePromotionsAsync();
}
```

#### 4.4. UI Changes
- **Create Order**: Thêm input nhập mã khuyến mãi
- **Promotions Page**: Quản lý các chương trình KM
- **Products Page**: Hiển thị badge "Sale" nếu có KM

---

## 5. Advanced Search (1.0đ)

### Mục tiêu
Tìm kiếm nâng cao với nhiều tiêu chí kết hợp.

### Thay đổi

#### 5.1. Search criteria
- Tìm kiếm theo nhiều fields cùng lúc
- Dùng operators: AND, OR, NOT
- Full-text search
- Saved searches
- Search suggestions/autocomplete

#### 5.2. ProductSearchCriteria
```csharp
public class ProductSearchCriteria
{
    public string? Keyword { get; set; }
    public List<int>? CategoryIds { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public bool? InStock { get; set; }
    public bool? HasPromotion { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
```

#### 5.3. OrderSearchCriteria
```csharp
public class OrderSearchCriteria
{
    public string? Keyword { get; set; }  // ID, Customer name
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<OrderStatus>? Statuses { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public int? CustomerId { get; set; }
    public bool? HasPromotion { get; set; }
}
```

#### 5.4. Advanced Search UI
- Expandable search panel
- Multiple filter chips
- Clear all filters button
- Save search as preset

---

## 6. Print Order to PDF/XPS (0.5đ)

### Mục tiêu
In đơn hàng thành file PDF hoặc XPS.

### Implementation

#### 6.1. PrintService
```csharp
public interface IPrintService
{
    Task PrintOrderAsync(Order order);
    Task ExportOrderToPdfAsync(Order order, string filePath);
    Task ExportOrderToXpsAsync(Order order, string filePath);
}
```

#### 6.2. Order Template
Sử dụng FlowDocument hoặc HTML template:
```
┌─────────────────────────────────────────┐
│          MY SHOP 2025                   │
│         ĐƠN HÀNG #123                   │
├─────────────────────────────────────────┤
│ Ngày: 15/01/2026                        │
│ Khách hàng: Nguyễn Văn A                │
│ SĐT: 0901234567                         │
│ Địa chỉ: 123 ABC Street                 │
├───────┬─────────┬────────┬──────────────┤
│ STT   │ SP      │ SL     │ Thành tiền   │
├───────┼─────────┼────────┼──────────────┤
│ 1     │iPhone 15│ 1      │ 29,990,000 ₫ │
│ 2     │AirPods  │ 2      │  6,000,000 ₫ │
├───────┴─────────┴────────┼──────────────┤
│                Tạm tính  │ 35,990,000 ₫ │
│           Giảm giá (10%) │ -3,599,000 ₫ │
│              TỔNG CỘNG   │ 32,391,000 ₫ │
└──────────────────────────┴──────────────┘
```

#### 6.3. NuGet Packages
- `QuestPDF` hoặc `iTextSharp` cho PDF
- Hoặc sử dụng built-in Windows Print API

---

## 7. Test Cases (0.5đ)

### Mục tiêu
Unit tests và UI tests cho các chức năng chính.

### Test Projects
```
Project_MyShop_2025.Tests/
├── Unit/
│   ├── Services/
│   │   ├── ProductServiceTests.cs
│   │   ├── OrderServiceTests.cs
│   │   ├── PromotionServiceTests.cs
│   │   └── CustomerServiceTests.cs
│   └── Helpers/
│       └── PasswordHelperTests.cs
└── Integration/
    ├── DatabaseTests.cs
    └── PromotionCalculationTests.cs
```

### Test Framework
- xUnit hoặc MSTest
- Moq cho mocking
- InMemory SQLite cho DB tests

### Example Tests
```csharp
public class PromotionServiceTests
{
    [Fact]
    public async Task ValidateCode_ExpiredPromotion_ReturnsNull()
    {
        // Arrange
        var promotion = new Promotion { EndDate = DateTime.Now.AddDays(-1) };
        // ...
        
        // Act
        var result = await _service.ValidateCodeAsync("EXPIRED");
        
        // Assert
        Assert.Null(result);
    }
    
    [Theory]
    [InlineData(100000, 10, 10000)]  // 10% of 100k = 10k
    [InlineData(50000, 20, 10000)]   // 20% of 50k = 10k
    public async Task CalculateDiscount_PercentageType_ReturnsCorrectAmount(
        int orderAmount, int discountPercent, int expectedDiscount)
    {
        // ...
    }
}
```

---

## 8. Backup/Restore Database (0.25đ)

### Mục tiêu
Backup và restore SQLite database.

### Implementation

#### 8.1. DatabaseService
```csharp
public interface IDatabaseService
{
    Task BackupAsync(string backupPath);
    Task RestoreAsync(string backupPath);
    string GetDatabasePath();
}

public class DatabaseService : IDatabaseService
{
    public async Task BackupAsync(string backupPath)
    {
        var dbPath = DatabasePathHelper.GetDatabasePath();
        File.Copy(dbPath, backupPath, overwrite: true);
    }
    
    public async Task RestoreAsync(string backupPath)
    {
        var dbPath = DatabasePathHelper.GetDatabasePath();
        // Close all connections first
        // Copy backup to db path
    }
}
```

#### 8.2. UI in ConfigPage
- Backup button → FileSavePicker
- Restore button → FileOpenPicker
- Auto backup schedule option

---

## 9. Auto Save (0.25đ)

### Mục tiêu
Tự động lưu draft khi tạo đơn hàng hoặc thêm sản phẩm.

### Implementation

#### 9.1. Auto-save service
```csharp
public interface IAutoSaveService
{
    Task SaveDraftAsync<T>(string key, T data);
    Task<T?> LoadDraftAsync<T>(string key);
    Task ClearDraftAsync(string key);
}
```

#### 9.2. Storage
- Sử dụng LocalSettings cho dữ liệu nhỏ
- Sử dụng LocalFolder files cho dữ liệu lớn

#### 9.3. Trigger
- Debounce 2-3 giây sau khi user ngừng nhập
- Save khi user navigate away (unsaved warning)

---

## Thứ tự triển khai đề xuất

1. **Phase 1 - Foundation**: DI + MVVM (bắt buộc làm trước)
2. **Phase 2 - Core Features**: Customer, Promotion, Advanced Search
3. **Phase 3 - Utilities**: Print, Backup/Restore, Auto Save
4. **Phase 4 - Quality**: Test Cases

---

## Commit Convention

Mỗi feature cần ít nhất 2 commits:

```
feat: add Customer model and migration
update: complete CustomerService and CRUD operations

feat: add Promotion model with discount types
update: integrate promotion into order creation flow

chore: add unit tests for PromotionService
fix: correct discount calculation for percentage type
```
