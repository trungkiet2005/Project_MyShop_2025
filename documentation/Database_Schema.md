# Database Design & Schema

## 1. Công nghệ Database
*   **Database Engine**: SQLite.
*   **Access Technology**: Entity Framework Core 9.0 (Code-First Approach).
*   **File Location**: `Shop.db` (Nằm trong thư mục local của ứng dụng).

## 2. Các bảng chính (Entities)

### Table: Products
Chứa thông tin sản phẩm (Master Data).
*   `Id` (INTEGER PK): Khóa chính tự tăng.
*   `Name` (TEXT): Tên sản phẩm.
*   `SKU` (TEXT): Mã vạch/Mã kiểm kê (Unique).
*   `Price` (INTEGER): Giá bán (VND).
*   `ImportPrice` (INTEGER): Giá nhập (để tính lãi lỗ).
*   `Quantity` (INTEGER): Số lượng tồn kho.
*   `CategoryId` (INTEGER FK): Khóa ngoại trỏ về `Categories`.

### Table: Categories
Chứa loại sản phẩm.
*   `Id` (INTEGER PK).
*   `Name` (TEXT).
*   `Description` (TEXT).

### Table: Orders
Chứa thông tin đơn hàng (Transaction Data).
*   `Id` (INTEGER PK).
*   `CreatedAt` (TEXT/DateTime): Ngày tạo đơn.
*   `Status` (INTEGER): Trạng thái (0=Created, 1=Paid, 2=Cancelled).
*   `CustomerName`, `CustomerPhone`, `CustomerAddress`: Thông tin khách mua lẻ.
*   `CustomerId` (INTEGER FK): Khóa ngoại trỏ về `Customers` (nếu có quản lý khách).
*   `TotalPrice` (INTEGER): Tổng tiền đơn hàng.

### Table: OrderItems
Chi tiết đơn hàng (lưu từng món trong đơn).
*   `Id` (INTEGER PK).
*   `OrderId` (INTEGER FK): Thuộc về đơn nào.
*   `ProductId` (INTEGER FK): Sản phẩm nào.
*   `Quantity` (INTEGER): Số lượng mua.
*   `Price` (INTEGER): Giá bán tại thời điểm mua (Snapshot price).
*   `TotalPrice` (INTEGER): Thành tiền (`Quantity * Price`).

### Table: Customers
Quản lý khách hàng thân thiết (Optional Feature C5).
*   `Id` (INTEGER PK).
*   `Name`, `Phone`, `Address`.
*   `LoyaltyPoints`: Điểm tích lũy.

### Table: Promotions
Quản lý mã giảm giá (Optional Feature C2).
*   `Id` (INTEGER PK).
*   `Code` (TEXT): Mã giảm giá (VD: SALE50).
*   `DiscountPercent` (FLOAT).
*   `StartDate`, `EndDate`: Thời hạn.

## 3. Mối quan hệ (Relationships)
*   **One-to-Many**: `Category` có nhiều `Products`.
*   **One-to-Many**: `Order` có nhiều `OrderItems`.
*   **Many-to-One**: `OrderItem` thuộc về 1 `Product`.
*   **Many-to-One**: `Order` thuộc về 1 `Customer` (Nullable).

## 4. Tại sao lại thiết kế như vậy?
*   **Tách bảng Order và OrderItems**: Để tuân thủ chuẩn hóa CSDL (1NF, 2NF), tránh lặp lại thông tin header đơn hàng.
*   **Lưu Price trong OrderItems**: Rất quan trọng. Giá sản phẩm trong tương lai có thể đổi, nhưng giá trong đơn hàng cũ phải giữ nguyên lịch sử.
*   **Dùng Integer cho tiền tệ**: Yêu cầu đề bài, đơn giản hóa và đủ lớn cho tiền Việt (lên đến 2 tỷ, dùng long nếu cần lớn hơn).
