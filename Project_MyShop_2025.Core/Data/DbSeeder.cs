using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_MyShop_2025.Core.Data
{
    public static class DbSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            // Kiểm tra xem đã có dữ liệu chưa
            if (context.Products.Any())
            {
                return; // DB đã có dữ liệu, không seed lại
            }

            // 1. TẠO CATEGORIES (DANH MỤC)
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics", Description = "Thiết bị điện tử công nghệ cao" },
                    new Category { Name = "Fashion & Apparel", Description = "Thời trang nam nữ và phụ kiện" },
                    new Category { Name = "Books & Stationery", Description = "Sách và dụng cụ văn phòng phẩm" },
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Lấy lại ID thực tế từ DB để gán cho sản phẩm (tránh lỗi lệch ID)
            var electronics = context.Categories.First(c => c.Name == "Electronics");
            var fashion = context.Categories.First(c => c.Name == "Fashion & Apparel");
            var books = context.Categories.First(c => c.Name == "Books & Stationery");

            // 2. TẠO PRODUCTS (SẢN PHẨM) - Dữ liệu cố định, ảnh hiển thị tên sản phẩm
            var products = new List<Product>();

            // --- ĐIỆN TỬ (High Value) ---
            products.AddRange(new[] {
                CreateProduct(electronics.Id, "iPhone 15 Pro Max", 29990000, 25000000, 50, "ELEC001", "Smartphone cao cấp nhất của Apple với khung titan."),
                CreateProduct(electronics.Id, "Samsung Galaxy S24 Ultra", 27990000, 23000000, 45, "ELEC002", "Điện thoại AI tiên tiến nhất với bút S-Pen."),
                CreateProduct(electronics.Id, "MacBook Pro 14 M3", 45990000, 40000000, 20, "ELEC003", "Laptop chuyên nghiệp cho lập trình viên và đồ họa."),
                CreateProduct(electronics.Id, "Sony WH-1000XM5", 7990000, 6500000, 100, "ELEC004", "Tai nghe chống ồn chủ động tốt nhất thị trường."),
                CreateProduct(electronics.Id, "iPad Pro 12.9 M2", 25990000, 22000000, 30, "ELEC005", "Máy tính bảng mạnh mẽ thay thế laptop."),
                CreateProduct(electronics.Id, "PlayStation 5 Slim", 14990000, 12000000, 15, "ELEC006", "Máy chơi game console thế hệ mới."),
                CreateProduct(electronics.Id, "Dell XPS 15 9530", 35990000, 30000000, 10, "ELEC007", "Laptop Windows cao cấp màn hình OLED."),
                CreateProduct(electronics.Id, "Apple Watch Series 9", 9990000, 8000000, 60, "ELEC008", "Đồng hồ thông minh theo dõi sức khỏe.")
            });

            // --- THỜI TRANG (Medium Value) ---
            products.AddRange(new[] {
                CreateProduct(fashion.Id, "Nike Air Force 1", 2690000, 2000000, 100, "FASH001", "Giày sneaker trắng huyền thoại."),
                CreateProduct(fashion.Id, "Adidas Ultraboost Light", 3990000, 3000000, 80, "FASH002", "Giày chạy bộ êm ái nhất."),
                CreateProduct(fashion.Id, "Levis 501 Original Jeans", 1790000, 1200000, 120, "FASH003", "Quần jean ống đứng cổ điển."),
                CreateProduct(fashion.Id, "Uniqlo Áo Khoác Phao", 1290000, 900000, 200, "FASH004", "Áo khoác siêu nhẹ giữ ấm."),
                CreateProduct(fashion.Id, "Ray-Ban Aviator", 3990000, 3000000, 50, "FASH005", "Kính mát phi công chống tia UV."),
                CreateProduct(fashion.Id, "Zara Blazer Nam", 2490000, 1800000, 40, "FASH006", "Áo vest thời trang công sở.")
            });

            // --- SÁCH (Low Value, High Volume) ---
            products.AddRange(new[] {
                CreateProduct(books.Id, "Clean Code", 549000, 400000, 50, "BOOK001", "Sách gối đầu giường cho mọi lập trình viên."),
                CreateProduct(books.Id, "Design Patterns (GoF)", 689000, 500000, 40, "BOOK002", "Các mẫu thiết kế phần mềm kinh điển."),
                CreateProduct(books.Id, "The Pragmatic Programmer", 599000, 450000, 45, "BOOK003", "Hành trình từ thợ code thành nghệ nhân."),
                CreateProduct(books.Id, "Sổ tay Moleskine", 450000, 300000, 200, "STAT001", "Sổ tay bìa cứng cao cấp."),
                CreateProduct(books.Id, "Bút Lamy Safari", 890000, 600000, 100, "STAT002", "Bút máy thiết kế công thái học.")
            });

            context.Products.AddRange(products);
            context.SaveChanges();

            // 3. TẠO ẢNH SẢN PHẨM (Mỗi SP có 1 ảnh chính + 2 ảnh phụ)
            var productImages = new List<ProductImage>();
            foreach (var p in products)
            {
                // Ảnh 1: Ảnh chính
                productImages.Add(new ProductImage { ProductId = p.Id, ImagePath = GetProductImageUrl(p.Name, "Main"), DisplayOrder = 1 });
                // Ảnh 2: Góc nghiêng
                productImages.Add(new ProductImage { ProductId = p.Id, ImagePath = GetProductImageUrl(p.Name, "Side"), DisplayOrder = 2 });
                // Ảnh 3: Chi tiết
                productImages.Add(new ProductImage { ProductId = p.Id, ImagePath = GetProductImageUrl(p.Name, "Detail"), DisplayOrder = 3 });
                
                // Update ảnh đại diện cho product
                p.Image = GetProductImageUrl(p.Name, "Main");
            }
            context.ProductImages.AddRange(productImages);
            context.SaveChanges();

            // 4. TẠO ORDERS (ĐƠN HÀNG) - KỊCH BẢN THỰC TẾ
            // Tạo danh sách khách hàng cố định
            var customers = newListCustomer();
            
            // Lấy danh sách sản phẩm để dùng
            var iphone = products.First(p => p.SKU == "ELEC001");
            var macbook = products.First(p => p.SKU == "ELEC003");
            var sonyHeadphone = products.First(p => p.SKU == "ELEC004");
            var nikeShoe = products.First(p => p.SKU == "FASH001");
            var bookCleanCode = products.First(p => p.SKU == "BOOK001");
            var bookDesign = products.First(p => p.SKU == "BOOK002");

            var orders = new List<Order>();
            var today = DateTime.Now;

            // --- THÁNG TRƯỚC NỮA (Dữ liệu cũ - Đã hoàn tất) ---
            orders.Add(CreateOrder(customers[0], today.AddDays(-60), OrderStatus.Paid, new[] { (iphone, 1), (sonyHeadphone, 1) })); // Đơn to
            orders.Add(CreateOrder(customers[1], today.AddDays(-58), OrderStatus.Paid, new[] { (bookCleanCode, 1), (bookDesign, 1) }));
            orders.Add(CreateOrder(customers[2], today.AddDays(-55), OrderStatus.Cancelled, new[] { (macbook, 1) })); // Đơn hủy

            // --- THÁNG TRƯỚC (Doanh số tăng) ---
            orders.Add(CreateOrder(customers[3], today.AddDays(-30), OrderStatus.Paid, new[] { (macbook, 1), (iphone, 1) })); // VIP Customer
            orders.Add(CreateOrder(customers[4], today.AddDays(-28), OrderStatus.Paid, new[] { (nikeShoe, 2) }));
            orders.Add(CreateOrder(customers[0], today.AddDays(-25), OrderStatus.Paid, new[] { (bookCleanCode, 5) })); // Mua cho team
            orders.Add(CreateOrder(customers[1], today.AddDays(-20), OrderStatus.Paid, new[] { (sonyHeadphone, 1) }));

            // --- TUẦN NÀY (Dữ liệu mới - Hỗn hợp trạng thái) ---
            // Đơn hôm qua - Đã thanh toán
            orders.Add(CreateOrder(customers[2], today.AddDays(-1), OrderStatus.Paid, new[] { (iphone, 2) })); 
            // Đơn hôm nay - Mới tạo (Chưa thanh toán)
            orders.Add(CreateOrder(customers[3], today, OrderStatus.Created, new[] { (macbook, 1), (bookDesign, 1) }));
            // Đơn hôm nay - Đã thanh toán
            orders.Add(CreateOrder(customers[5], today.AddHours(-2), OrderStatus.Paid, new[] { (nikeShoe, 1), (sonyHeadphone, 1) }));
            // Đơn hôm nay - Vừa hủy
            orders.Add(CreateOrder(customers[4], today.AddHours(-1), OrderStatus.Cancelled, new[] { (iphone, 1) }));

            context.Orders.AddRange(orders);
            context.SaveChanges();
            
            // Cập nhật lại tồn kho cho các sản phẩm bán chạy (để test cảnh báo hết hàng)
            iphone.Quantity = 5; 
            macbook.Quantity = 2; // Sắp hết hàng
            
            context.SaveChanges();
        }

        // --- CÁC HÀM HELPER ---

        private static Product CreateProduct(int catId, string name, int price, int importPrice, int qty, string sku, string desc)
        {
            return new Product
            {
                Name = name,
                Price = price,
                ImportPrice = importPrice,
                Quantity = qty,
                CategoryId = catId,
                SKU = sku,
                Description = desc,
                Image = "" // Sẽ update sau
            };
        }

        private static string GetProductImageUrl(string productName, string type)
        {
            // Sử dụng dịch vụ placehold.co để tạo ảnh có chứa TÊN SẢN PHẨM
            // Encode tên sản phẩm để an toàn trên URL
            var encodedName = Uri.EscapeDataString($"{productName} - {type}");
            // Màu nền ngẫu nhiên dựa trên tên (để không bị đơn điệu nhưng vẫn cố định)
            var color = Math.Abs(productName.GetHashCode()).ToString("X").Substring(0, 6);
            return $"https://placehold.co/600x400/{color}/FFF?text={encodedName}";
        }

        private static Order CreateOrder(CustomerInfo customer, DateTime date, OrderStatus status, (Product prod, int qty)[] items)
        {
            var order = new Order
            {
                CustomerName = customer.Name,
                CustomerPhone = customer.Phone,
                CustomerAddress = customer.Address,
                CreatedAt = date,
                Status = status,
                Items = new List<OrderItem>()
            };

            int total = 0;
            foreach (var item in items)
            {
                int lineTotal = item.prod.Price * item.qty;
                total += lineTotal;
                order.Items.Add(new OrderItem
                {
                    ProductId = item.prod.Id,
                    Quantity = item.qty,
                    Price = item.prod.Price,
                    TotalPrice = lineTotal
                });
            }
            order.TotalPrice = total;
            return order;
        }

        private static List<CustomerInfo> newListCustomer()
        {
            return new List<CustomerInfo>
            {
                new CustomerInfo { Name = "Nguyễn Văn An", Phone = "0901234567", Address = "123 Lê Lợi, Q.1, TP.HCM" },
                new CustomerInfo { Name = "Trần Thị Bích", Phone = "0909888777", Address = "45 Nguyễn Huệ, Q.1, TP.HCM" },
                new CustomerInfo { Name = "Lê Hoàng Cường", Phone = "0912345678", Address = "100 Điện Biên Phủ, Bình Thạnh, TP.HCM" },
                new CustomerInfo { Name = "Phạm Minh Duy", Phone = "0987654321", Address = "Tòa nhà Bitexco, Q.1, TP.HCM" },
                new CustomerInfo { Name = "Đặng Thu Thảo", Phone = "0933444555", Address = "Khu đô thị Sala, Q.2, TP.HCM" },
                new CustomerInfo { Name = "Vũ Quang Huy", Phone = "0977888999", Address = "Landmark 81, Bình Thạnh, TP.HCM" }
            };
        }

        private class CustomerInfo
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
        }
    }
}