# MyShop 2025 - Views Documentation

## Tổng quan
Project sử dụng pattern **Code-Behind** (không phải MVVM thuần túy). Logic được đặt trực tiếp trong file `.xaml.cs`.

---

## 1. LoginPage

**Files:** `Views/LoginPage.xaml`, `Views/LoginPage.xaml.cs`

### Chức năng
- Đăng nhập với username/password
- Checkbox "Remember Me" để auto-login lần sau
- Đăng ký tài khoản mới
- Hiển thị version từ Package manifest
- Navigate đến ConfigPage để cấu hình server

### Luồng xử lý
```
1. Page Load → CheckRememberedUser()
   - Nếu có saved username → auto navigate đến ShellPage
   
2. Login Click:
   - Validate input
   - Query User từ DB
   - Verify password hash
   - Lưu username nếu RememberMe checked
   - Navigate đến ShellPage

3. Signup Click:
   - Show ContentDialog với form
   - Validate & tạo User mới
   - Hash password trước khi lưu
```

### Settings sử dụng
- `RememberMe_Username`: Username đã lưu

---

## 2. ShellPage (Navigation Host)

**Files:** `Views/ShellPage.xaml`, `Views/ShellPage.xaml.cs`

### Chức năng
- Container chính sau khi đăng nhập
- NavigationView với sidebar menu
- Điều hướng giữa các trang chức năng
- Hiển thị thông tin user (avatar, name)
- Logout button

### Navigation Items
| Tag | Page | Icon |
|-----|------|------|
| Dashboard | DashboardPage | Home |
| Products | ProductsPage | AllApps |
| Orders | OrdersPage | Shop |
| Reports | ReportsPage | ReportDocument |
| Chatbot | ChatbotPage | Chat |
| Settings | ConfigPage | Settings |

### Settings sử dụng
- `LastPage`: Trang cuối truy cập (để restore)
- `RememberLastPage`: Toggle ghi nhớ trang

---

## 3. DashboardPage

**Files:** `Views/DashboardPage.xaml`, `Views/DashboardPage.xaml.cs`

### Layout
```
┌─────────────────────────────────────────────────┐
│ Header: "Good morning, Admin!"                  │
├────────┬────────┬────────┬────────┬────────┬────┤
│Products│ Today  │ Today  │ Orders │ Best   │Low │
│ Count  │Orders  │Revenue │ Status │Sellers │Stk │
├────────┴────────┴────────┴────────┴────────┴────┤
│ Revenue Chart (Line/Bar by date range)          │
├─────────────────────────────────────────────────┤
│ Recent Orders List                              │
└─────────────────────────────────────────────────┘
```

### Metrics Cards
1. **Total Products**: Tổng SP + trend indicator
2. **Today Orders**: Đơn hàng hôm nay + trend
3. **Today Revenue**: Doanh thu hôm nay + trend
4. **Orders by Status**: Pie chart Created/Paid/Cancelled
5. **Best Sellers**: Top 5 SP bán chạy
6. **Low Stock Alert**: Top 5 SP sắp hết (qty < 5)

### Chart Features
- Date range selector: Today, This Week, This Month, This Year
- Vẽ chart thủ công bằng XAML (không dùng thư viện)
- Sparkline mini-charts trên mỗi card

---

## 4. ProductsPage

**Files:** `Views/ProductsPage.xaml`, `Views/ProductsPage.xaml.cs`

### Layout
```
┌─────────────────────────────────────────────────┐
│ Header: "Product Management" [n products]       │
│                    [Import] [Category] [+Add]   │
├─────────────────────────────────────────────────┤
│ [Search box] [Category▼] [Price Range] [Sort▼]  │
│ Category Pills: [All] [Electronics] [Fashion]...│
├─────────────────────────────────────────────────┤
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐   │
│ │ Img  │ │ Img  │ │ Img  │ │ Img  │ │ Img  │   │
│ │Name  │ │Name  │ │Name  │ │Name  │ │Name  │   │
│ │Price │ │Price │ │Price │ │Price │ │Price │   │
│ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘   │
├─────────────────────────────────────────────────┤
│ [← Prev] Page 1 of 5 | Show: [20 items▼] [Next→]│
└─────────────────────────────────────────────────┘
```

### Chức năng
- **Search**: Tìm theo tên hoặc SKU
- **Filter by Category**: ComboBox + Pills UI
- **Filter by Price**: Min/Max range
- **Sort**: Name (A-Z, Z-A), Price, Stock
- **Pagination**: Số items/trang có thể thay đổi
- **CRUD**: Add/Edit/Delete product với ContentDialog
- **Import**: From Excel (.xlsx) hoặc Access (.accdb)

### Helper Classes
```csharp
class CategoryFilterItem { Name, Id, Count }
class ProductDisplayModel : INotifyPropertyChanged
{
    Id, Name, SKU, PriceFormatted, Quantity,
    ImageSource, StockBadgeText, StockBadgeBackground...
}
```

---

## 5. OrdersPage

**Files:** `Views/OrdersPage.xaml`, `Views/OrdersPage.xaml.cs`

