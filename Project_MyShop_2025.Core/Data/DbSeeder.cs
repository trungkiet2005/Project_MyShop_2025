using Project_MyShop_2025.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace Project_MyShop_2025.Core.Data
{
    public static class DbSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            try
            {
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
                    System.Diagnostics.Debug.WriteLine($"Created {categories.Count} categories");
                }

            // Lấy lại ID thực tế từ DB để gán cho sản phẩm (tránh lỗi lệch ID)
            var electronics = context.Categories.FirstOrDefault(c => c.Name == "Electronics");
            var fashion = context.Categories.FirstOrDefault(c => c.Name == "Fashion & Apparel");
            var books = context.Categories.FirstOrDefault(c => c.Name == "Books & Stationery");

            if (electronics == null || fashion == null || books == null)
            {
                return; // Không có categories, không thể seed
            }

            // 2. TẠO PRODUCTS (SẢN PHẨM) - Chỉ seed nếu chưa có
            // Để force reseed, uncomment các dòng dưới:
            // if (context.ProductImages.Any())
            // {
            //     System.Diagnostics.Debug.WriteLine("Removing old product images to reseed with new image paths...");
            //     context.ProductImages.RemoveRange(context.ProductImages);
            // }
            // if (context.Products.Any())
            // {
            //     System.Diagnostics.Debug.WriteLine("Removing old products to reseed with new image paths...");
            //     context.Products.RemoveRange(context.Products);
            //     context.SaveChanges();
            // }
            
            if (!context.Products.Any())
            {
                var products = new List<Product>();

                // --- ĐIỆN TỬ (High Value) - 22 sản phẩm ---
                products.AddRange(new[] {
                    CreateProduct(electronics.Id, "iPhone 15 Pro Max", 29990000, 25000000, 50, "ELEC001", "Smartphone cao cấp nhất của Apple với khung titan."),
                    CreateProduct(electronics.Id, "Samsung Galaxy S24 Ultra", 27990000, 23000000, 45, "ELEC002", "Điện thoại AI tiên tiến nhất với bút S-Pen."),
                    CreateProduct(electronics.Id, "MacBook Pro 14 M3", 45990000, 40000000, 20, "ELEC003", "Laptop chuyên nghiệp cho lập trình viên và đồ họa."),
                    CreateProduct(electronics.Id, "Sony WH-1000XM5", 7990000, 6500000, 100, "ELEC004", "Tai nghe chống ồn chủ động tốt nhất thị trường."),
                    CreateProduct(electronics.Id, "iPad Pro 12.9 M2", 25990000, 22000000, 30, "ELEC005", "Máy tính bảng mạnh mẽ thay thế laptop."),
                    CreateProduct(electronics.Id, "PlayStation 5 Slim", 14990000, 12000000, 15, "ELEC006", "Máy chơi game console thế hệ mới."),
                    CreateProduct(electronics.Id, "Dell XPS 15 9530", 35990000, 30000000, 10, "ELEC007", "Laptop Windows cao cấp màn hình OLED."),
                    CreateProduct(electronics.Id, "Apple Watch Series 9", 9990000, 8000000, 60, "ELEC008", "Đồng hồ thông minh theo dõi sức khỏe."),
                    // Thêm 14 sản phẩm điện tử mới
                    CreateProduct(electronics.Id, "AirPods Pro 2", 6990000, 5500000, 80, "ELEC009", "Tai nghe true wireless với ANC."),
                    CreateProduct(electronics.Id, "Nintendo Switch OLED", 8990000, 7000000, 40, "ELEC010", "Máy chơi game cầm tay đa năng."),
                    CreateProduct(electronics.Id, "Canon EOS R6 Mark II", 52990000, 45000000, 12, "ELEC011", "Máy ảnh mirrorless full-frame chuyên nghiệp."),
                    CreateProduct(electronics.Id, "LG OLED TV 55 inch", 32990000, 28000000, 8, "ELEC012", "TV OLED 4K màu sắc hoàn hảo."),
                    CreateProduct(electronics.Id, "Xiaomi Robot Vacuum", 8990000, 7000000, 25, "ELEC013", "Robot hút bụi thông minh lập bản đồ."),
                    CreateProduct(electronics.Id, "GoPro Hero 12", 12990000, 10000000, 35, "ELEC014", "Camera hành trình chống nước 5K."),
                    CreateProduct(electronics.Id, "Bose SoundLink Flex", 3290000, 2500000, 55, "ELEC015", "Loa bluetooth di động chống nước."),
                    CreateProduct(electronics.Id, "Logitech MX Master 3S", 2490000, 1800000, 70, "ELEC016", "Chuột không dây cao cấp cho văn phòng."),
                    CreateProduct(electronics.Id, "Samsung Galaxy Tab S9", 19990000, 16000000, 22, "ELEC017", "Máy tính bảng Android cao cấp."),
                    CreateProduct(electronics.Id, "DJI Mini 3 Pro", 21990000, 18000000, 15, "ELEC018", "Flycam nhỏ gọn quay 4K."),
                    CreateProduct(electronics.Id, "Kindle Paperwhite", 3990000, 3000000, 45, "ELEC019", "Máy đọc sách điện tử chống nước."),
                    CreateProduct(electronics.Id, "ASUS ROG Phone 8 Pro", 24990000, 20000000, 18, "ELEC020", "Gaming phone hiệu năng khủng."),
                    CreateProduct(electronics.Id, "Anker PowerCore 26800", 1290000, 900000, 150, "ELEC021", "Sạc dự phòng dung lượng cao."),
                    CreateProduct(electronics.Id, "Apple HomePod Mini", 2490000, 1800000, 65, "ELEC022", "Loa thông minh tích hợp Siri.")
                });

                // --- THỜI TRANG (Medium Value) - 22 sản phẩm ---
                products.AddRange(new[] {
                    CreateProduct(fashion.Id, "Nike Air Force 1", 2690000, 2000000, 100, "FASH001", "Giày sneaker trắng huyền thoại."),
                    CreateProduct(fashion.Id, "Adidas Ultraboost Light", 3990000, 3000000, 80, "FASH002", "Giày chạy bộ êm ái nhất."),
                    CreateProduct(fashion.Id, "Levis 501 Original Jeans", 1790000, 1200000, 120, "FASH003", "Quần jean ống đứng cổ điển."),
                    CreateProduct(fashion.Id, "Uniqlo Áo Khoác Phao", 1290000, 900000, 200, "FASH004", "Áo khoác siêu nhẹ giữ ấm."),
                    CreateProduct(fashion.Id, "Ray-Ban Aviator", 3990000, 3000000, 50, "FASH005", "Kính mát phi công chống tia UV."),
                    CreateProduct(fashion.Id, "Zara Blazer Nam", 2490000, 1800000, 40, "FASH006", "Áo vest thời trang công sở."),
                    // Thêm 16 sản phẩm thời trang mới
                    CreateProduct(fashion.Id, "Converse Chuck Taylor", 1590000, 1100000, 90, "FASH007", "Giày canvas cổ điển mọi thời đại."),
                    CreateProduct(fashion.Id, "H&M Áo Polo Cơ Bản", 399000, 250000, 250, "FASH008", "Áo polo cotton thoáng mát."),
                    CreateProduct(fashion.Id, "Gucci Belt Logo", 12990000, 10000000, 15, "FASH009", "Thắt lưng da cao cấp logo GG."),
                    CreateProduct(fashion.Id, "Tommy Hilfiger Jacket", 4990000, 3800000, 45, "FASH010", "Áo khoác bomber phong cách Mỹ."),
                    CreateProduct(fashion.Id, "Puma RS-X Sneakers", 2890000, 2200000, 60, "FASH011", "Giày thể thao chunky retro."),
                    CreateProduct(fashion.Id, "Calvin Klein Underwear 3-Pack", 890000, 600000, 180, "FASH012", "Bộ 3 quần lót cotton cao cấp."),
                    CreateProduct(fashion.Id, "Fossil Smartwatch Gen 6", 6990000, 5500000, 35, "FASH013", "Đồng hồ thông minh thời trang."),
                    CreateProduct(fashion.Id, "Michael Kors Túi Xách", 8990000, 7000000, 25, "FASH014", "Túi xách da thời trang nữ."),
                    CreateProduct(fashion.Id, "New Balance 574", 2390000, 1800000, 75, "FASH015", "Giày sneaker lifestyle cổ điển."),
                    CreateProduct(fashion.Id, "Polo Ralph Lauren Shirt", 2790000, 2100000, 55, "FASH016", "Áo sơ mi Oxford nam."),
                    CreateProduct(fashion.Id, "Vans Old Skool", 1690000, 1200000, 95, "FASH017", "Giày skate board huyền thoại."),
                    CreateProduct(fashion.Id, "GAP Hoodie Classic", 1490000, 1000000, 110, "FASH018", "Áo hoodie nỉ thoải mái."),
                    CreateProduct(fashion.Id, "Lacoste Polo Classic", 2990000, 2200000, 65, "FASH019", "Áo polo logo cá sấu."),
                    CreateProduct(fashion.Id, "Casio G-Shock GA-2100", 3290000, 2500000, 48, "FASH020", "Đồng hồ thể thao chống sốc."),
                    CreateProduct(fashion.Id, "North Face Puffer Jacket", 5990000, 4500000, 30, "FASH021", "Áo phao lông vũ chống lạnh."),
                    CreateProduct(fashion.Id, "Timberland 6 Inch Boot", 4590000, 3500000, 42, "FASH022", "Giày boot da cao cổ cổ điển.")
                });

                // --- SÁCH & VĂN PHÒNG PHẨM (Low Value, High Volume) - 22 sản phẩm ---
                products.AddRange(new[] {
                    CreateProduct(books.Id, "Clean Code", 549000, 400000, 50, "BOOK001", "Sách gối đầu giường cho mọi lập trình viên."),
                    CreateProduct(books.Id, "Design Patterns (GoF)", 689000, 500000, 40, "BOOK002", "Các mẫu thiết kế phần mềm kinh điển."),
                    CreateProduct(books.Id, "The Pragmatic Programmer", 599000, 450000, 45, "BOOK003", "Hành trình từ thợ code thành nghệ nhân."),
                    CreateProduct(books.Id, "Sổ tay Moleskine", 450000, 300000, 200, "STAT001", "Sổ tay bìa cứng cao cấp."),
                    CreateProduct(books.Id, "Bút Lamy Safari", 890000, 600000, 100, "STAT002", "Bút máy thiết kế công thái học."),
                    // Thêm 17 sản phẩm sách & văn phòng phẩm mới
                    CreateProduct(books.Id, "Refactoring (Martin Fowler)", 649000, 480000, 38, "BOOK004", "Cải thiện thiết kế code hiện có."),
                    CreateProduct(books.Id, "Head First Design Patterns", 789000, 600000, 42, "BOOK005", "Học design patterns dễ hiểu."),
                    CreateProduct(books.Id, "Đắc Nhân Tâm", 108000, 70000, 300, "BOOK006", "Sách self-help bán chạy nhất."),
                    CreateProduct(books.Id, "Nhà Giả Kim", 79000, 50000, 280, "BOOK007", "Tiểu thuyết Paulo Coelho nổi tiếng."),
                    CreateProduct(books.Id, "Atomic Habits", 189000, 130000, 150, "BOOK008", "Thay đổi thói quen nhỏ, kết quả lớn."),
                    CreateProduct(books.Id, "Thinking Fast and Slow", 299000, 200000, 85, "BOOK009", "Tâm lý học về ra quyết định."),
                    CreateProduct(books.Id, "Rich Dad Poor Dad", 159000, 100000, 175, "BOOK010", "Tài chính cá nhân cơ bản."),
                    CreateProduct(books.Id, "System Design Interview", 890000, 700000, 28, "BOOK011", "Chuẩn bị phỏng vấn kỹ thuật."),
                    CreateProduct(books.Id, "Cracking Coding Interview", 950000, 750000, 32, "BOOK012", "189 câu hỏi phỏng vấn lập trình."),
                    CreateProduct(books.Id, "Bút Parker Jotter", 590000, 400000, 120, "STAT003", "Bút bi kim loại cao cấp."),
                    CreateProduct(books.Id, "Bút chì Staedtler 2B", 45000, 25000, 500, "STAT004", "Bút chì vẽ kỹ thuật Đức."),
                    CreateProduct(books.Id, "Sổ tay Leuchtturm1917", 520000, 380000, 90, "STAT005", "Sổ bullet journal chất lượng."),
                    CreateProduct(books.Id, "Mực Pilot Iroshizuku", 450000, 320000, 75, "STAT006", "Mực bút máy Nhật cao cấp."),
                    CreateProduct(books.Id, "Thước kẻ Faber-Castell", 35000, 20000, 400, "STAT007", "Thước nhựa trong suốt 30cm."),
                    CreateProduct(books.Id, "Gôm Pentel Hi-Polymer", 25000, 15000, 600, "STAT008", "Gôm mềm không để lại vết."),
                    CreateProduct(books.Id, "Bìa sổ còng A4", 89000, 55000, 250, "STAT009", "Bìa còng đựng tài liệu."),
                    CreateProduct(books.Id, "Bút highlight Stabilo", 65000, 40000, 350, "STAT010", "Bộ 6 màu đánh dấu nổi bật.")
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
                
                System.Diagnostics.Debug.WriteLine($"Seeded {products.Count} products with images");
            }
            else
            {
                // Nếu đã có products, cập nhật lại đường dẫn ảnh cho chúng
                System.Diagnostics.Debug.WriteLine("Products already exist, updating image paths...");
                var existingProducts = context.Products.Include(p => p.ProductImages).ToList();
                foreach (var product in existingProducts)
                {
                    var newImagePath = FindProductImageFile(product.Name);
                    if (!string.IsNullOrEmpty(newImagePath))
                    {
                        // Cập nhật ảnh chính của product
                        product.Image = newImagePath;
                        
                        // Cập nhật ProductImages nếu có
                        var mainImage = product.ProductImages.OrderBy(img => img.DisplayOrder).FirstOrDefault();
                        if (mainImage != null)
                        {
                            mainImage.ImagePath = newImagePath;
                        }
                        else
                        {
                            // Tạo ProductImage mới nếu chưa có
                            context.ProductImages.Add(new ProductImage 
                            { 
                                ProductId = product.Id, 
                                ImagePath = newImagePath, 
                                DisplayOrder = 1 
                            });
                        }
                    }
                }
                context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Updated image paths for {existingProducts.Count} existing products");
            }

            // 4. TẠO ORDERS (ĐƠN HÀNG) - Seed orders
            System.Diagnostics.Debug.WriteLine("About to call SeedOrders...");
            // Reload Products từ database để đảm bảo có đầy đủ thông tin
            context.ChangeTracker.Clear();
            
            // Kiểm tra xem có orders hợp lệ không (có items)
            var existingOrdersWithItems = context.Orders
                .Include(o => o.Items)
                .Where(o => o.Items.Any())
                .Count();
            
            // Nếu không có orders hợp lệ, force seed lại
            bool shouldForceSeed = existingOrdersWithItems == 0;
            System.Diagnostics.Debug.WriteLine($"Existing orders with items: {existingOrdersWithItems}, Force seed: {shouldForceSeed}");
            
            SeedOrders(context, force: shouldForceSeed);
            
            // 5. TẠO PROMOTIONS (KHUYẾN MÃI)
            SeedPromotions(context);
            
            // 6. TẠO USERS (NGƯỜI DÙNG)
            SeedUsers(context);
            
            // 7. TẠO CUSTOMERS (KHÁCH HÀNG)
            SeedCustomers(context);
            
            System.Diagnostics.Debug.WriteLine("Seed completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DbSeeder.Seed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw để caller biết có lỗi
            }
        }

        // Method riêng để seed Orders - có thể gọi độc lập
        // force: nếu true, sẽ xóa tất cả orders hiện có và seed lại
        public static void SeedOrders(ShopDbContext context, bool force = false)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SeedOrders: Starting...");
                
                // Kiểm tra có Products chưa - PHẢI KIỂM TRA TRƯỚC
                var productCount = context.Products.Count();
                System.Diagnostics.Debug.WriteLine($"SeedOrders: Found {productCount} products");
                if (productCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SeedOrders: No products found, cannot seed orders");
                    return; // Cần có Products trước
                }

                // Nếu force = true, xóa tất cả orders hiện có
                if (force)
                {
                    System.Diagnostics.Debug.WriteLine("SeedOrders: Force mode - removing all existing orders...");
                    var allOrders = context.Orders
                        .Include(o => o.Items)
                        .ToList();
                    context.Orders.RemoveRange(allOrders);
                    context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"SeedOrders: Removed {allOrders.Count} orders");
                }
                else
                {
                    // Chỉ seed nếu chưa có Orders HOẶC orders rỗng (không có items)
                    var existingOrdersCount = context.Orders.Count();
                    var ordersWithItems = context.Orders
                        .Include(o => o.Items)
                        .Where(o => o.Items.Any())
                        .Count();
                    
                    System.Diagnostics.Debug.WriteLine($"SeedOrders: Found {existingOrdersCount} orders, {ordersWithItems} with items");
                    
                    if (ordersWithItems > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("SeedOrders: Orders with items already exist, skipping");
                        return; // Đã có Orders hợp lệ rồi, không seed lại
                    }
                    
                    // Nếu có orders nhưng không có items, xóa chúng đi để seed lại
                    if (existingOrdersCount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("SeedOrders: Found empty orders, removing them...");
                        var emptyOrders = context.Orders
                            .Include(o => o.Items)
                            .Where(o => !o.Items.Any())
                            .ToList();
                        context.Orders.RemoveRange(emptyOrders);
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"SeedOrders: Removed {emptyOrders.Count} empty orders");
                    }
                }

                // Tạo danh sách khách hàng cố định
                var customers = newListCustomer();
                System.Diagnostics.Debug.WriteLine($"SeedOrders: Created {customers.Count} customers");
                
                // Lấy danh sách sản phẩm từ database
                var products = context.Products.ToList();
                var iphone = products.FirstOrDefault(p => p.SKU == "ELEC001");
                var samsung = products.FirstOrDefault(p => p.SKU == "ELEC002");
                var macbook = products.FirstOrDefault(p => p.SKU == "ELEC003");
                var sonyHeadphone = products.FirstOrDefault(p => p.SKU == "ELEC004");
                var ipad = products.FirstOrDefault(p => p.SKU == "ELEC005");
                var ps5 = products.FirstOrDefault(p => p.SKU == "ELEC006");
                var dellXps = products.FirstOrDefault(p => p.SKU == "ELEC007");
                var appleWatch = products.FirstOrDefault(p => p.SKU == "ELEC008");
                var nikeShoe = products.FirstOrDefault(p => p.SKU == "FASH001");
                var adidasShoe = products.FirstOrDefault(p => p.SKU == "FASH002");
                var jeans = products.FirstOrDefault(p => p.SKU == "FASH003");
                var jacket = products.FirstOrDefault(p => p.SKU == "FASH004");
                var sunglasses = products.FirstOrDefault(p => p.SKU == "FASH005");
                var blazer = products.FirstOrDefault(p => p.SKU == "FASH006");
                var bookCleanCode = products.FirstOrDefault(p => p.SKU == "BOOK001");
                var bookDesign = products.FirstOrDefault(p => p.SKU == "BOOK002");
                var bookPragmatic = products.FirstOrDefault(p => p.SKU == "BOOK003");
                var notebook = products.FirstOrDefault(p => p.SKU == "STAT001");
                var pen = products.FirstOrDefault(p => p.SKU == "STAT002");

                // Log missing products
                var missingProducts = new List<string>();
                if (iphone == null) missingProducts.Add("ELEC001");
                if (samsung == null) missingProducts.Add("ELEC002");
                if (macbook == null) missingProducts.Add("ELEC003");
                if (sonyHeadphone == null) missingProducts.Add("ELEC004");
                if (ipad == null) missingProducts.Add("ELEC005");
                if (ps5 == null) missingProducts.Add("ELEC006");
                if (dellXps == null) missingProducts.Add("ELEC007");
                if (appleWatch == null) missingProducts.Add("ELEC008");
                if (nikeShoe == null) missingProducts.Add("FASH001");
                if (adidasShoe == null) missingProducts.Add("FASH002");
                if (jeans == null) missingProducts.Add("FASH003");
                if (jacket == null) missingProducts.Add("FASH004");
                if (sunglasses == null) missingProducts.Add("FASH005");
                if (blazer == null) missingProducts.Add("FASH006");
                if (bookCleanCode == null) missingProducts.Add("BOOK001");
                if (bookDesign == null) missingProducts.Add("BOOK002");
                if (bookPragmatic == null) missingProducts.Add("BOOK003");
                if (notebook == null) missingProducts.Add("STAT001");
                if (pen == null) missingProducts.Add("STAT002");
                
                if (missingProducts.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"SeedOrders: Missing products: {string.Join(", ", missingProducts)}");
                    System.Diagnostics.Debug.WriteLine($"SeedOrders: Available SKUs: {string.Join(", ", products.Select(p => p.SKU ?? "NULL"))}");
                }

                // Kiểm tra có ít nhất một số products cơ bản không
                var essentialProducts = new[] { iphone, samsung, macbook, sonyHeadphone, nikeShoe, bookCleanCode };
                var hasEssentialProducts = essentialProducts.Any(p => p != null);
                
                if (!hasEssentialProducts)
                {
                    System.Diagnostics.Debug.WriteLine("SeedOrders: Not enough essential products to create orders. Please seed products first.");
                    return; // Không đủ sản phẩm cơ bản để tạo Orders
                }
                
                System.Diagnostics.Debug.WriteLine($"SeedOrders: Found {products.Count} products, will create orders with available products");

            var orders = new List<Order>();
            var today = DateTime.Now;

            // --- THÁNG TRƯỚC NỮA (Dữ liệu cũ - Đã hoàn tất) ---
            var order1 = CreateOrder(customers[0], today.AddDays(-60), OrderStatus.Paid, new[] { (iphone, 1), (sonyHeadphone, 1) }); // Đơn to
            if (order1 != null) orders.Add(order1);
            var order2 = CreateOrder(customers[1], today.AddDays(-58), OrderStatus.Paid, new[] { (bookCleanCode, 1), (bookDesign, 1) });
            if (order2 != null) orders.Add(order2);
            var order3 = CreateOrder(customers[2], today.AddDays(-55), OrderStatus.Cancelled, new[] { (macbook, 1) }); // Đơn hủy
            if (order3 != null) orders.Add(order3);
            var order4 = CreateOrder(customers[3], today.AddDays(-57), OrderStatus.Paid, new[] { (nikeShoe, 2), (jeans, 1) });
            if (order4 != null) orders.Add(order4);
            var order5 = CreateOrder(customers[4], today.AddDays(-59), OrderStatus.Paid, new[] { (appleWatch, 1) });
            if (order5 != null) orders.Add(order5);

            // --- THÁNG TRƯỚC (Doanh số tăng) ---
            var order6 = CreateOrder(customers[3], today.AddDays(-30), OrderStatus.Paid, new[] { (macbook, 1), (iphone, 1) }); // VIP Customer
            if (order6 != null) orders.Add(order6);
            var order7 = CreateOrder(customers[4], today.AddDays(-28), OrderStatus.Paid, new[] { (nikeShoe, 2) });
            if (order7 != null) orders.Add(order7);
            var order8 = CreateOrder(customers[0], today.AddDays(-25), OrderStatus.Paid, new[] { (bookCleanCode, 5) }); // Mua cho team
            if (order8 != null) orders.Add(order8);
            var order9 = CreateOrder(customers[1], today.AddDays(-20), OrderStatus.Paid, new[] { (sonyHeadphone, 1) });
            if (order9 != null) orders.Add(order9);
            var order10 = CreateOrder(customers[5], today.AddDays(-22), OrderStatus.Paid, new[] { (ipad, 1), (bookPragmatic, 2) });
            if (order10 != null) orders.Add(order10);
            var order11 = CreateOrder(customers[2], today.AddDays(-27), OrderStatus.Paid, new[] { (sunglasses, 1), (blazer, 1) });
            if (order11 != null) orders.Add(order11);
            var order12 = CreateOrder(customers[0], today.AddDays(-24), OrderStatus.Cancelled, new[] { (ps5, 1) }); // Đơn hủy
            if (order12 != null) orders.Add(order12);

            // --- TUẦN NÀY (Dữ liệu mới - Hỗn hợp trạng thái) ---
            // Đơn 3 ngày trước - Đã thanh toán
            var order13 = CreateOrder(customers[0], today.AddDays(-3), OrderStatus.Paid, new[] { (samsung, 1), (appleWatch, 1) });
            if (order13 != null) orders.Add(order13);
            // Đơn 2 ngày trước - Đã thanh toán
            var order14 = CreateOrder(customers[1], today.AddDays(-2), OrderStatus.Paid, new[] { (adidasShoe, 1), (jeans, 2) });
            if (order14 != null) orders.Add(order14);
            // Đơn hôm qua - Đã thanh toán
            var order15 = CreateOrder(customers[2], today.AddDays(-1), OrderStatus.Paid, new[] { (iphone, 2) });
            if (order15 != null) orders.Add(order15);
            // Đơn hôm qua - Đã thanh toán (đơn lớn)
            var order16 = CreateOrder(customers[5], today.AddDays(-1).AddHours(-5), OrderStatus.Paid, new[] { (ipad, 1), (bookCleanCode, 2), (bookPragmatic, 1) });
            if (order16 != null) orders.Add(order16);
            // Đơn hôm nay - Mới tạo (Chưa thanh toán)
            var order17 = CreateOrder(customers[3], today, OrderStatus.Created, new[] { (macbook, 1), (bookDesign, 1) });
            if (order17 != null) orders.Add(order17);
            // Đơn hôm nay - Đã thanh toán
            var order18 = CreateOrder(customers[5], today.AddHours(-2), OrderStatus.Paid, new[] { (nikeShoe, 1), (sonyHeadphone, 1) });
            if (order18 != null) orders.Add(order18);
            // Đơn hôm nay - Đã thanh toán (thời trang)
            var order19 = CreateOrder(customers[0], today.AddHours(-3), OrderStatus.Paid, new[] { (sunglasses, 1), (blazer, 1), (jacket, 1) });
            if (order19 != null) orders.Add(order19);
            // Đơn hôm nay - Mới tạo
            var order20 = CreateOrder(customers[1], today.AddHours(-1), OrderStatus.Created, new[] { (ps5, 1) });
            if (order20 != null) orders.Add(order20);
            // Đơn hôm nay - Vừa hủy
            var order21 = CreateOrder(customers[4], today.AddHours(-1), OrderStatus.Cancelled, new[] { (iphone, 1) });
            if (order21 != null) orders.Add(order21);
            // Đơn hôm nay - Đã thanh toán (văn phòng phẩm)
            var order22 = CreateOrder(customers[2], today.AddHours(-30), OrderStatus.Paid, new[] { (notebook, 5), (pen, 3) });
            if (order22 != null) orders.Add(order22);
            // Đơn hôm nay - Đã thanh toán (điện tử cao cấp)
            var order23 = CreateOrder(customers[3], today.AddHours(-20), OrderStatus.Paid, new[] { (dellXps, 1), (sonyHeadphone, 1) });
            if (order23 != null) orders.Add(order23);
            // Thêm một số đơn nữa để test
            var order24 = CreateOrder(customers[5], today.AddDays(-4), OrderStatus.Paid, new[] { (bookCleanCode, 3), (bookDesign, 2), (bookPragmatic, 1) });
            if (order24 != null) orders.Add(order24);
            var order25 = CreateOrder(customers[0], today.AddDays(-5), OrderStatus.Paid, new[] { (jacket, 2), (jeans, 3) });
            if (order25 != null) orders.Add(order25);
            var order26 = CreateOrder(customers[1], today.AddDays(-6), OrderStatus.Created, new[] { (appleWatch, 1), (sonyHeadphone, 1) });
            if (order26 != null) orders.Add(order26);
            var order27 = CreateOrder(customers[2], today.AddDays(-7), OrderStatus.Paid, new[] { (notebook, 10), (pen, 5) });
            if (order27 != null) orders.Add(order27);

                System.Diagnostics.Debug.WriteLine($"SeedOrders: Created {orders.Count} orders in memory");
                
                if (orders.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SeedOrders: WARNING - No orders to save! Check if products are available.");
                    return;
                }
                
                context.Orders.AddRange(orders);
                var savedCount = context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SeedOrders: Saved {orders.Count} orders to database (SaveChanges returned {savedCount})");
                
                // Verify orders were saved
                var verifyCount = context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Items.Any())
                    .Count();
                System.Diagnostics.Debug.WriteLine($"SeedOrders: Verification - Found {verifyCount} orders with items in database");
                
                // Cập nhật lại tồn kho cho các sản phẩm bán chạy (để test cảnh báo hết hàng)
                if (iphone != null)
                {
                    iphone.Quantity = 5;
                }
                if (macbook != null)
                {
                    macbook.Quantity = 2; // Sắp hết hàng
                }
                
                context.SaveChanges();
                System.Diagnostics.Debug.WriteLine("SeedOrders: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SeedOrders: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
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
            // Chỉ tìm file ảnh thực tế cho ảnh chính (Main)
            // Các ảnh phụ (Side, Detail) sẽ dùng cùng file hoặc placeholder
            if (type == "Main")
            {
                var imagePath = FindProductImageFile(productName);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    return imagePath;
                }
            }
            else
            {
                // Cho ảnh phụ, thử dùng cùng file với ảnh chính
                var mainImagePath = FindProductImageFile(productName);
                if (!string.IsNullOrEmpty(mainImagePath))
                {
                    return mainImagePath; // Dùng cùng file cho tất cả ảnh
                }
            }
            
            // Fallback về placeholder nếu không tìm thấy file
            var encodedName = Uri.EscapeDataString($"{productName} - {type}");
            var color = Math.Abs(productName.GetHashCode()).ToString("X").Substring(0, 6);
            return $"https://placehold.co/600x400/{color}/FFF?text={encodedName}";
        }

        /// <summary>
        /// Mapping tên sản phẩm với tên file ảnh (để xử lý các trường hợp đặc biệt)
        /// </summary>
        private static readonly Dictionary<string, string> ProductImageMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "iPhone 15 Pro Max", "iPhone_15_Pro_Max" },
            { "Samsung Galaxy S24 Ultra", "Samsung_Galaxy_S24_Ultra" },
            { "MacBook Pro 14 M3", "MacBook_Pro_14_M3" },
            { "Sony WH-1000XM5", "Sony_WH-1000XM5" },
            { "iPad Pro 12.9 M2", "iPad_Pro_12.9_M2" },
            { "PlayStation 5 Slim", "PlayStation_5_Slim" },
            { "Dell XPS 15 9530", "Dell_XPS_15_9530" },
            { "Apple Watch Series 9", "Apple_Watch_Series_9" },
            { "Nike Air Force 1", "Nike_Air_Force_1" },
            { "Adidas Ultraboost Light", "Adidas_Ultraboost_Light" },
            { "Levis 501 Original Jeans", "Levis_501_Original_Jeans" },
            { "Uniqlo Áo Khoác Phao", "Uniqlo_Áo_Khoác_Phao" },
            { "Ray-Ban Aviator", "Ray-Ban_Aviator" },
            { "Zara Blazer Nam", "Zara_Blazer_Nam" },
            { "Clean Code", "image_Code" }, // File có tên khác
            { "Design Patterns (GoF)", "Design_Patterns" },
            { "The Pragmatic Programmer", "The_Pragmatic_Programmer" },
            { "Sổ tay Moleskine", "Sổ_tay_Moleskine" },
            { "Bút Lamy Safari", "Bút_Lamy_Safari" },
        };

        /// <summary>
        /// Tìm file ảnh của sản phẩm trong folder product_image
        /// Hỗ trợ nhiều định dạng: .jpg, .png, .avif, .webp
        /// </summary>
        private static string? FindProductImageFile(string productName)
        {
            try
            {
                // Lấy đường dẫn của assembly hiện tại
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                
                if (string.IsNullOrEmpty(assemblyDirectory))
                {
                    // Fallback: sử dụng AppDomain.CurrentDomain.BaseDirectory
                    assemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }
                
                // Tìm folder product_image
                // Thử nhiều đường dẫn có thể (ưu tiên UI project Assets trước):
                var possiblePaths = new[]
                {
                    // Trong output directory của UI project (AppX)
                    Path.Combine(assemblyDirectory, "Assets", "product_image"),
                    // Trong output directory của UI project (không phải AppX)
                    Path.Combine(assemblyDirectory, "..", "Assets", "product_image"),
                    Path.Combine(assemblyDirectory, "..", "..", "Assets", "product_image"),
                    // Từ source code UI project
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "product_image"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "Project_MyShop_2025", "Assets", "product_image"),
                    // Từ Core project
                    Path.Combine(assemblyDirectory, "..", "..", "..", "..", "Project_MyShop_2025.Core", "Assets", "product_image"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Project_MyShop_2025.Core", "Assets", "product_image"),
                    // Thử với đường dẫn tuyệt đối từ workspace
                    Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", "..", "Project_MyShop_2025", "Assets", "product_image")),
                    Path.GetFullPath(Path.Combine(assemblyDirectory, "..", "..", "..", "..", "Project_MyShop_2025.Core", "Assets", "product_image")),
                };
                
                string? productImageFolder = null;
                foreach (var path in possiblePaths)
                {
                    var normalizedPath = Path.GetFullPath(path);
                    if (Directory.Exists(normalizedPath))
                    {
                        productImageFolder = normalizedPath;
                        break;
                    }
                }
                
                if (string.IsNullOrEmpty(productImageFolder))
                {
                    System.Diagnostics.Debug.WriteLine($"FindProductImageFile: Cannot find product_image folder.");
                    System.Diagnostics.Debug.WriteLine($"Assembly location: {assemblyLocation}");
                    System.Diagnostics.Debug.WriteLine($"Assembly directory: {assemblyDirectory}");
                    System.Diagnostics.Debug.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
                    System.Diagnostics.Debug.WriteLine($"Searched paths: {string.Join("\n  - ", possiblePaths.Select(p => $"{p} (Exists: {Directory.Exists(Path.GetFullPath(p))})"))}");
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"FindProductImageFile: Found product_image folder at: {productImageFolder}");
                
                // Lấy tên file base từ mapping hoặc tự động generate
                string fileNameBase;
                if (ProductImageMapping.TryGetValue(productName, out var mappedName))
                {
                    fileNameBase = mappedName;
                }
                else
                {
                    // Chuyển đổi tên sản phẩm thành format tên file
                    fileNameBase = productName
                        .Replace(" ", "_")
                        .Replace("-", "_")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("&", "_");
                }
                
                // Các định dạng ảnh được hỗ trợ (ưu tiên .jpg trước)
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".avif", ".webp" };
                
                // Tìm file với các extension khác nhau
                foreach (var ext in extensions)
                {
                    var fileName = fileNameBase + ext;
                    var fullPath = Path.Combine(productImageFolder, fileName);
                    
                    if (File.Exists(fullPath))
                    {
                        // Trả về đường dẫn file:// đúng format cho WinUI (cần 3 slashes: file:///)
                        // Chuyển đổi backslash thành forward slash và thêm file:// prefix
                        var uriPath = fullPath.Replace('\\', '/');
                        if (!uriPath.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                        {
                            uriPath = "file:///" + uriPath;
                        }
                        System.Diagnostics.Debug.WriteLine($"FindProductImageFile: Found image for '{productName}': {uriPath}");
                        return uriPath;
                    }
                }
                
                // Nếu không tìm thấy với tên chính xác, thử tìm với tên gần đúng
                var files = Directory.GetFiles(productImageFolder);
                var matchingFile = files.FirstOrDefault(f => 
                {
                    var fileName = Path.GetFileNameWithoutExtension(f);
                    // So sánh không phân biệt hoa thường và bỏ qua dấu gạch dưới/khoảng trắng
                    var normalizedFileName = fileName.Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                    var normalizedProductName = productName.Replace(" ", "").Replace("-", "").Replace("_", "").Replace("(", "").Replace(")", "").ToLowerInvariant();
                    return normalizedFileName.Contains(normalizedProductName) || normalizedProductName.Contains(normalizedFileName);
                });
                
                if (matchingFile != null)
                {
                    // Trả về đường dẫn file:// đúng format cho WinUI
                    var uriPath = matchingFile.Replace('\\', '/');
                    if (!uriPath.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                    {
                        uriPath = "file:///" + uriPath;
                    }
                    return uriPath;
                }
                
                System.Diagnostics.Debug.WriteLine($"FindProductImageFile: Cannot find image for product '{productName}' in folder '{productImageFolder}'. Available files: {string.Join(", ", files.Select(Path.GetFileName))}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindProductImageFile: Error finding image for '{productName}': {ex.Message}");
                return null;
            }
        }

        private static Order CreateOrder(CustomerInfo customer, DateTime date, OrderStatus status, (Product? prod, int qty)[] items)
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
                // Skip null products
                if (item.prod == null)
                {
                    System.Diagnostics.Debug.WriteLine($"CreateOrder: Skipping null product in order for {customer.Name}");
                    continue;
                }
                
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
            
            // Only return order if it has at least one item
            if (order.Items.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"CreateOrder: Order for {customer.Name} has no valid items, returning null");
                return null!; // Return null order - caller should check
            }
            
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
                new CustomerInfo { Name = "Vũ Quang Huy", Phone = "0977888999", Address = "Landmark 81, Bình Thạnh, TP.HCM" },
                new CustomerInfo { Name = "Hoàng Thị Mai", Phone = "0911222333", Address = "15 Lê Duẩn, Q.1, TP.HCM" },
                new CustomerInfo { Name = "Ngô Văn Hùng", Phone = "0922333444", Address = "88 Hai Bà Trưng, Q.3, TP.HCM" },
                new CustomerInfo { Name = "Bùi Thị Lan", Phone = "0933444555", Address = "20 Cộng Hòa, Tân Bình, TP.HCM" },
                new CustomerInfo { Name = "Đỗ Minh Tuấn", Phone = "0944555666", Address = "5 Nguyễn Trãi, Q.5, TP.HCM" },
                new CustomerInfo { Name = "Lý Thị Hà", Phone = "0955666777", Address = "12 Hoàng Diệu, Q.4, TP.HCM" },
                new CustomerInfo { Name = "Trương Văn Long", Phone = "0966777888", Address = "7 Trần Hưng Đạo, Q.1, TP.HCM" },
                new CustomerInfo { Name = "Đinh Thị Ngọc", Phone = "0977888999", Address = "30 Phạm Văn Đồng, Gò Vấp, TP.HCM" },
                new CustomerInfo { Name = "Võ Văn Nam", Phone = "0988999000", Address = "9 Xô Viết Nghệ Tĩnh, Bình Thạnh, TP.HCM" },
                new CustomerInfo { Name = "Dương Thị Tuyết", Phone = "0999000111", Address = "50 Nguyễn Thị Minh Khai, Q.1, TP.HCM" },
                new CustomerInfo { Name = "Hồ Văn Kiệt", Phone = "0901112223", Address = "100 Lý Thường Kiệt, Q.10, TP.HCM" },
                new CustomerInfo { Name = "Cao Thị Hương", Phone = "0912223334", Address = "25 Nguyễn Văn Cừ, Q.5, TP.HCM" },
                new CustomerInfo { Name = "Phan Văn Đức", Phone = "0923334445", Address = "60 Trường Chinh, Tân Bình, TP.HCM" },
                new CustomerInfo { Name = "Trịnh Thị Hoa", Phone = "0934445556", Address = "80 Cách Mạng Tháng 8, Q.3, TP.HCM" },
                new CustomerInfo { Name = "Lâm Văn Phúc", Phone = "0945556667", Address = "40 Võ Văn Tần, Q.3, TP.HCM" }
            };
        }

        public static void SeedPromotions(ShopDbContext context)
        {
            if (context.Promotions.Any()) return;

            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Name = "Siêu Sale Mùa Hè",
                    Code = "SUMMER2025",
                    Description = "Giảm giá 10% cho tất cả đơn hàng chào hè.",
                    Type = PromotionType.Percentage,
                    DiscountValue = 10,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(30),
                    IsActive = true,
                    UsageLimit = 1000
                },
                new Promotion
                {
                    Name = "Khách Hàng Mới",
                    Code = "WELCOME",
                    Description = "Giảm 50k cho đơn hàng đầu tiên.",
                    Type = PromotionType.FixedAmount,
                    DiscountValue = 50000,
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(5),
                    IsActive = true,
                    MinOrderAmount = 200000
                },
                new Promotion
                {
                    Name = "VIP Discount",
                    Code = "VIPMEMBER",
                    Description = "Giảm 15% tối đa 500k cho thành viên VIP.",
                    Type = PromotionType.Percentage,
                    DiscountValue = 15,
                    MaxDiscountAmount = 500000,
                    StartDate = DateTime.Now.AddYears(-1),
                    EndDate = DateTime.Now.AddYears(1),
                    IsActive = true
                },
                new Promotion
                {
                    Name = "Flash Sale Điện Tử",
                    Code = "TECHFLASH",
                    Description = "Giảm 500k cho đơn hàng điện tử trên 5 triệu.",
                    Type = PromotionType.FixedAmount,
                    DiscountValue = 500000,
                    MinOrderAmount = 5000000,
                    StartDate = DateTime.Now.AddDays(-2),
                    EndDate = DateTime.Now.AddDays(5),
                    IsActive = true
                },
                new Promotion
                {
                    Name = "Back to School",
                    Code = "SCHOOL2025",
                    Description = "Giảm 20% cho sách và văn phòng phẩm.",
                    Type = PromotionType.Percentage,
                    DiscountValue = 20,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(2),
                    IsActive = true
                },
                new Promotion
                {
                    Name = "Black Friday Sớm",
                    Code = "BF2025EARLY",
                    Description = "Giảm giá sốc 30% cho một số mặt hàng thời trang.",
                    Type = PromotionType.Percentage,
                    DiscountValue = 30,
                    StartDate = DateTime.Now.AddDays(10), // Chưa diễn ra
                    EndDate = DateTime.Now.AddDays(15),
                    IsActive = true
                },
                new Promotion
                {
                    Name = "Hết Hạn - Tết 2024",
                    Code = "TET2024",
                    Description = "Khuyến mãi Tết Nguyên Đán đã kết thúc.",
                    Type = PromotionType.FixedAmount,
                    DiscountValue = 100000,
                    StartDate = DateTime.Now.AddMonths(-5),
                    EndDate = DateTime.Now.AddMonths(-4),
                    IsActive = false
                },
                new Promotion
                {
                    Name = "Mua 2 Tặng 1 (Sách)",
                    Code = "BOOKDEAL",
                    Description = "Mua 2 cuốn sách bất kỳ được tặng 1 cuốn (trị giá thấp nhất).",
                    Type = PromotionType.BuyXGetY,
                    DiscountValue = 2, // Mua 2
                    FreeQuantity = 1,  // Tặng 1
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(1),
                    IsActive = true
                }
            };
            
            context.Promotions.AddRange(promotions);
            context.SaveChanges();
            System.Diagnostics.Debug.WriteLine($"Seeded {promotions.Count} promotions");
        }

        public static void SeedUsers(ShopDbContext context)
        {
            if (context.Users.Any()) return;

            // Sử dụng PasswordHelper để hash mật khẩu (nếu có helpers)
            // Hoặc tạo trực tiếp nếu Helper chưa sẵn sàng trong context seeding này
            // Giả sử có class PasswordHelper static
            
            string adminSalt, adminHash;
            Project_MyShop_2025.Core.Helpers.PasswordHelper.CreatePasswordHash("admin123", out adminHash, out adminSalt);

            string staffSalt, staffHash;
            Project_MyShop_2025.Core.Helpers.PasswordHelper.CreatePasswordHash("staff123", out staffHash, out staffSalt);

            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    PasswordHash = adminHash,
                    PasswordSalt = adminSalt,
                    Email = "admin@myshop.com",
                    Role = "Admin",
                    FullName = "Administrator",
                    Phone = "0909000111"
                },
                new User
                {
                    Username = "staff",
                    PasswordHash = staffHash,
                    PasswordSalt = staffSalt,
                    Email = "staff@myshop.com",
                    Role = "Staff",
                    FullName = "Nhân Viên Bán Hàng",
                    Phone = "0909000222"
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
            System.Diagnostics.Debug.WriteLine($"Seeded {users.Count} users");
        }

        public static void SeedCustomers(ShopDbContext context)
        {
            if (context.Customers.Any()) return;

            var customers = new List<Customer>
            {
                new Customer { Name = "Nguyễn Văn An", Phone = "0901234567", Email = "an.nguyen@gmail.com", Address = "123 Nguyễn Huệ, Q.1, TP.HCM", LoyaltyPoints = 1500, CreatedAt = DateTime.Now.AddMonths(-6), IsActive = true },
                new Customer { Name = "Trần Thị Bình", Phone = "0912345678", Email = "binh.tran@yahoo.com", Address = "456 Lê Lợi, Q.3, TP.HCM", LoyaltyPoints = 2300, CreatedAt = DateTime.Now.AddMonths(-8), IsActive = true },
                new Customer { Name = "Lê Hoàng Cường", Phone = "0923456789", Email = "cuong.le@hotmail.com", Address = "789 Trần Hưng Đạo, Q.5, TP.HCM", LoyaltyPoints = 800, CreatedAt = DateTime.Now.AddMonths(-3), IsActive = true },
                new Customer { Name = "Phạm Minh Đức", Phone = "0934567890", Email = "duc.pham@outlook.com", Address = "101 Võ Văn Tần, Q.3, TP.HCM", LoyaltyPoints = 3200, CreatedAt = DateTime.Now.AddMonths(-12), IsActive = true, Notes = "Khách hàng VIP" },
                new Customer { Name = "Hoàng Thị Hương", Phone = "0945678901", Email = "huong.hoang@gmail.com", Address = "202 Cách Mạng Tháng 8, Q.10, TP.HCM", LoyaltyPoints = 500, CreatedAt = DateTime.Now.AddMonths(-2), IsActive = true },
                new Customer { Name = "Vũ Quốc Khánh", Phone = "0956789012", Email = "khanh.vu@gmail.com", Address = "303 Lý Thường Kiệt, Q.Tân Bình, TP.HCM", LoyaltyPoints = 1200, CreatedAt = DateTime.Now.AddMonths(-5), IsActive = true },
                new Customer { Name = "Đặng Thanh Lan", Phone = "0967890123", Email = "lan.dang@yahoo.com", Address = "404 Nguyễn Đình Chiểu, Q.Bình Thạnh, TP.HCM", LoyaltyPoints = 900, CreatedAt = DateTime.Now.AddMonths(-4), IsActive = true },
                new Customer { Name = "Bùi Văn Minh", Phone = "0978901234", Email = "minh.bui@gmail.com", Address = "505 Điện Biên Phủ, Q.Bình Thạnh, TP.HCM", LoyaltyPoints = 2100, CreatedAt = DateTime.Now.AddMonths(-9), IsActive = true },
                new Customer { Name = "Đỗ Thị Ngọc", Phone = "0989012345", Email = "ngoc.do@hotmail.com", Address = "606 Nguyễn Thị Minh Khai, Q.1, TP.HCM", LoyaltyPoints = 1800, CreatedAt = DateTime.Now.AddMonths(-7), IsActive = true },
                new Customer { Name = "Ngô Quang Phú", Phone = "0990123456", Email = "phu.ngo@gmail.com", Address = "707 Hai Bà Trưng, Q.1, TP.HCM", LoyaltyPoints = 650, CreatedAt = DateTime.Now.AddMonths(-1), IsActive = true },
                new Customer { Name = "Trịnh Văn Quân", Phone = "0901111222", Email = "quan.trinh@outlook.com", Address = "808 Phan Đình Phùng, Q.Phú Nhuận, TP.HCM", LoyaltyPoints = 2500, CreatedAt = DateTime.Now.AddMonths(-10), IsActive = true, Notes = "Mua số lượng lớn" },
                new Customer { Name = "Lý Thu Sương", Phone = "0912222333", Email = "suong.ly@gmail.com", Address = "909 Cộng Hòa, Q.Tân Bình, TP.HCM", LoyaltyPoints = 750, CreatedAt = DateTime.Now.AddMonths(-3), IsActive = true },
                new Customer { Name = "Mai Xuân Tâm", Phone = "0923333444", Email = "tam.mai@yahoo.com", Address = "1010 Sư Vạn Hạnh, Q.10, TP.HCM", LoyaltyPoints = 400, CreatedAt = DateTime.Now.AddDays(-45), IsActive = true },
                new Customer { Name = "Phan Thị Uyên", Phone = "0934444555", Email = "uyen.phan@gmail.com", Address = "1111 Nguyễn Trãi, Q.5, TP.HCM", LoyaltyPoints = 1900, CreatedAt = DateTime.Now.AddMonths(-6), IsActive = true },
                new Customer { Name = "Hồ Văn Vinh", Phone = "0945555666", Email = "vinh.ho@hotmail.com", Address = "1212 Lý Tự Trọng, Q.1, TP.HCM", LoyaltyPoints = 3000, CreatedAt = DateTime.Now.AddMonths(-11), IsActive = true, Notes = "Khách hàng thân thiết" },
                new Customer { Name = "Đinh Thị Xuân", Phone = "0956666777", Email = "xuan.dinh@gmail.com", Address = "1313 Phan Văn Trị, Q.Gò Vấp, TP.HCM", LoyaltyPoints = 550, CreatedAt = DateTime.Now.AddMonths(-2), IsActive = true },
                new Customer { Name = "Võ Công Yên", Phone = "0967777888", Email = "yen.vo@outlook.com", Address = "1414 Lê Văn Sỹ, Q.3, TP.HCM", LoyaltyPoints = 1100, CreatedAt = DateTime.Now.AddMonths(-4), IsActive = true },
                new Customer { Name = "Nguyễn Bảo Zân", Phone = "0978888999", Email = "zan.nguyen@gmail.com", Address = "1515 Pasteur, Q.3, TP.HCM", LoyaltyPoints = 280, CreatedAt = DateTime.Now.AddDays(-20), IsActive = true },
                new Customer { Name = "Trương Thị Ánh", Phone = "0989999000", Email = "anh.truong@yahoo.com", Address = "1616 Nguyễn Văn Cừ, Q.5, TP.HCM", LoyaltyPoints = 2000, CreatedAt = DateTime.Now.AddMonths(-8), IsActive = false, Notes = "Tài khoản tạm ngưng" },
                new Customer { Name = "Lưu Minh Bách", Phone = "0900000111", Email = "bach.luu@gmail.com", Address = "1717 An Dương Vương, Q.5, TP.HCM", LoyaltyPoints = 1400, CreatedAt = DateTime.Now.AddMonths(-5), IsActive = true }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
            System.Diagnostics.Debug.WriteLine($"Seeded {customers.Count} customers");
        }

        private class CustomerInfo
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
        }
    }
}