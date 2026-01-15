# MyShop 2025 - Models Documentation

## 1. Category (Danh mục sản phẩm)

**File:** `Project_MyShop_2025.Core/Models/Category.cs`

```csharp
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

### Mô tả
- Đại diện cho một danh mục/loại sản phẩm
- Mối quan hệ: 1 Category → N Products

### Dữ liệu mẫu (Seed)
1. Electronics - Thiết bị điện tử công nghệ cao
2. Fashion & Apparel - Thời trang nam nữ và phụ kiện
3. Books & Stationery - Sách và dụng cụ văn phòng phẩm

---

## 2. Product (Sản phẩm)

**File:** `Project_MyShop_2025.Core/Models/Product.cs`

```csharp
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? SKU { get; set; }  // Stock Keeping Unit

    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int Price { get; set; }  // Giá bán (VND)

    public int ImportPrice { get; set; }  // Giá nhập

    public string? Image { get; set; }  // Ảnh đại diện (URL hoặc file path)

    public int Quantity { get; set; }  // Số lượng tồn kho

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    // Navigation property for multiple images
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
```

### Mô tả
- Đại diện cho một sản phẩm trong hệ thống
- Mối quan hệ: N Products ← 1 Category
- Mối quan hệ: 1 Product → N ProductImages

### Tính năng đặc biệt
- `Quantity` hiển thị badge cảnh báo khi < 5 (Low Stock)
- `Image` có thể là URL placeholder hoặc file:// path

---

## 3. ProductImage (Ảnh sản phẩm)

**File:** `Project_MyShop_2025.Core/Models/ProductImage.cs`

```csharp
public class ProductImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }  // Thứ tự hiển thị

    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}
```

### Mô tả
- Mỗi sản phẩm có thể có nhiều ảnh
- `DisplayOrder` = 1: Ảnh chính
- Seed data tạo 3 ảnh cho mỗi sản phẩm: Main, Side, Detail

---

## 4. Order (Đơn hàng)

**File:** `Project_MyShop_2025.Core/Models/Order.cs`

```csharp
public enum OrderStatus
{
    Created = 0,    // Mới tạo
    Paid = 1,       // Đã thanh toán
    Cancelled = 2   // Đã hủy
}

public class Order
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int TotalPrice { get; set; }  // Tổng tiền (VND)

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    // Thông tin khách hàng (inline, chưa có Customer entity riêng)
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
```

### Mô tả
- Đại diện cho một đơn hàng
- Thông tin khách hàng được lưu trực tiếp (chưa có Customer entity)
- Trạng thái: Created → Paid hoặc Created → Cancelled

### State Diagram
```
[*] → Created
Created → Paid
Created → Cancelled
Paid → [*]
Cancelled → [*]
```

---

## 5. OrderItem (Chi tiết đơn hàng)

**File:** `Project_MyShop_2025.Core/Models/OrderItem.cs`

```csharp
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    public int Price { get; set; }  // Đơn giá tại thời điểm mua

    public int TotalPrice { get; set; }  // = Quantity * Price
}
```

### Mô tả
- Một dòng item trong đơn hàng
- `Price` lưu đơn giá tại thời điểm mua (không thay đổi khi Product.Price thay đổi)

---

## 6. User (Người dùng)

**File:** `Project_MyShop_2025.Core/Models/User.cs`

```csharp
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;  // Store Hashed Password

    public string? FullName { get; set; }
    
    public string? Email { get; set; }
    
    public string Role { get; set; } = "User";  // Admin, User
}
```

### Mô tả
- Tài khoản người dùng
- Password được hash bằng SHA256 trước khi lưu
- Role hiện chỉ có "Admin" và "User" (chưa có phân quyền chi tiết)

### Default Admin Account (Seed)
- Username: `admin`
- Password: `admin123` (hashed)

---

## Tổng quan quan hệ (Relationships)

```
Category (1) ─── (N) Product (1) ─── (N) ProductImage
                      │
                      │ (N)
                      │
Order (1) ─────── (N) OrderItem

User (standalone)
```

## Ghi chú quan trọng

1. **Tiền tệ**: Sử dụng `int` cho giá (max ~4 tỷ VND), phù hợp với thực tế VN
2. **Chưa có Customer entity riêng**: Thông tin KH lưu inline trong Order
3. **Chưa có Promotion/Discount**: Cần bổ sung cho phần C
4. **Image path**: Hỗ trợ cả URL và local file path (file:///)