### Layout
```
┌─────────────────────────────────────────────────┐
│ Header: "Order Management" [n orders]           │
│                                    [+Create]    │
├─────────────────────────────────────────────────┤
│ [Search] [Date Range▼] [Sort▼]        [Export]  │
│ Status Pills: [All] [Created] [Paid] [Cancelled]│
├─────────────────────────────────────────────────┤
│ ┌───────────────────────────────────────────┐   │
│ │ #123 │ Customer Name    │ Paid │ $1,234  │   │
│ │ ORD  │ Date • 3 items   │      │ [👁][✎]│   │
│ └───────────────────────────────────────────┘   │
│ ┌───────────────────────────────────────────┐   │
│ │ #124 │ Customer Name    │Created│ $567  │   │
│ └───────────────────────────────────────────┘   │
├─────────────────────────────────────────────────┤
│ [← Prev] Page 1 of 3 | Show: [20 orders▼] [→]   │
└─────────────────────────────────────────────────┘
```

### Chức năng
- **Search**: Theo customer name, order ID
- **Filter by Date**: From/To date picker
- **Filter by Status**: Pills UI
- **Sort**: Date (Newest/Oldest), Amount (High/Low)
- **View Details**: Popup với danh sách items
- **Update Status**: Thay đổi trạng thái đơn
- **Edit Order**: Sửa thông tin khách hàng
- **Delete Order**: Xóa đơn (confirm dialog)
- **Create Order**: Wizard chọn sản phẩm + số lượng

### Helper Classes
```csharp
class StatusFilterItem { Name, Status, Count, IsSelected }
class OrderDisplayModel
{
    OrderId, CustomerName, CreatedAt, ItemsCount,
    ItemsSummary, StatusText, StatusBackground,
    TotalPriceFormatted...
}
```

---

## 6. ReportsPage

**Files:** `Views/ReportsPage.xaml`, `Views/ReportsPage.xaml.cs`

### Layout
```
┌─────────────────────────────────────────────────┐
│ Header: "Reports & Analytics"                   │
│ Quick: [Today] [This Week] [This Month] [Year] │
│ Custom: [From: ___] [To: ___]  Period: [Day▼]  │
├────────┬────────┬────────┬─────────────────────┤
│Revenue │ Profit │ Orders │  Top Products        │
│₫XXXM  │ ₫XXXM  │  XXX   │  1. Product A        │
│        │        │        │  2. Product B        │
├────────┴────────┴────────┴─────────────────────┤
│ Product Sales Chart (Line)                      │
├─────────────────────────────────────────────────┤
│ Revenue & Profit Chart (Bar)                    │
└─────────────────────────────────────────────────┘
```

### Chức năng
- **Quick Period Buttons**: Today, This Week, This Month, This Year
- **Custom Date Range**: DatePicker cho From/To
- **Period Grouping**: Day, Week, Month, Year
- **KPI Cards**: Revenue, Profit, Order count
- **Top Products**: Danh sách SP bán chạy nhất trong kỳ
- **Orders by Status**: Pie chart Created/Paid/Cancelled
- **Product Sales Chart**: Line chart số lượng bán theo thời gian
- **Revenue/Profit Chart**: Bar chart so sánh Revenue vs Profit

### Chart Implementation
- Vẽ thủ công bằng XAML Canvas
- Không sử dụng thư viện chart bên ngoài

---

## 7. ConfigPage (Settings)

**Files:** `Views/ConfigPage.xaml`, `Views/ConfigPage.xaml.cs`

### Settings Categories

#### Display Settings
- Products per page: 5/10/15/20
- Orders per page: 5/10/15/20
- Remember last page: Toggle

#### Database Settings
- Server address
- Port
- Database name
- Username
- Password
- Test connection button

#### AI Settings (Chatbot)
- Gemini API Key

### Settings Storage
Sử dụng `ApplicationData.Current.LocalSettings`

---

## 8. ChatbotPage

**Files:** `Views/ChatbotPage.xaml`, `Views/ChatbotPage.xaml.cs`

### Chức năng
- Chat interface với AI assistant
- Sử dụng Gemini API
- Hỗ trợ hỏi đáp về sản phẩm, đơn hàng

---

## Common UI Patterns

### Color Palette
```
Primary Blue: #3B82F6
Purple: #8B5CF6
Green (Success): #22C55E / #10B981
Yellow (Warning): #F59E0B
Red (Danger): #EF4444
Background: #F8FAFC
Card Background: #FFFFFF
Border: #E2E8F0
Text Primary: #0F172A
Text Secondary: #64748B
Text Muted: #94A3B8
```

### Components
- **Pills/Badges**: Border with CornerRadius="16"
- **Cards**: Border with CornerRadius="12", Shadow
- **Buttons**: CornerRadius="8", Padding="16,10"
- **Icons**: FontIcon from Segoe MDL2 Assets

### Hover Effects
```csharp
void Card_PointerEntered(object sender, ...) {
    var border = sender as Border;
    border.BorderBrush = GetColorFromHex("#3B82F6");
}
void Card_PointerExited(object sender, ...) {
    var border = sender as Border;
    border.BorderBrush = GetColorFromHex("#E2E8F0");
}
```
