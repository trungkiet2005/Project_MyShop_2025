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
            if (context.Products.Any())
            {
                return; // DB has been  seeded
            }

            // Create 3 main categories as required
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics", Description = "Latest electronic devices and accessories" },
                    new Category { Name = "Fashion & Apparel", Description = "Clothing and fashion accessories" },
                    new Category { Name = "Books & Stationery", Description = "Books, notebooks, and office supplies" },
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Re-fetch categories to get valid IDs
            var electronics = context.Categories.FirstOrDefault(c => c.Name == "Electronics");
            var fashion = context.Categories.FirstOrDefault(c => c.Name == "Fashion & Apparel");
            var books = context.Categories.FirstOrDefault(c => c.Name == "Books & Stationery");

            int elecId = electronics?.Id ?? 1;
            int fashId = fashion?.Id ?? 2;
            int bookId = books?.Id ?? 3;

            // ===== ELECTRONICS (22 Products) =====
            var electronicsProducts = new List<Product>
            {
                new Product { Name = "iPhone 15 Pro Max", Price = 29990000, ImportPrice = 25000000, Quantity = 15, CategoryId = elecId, SKU = "ELEC001", Description = "Latest flagship smartphone with A17 Pro chip and titanium design" },
                new Product { Name = "Samsung Galaxy S24 Ultra", Price = 27990000, ImportPrice = 23000000, Quantity = 20, CategoryId = elecId, SKU = "ELEC002", Description = "Premium Android smartphone with S Pen and AI features" },
                new Product { Name = "MacBook Pro 14\" M3", Price = 45990000, ImportPrice = 40000000, Quantity = 12, CategoryId = elecId, SKU = "ELEC003", Description = "Professional laptop with M3 chip and Liquid Retina XDR display" },
                new Product { Name = "Dell XPS 15", Price = 35990000, ImportPrice = 30000000, Quantity = 18, CategoryId = elecId, SKU = "ELEC004", Description = "High-performance Windows laptop with OLED display" },
                new Product { Name = "iPad Pro 12.9\"", Price = 25990000, ImportPrice = 22000000, Quantity = 25, CategoryId = elecId, SKU = "ELEC005", Description = "Powerful tablet with M2 chip and Apple Pencil support" },
                
                new Product { Name = "Apple Watch Series 9", Price = 9990000, ImportPrice = 8500000, Quantity = 30, CategoryId = elecId, SKU = "ELEC006", Description = "Advanced smartwatch with health tracking features" },
                new Product { Name = "Sony WH-1000XM5", Price = 7990000, ImportPrice = 6500000, Quantity = 40, CategoryId = elecId, SKU = "ELEC007", Description = "Premium noise-cancelling wireless headphones" },
                new Product { Name = "AirPods Pro 2", Price = 5990000, ImportPrice = 5000000, Quantity = 50, CategoryId = elecId, SKU = "ELEC008", Description = "True wireless earbuds with active noise cancellation" },
                new Product { Name = "Samsung 55\" QLED 4K TV", Price = 18990000, ImportPrice = 15000000, Quantity = 8, CategoryId = elecId, SKU = "ELEC009", Description = "Quantum Dot 4K Smart TV with HDR10+" },
                new Product { Name = "LG 65\" OLED TV", Price = 35990000, ImportPrice = 30000000, Quantity = 5, CategoryId = elecId, SKU = "ELEC010", Description = "Premium OLED TV with perfect blacks and wide viewing angles" },
                
                new Product { Name = "Canon EOS R6 Mark II", Price = 55990000, ImportPrice = 48000000, Quantity = 6, CategoryId = elecId, SKU = "ELEC011", Description = "Full-frame mirrorless camera for professionals" },
                new Product { Name = "Sony A7 IV", Price = 49990000, ImportPrice = 42000000, Quantity = 7, CategoryId = elecId, SKU = "ELEC012", Description = "Versatile full-frame camera with 33MP sensor" },
                new Product { Name = "DJI Mavic 3 Pro", Price = 43990000, ImportPrice = 38000000, Quantity = 4, CategoryId = elecId, SKU = "ELEC013", Description = "Professional drone with tri-camera system" },
                new Product { Name = "GoPro Hero 12 Black", Price = 10990000, ImportPrice = 9000000, Quantity = 22, CategoryId = elecId, SKU = "ELEC014", Description = "Action camera with 5.3K video and HyperSmooth 6.0" },
                new Product { Name = "Nintendo Switch OLED", Price = 8490000, ImportPrice = 7000000, Quantity = 35, CategoryId = elecId, SKU = "ELEC015", Description = "Gaming console with vibrant OLED screen" },
                
                new Product { Name = "PlayStation 5", Price = 14990000, ImportPrice = 12500000, Quantity = 12, CategoryId = elecId, SKU = "ELEC016", Description = "Next-gen gaming console with 4K gaming" },
                new Product { Name = "Xbox Series X", Price = 13990000, ImportPrice = 11500000, Quantity = 15, CategoryId = elecId, SKU = "ELEC017", Description = "Powerful 4K gaming console with Game Pass" },
                new Product { Name = "Logitech MX Master 3S", Price = 2490000, ImportPrice = 2000000, Quantity = 60, CategoryId = elecId, SKU = "ELEC018", Description = "Premium wireless mouse for productivity" },
                new Product { Name = "Keychron K8 Pro", Price = 3290000, ImportPrice = 2700000, Quantity = 45, CategoryId = elecId, SKU = "ELEC019", Description = "Wireless mechanical keyboard with hot-swappable switches" },
                new Product { Name = "Samsung T7 Shield 2TB SSD", Price = 5990000, ImportPrice = 4800000, Quantity = 28, CategoryId = elecId, SKU = "ELEC020", Description = "Rugged portable SSD with fast transfer speeds" },
                
                new Product { Name = "Anker PowerCore 20000mAh", Price = 890000, ImportPrice = 600000, Quantity = 100, CategoryId = elecId, SKU = "ELEC021", Description = "High-capacity portable power bank" },
                new Product { Name = "TP-Link Archer AX73", Price = 2990000, ImportPrice = 2300000, Quantity = 32, CategoryId = elecId, SKU = "ELEC022", Description = "WiFi 6 router with MU-MIMO and beamforming" },
            };

            // ===== FASHION & APPAREL (22 Products) =====
            var fashionProducts = new List<Product>
            {
                new Product { Name = "Nike Air Force 1", Price = 2690000, ImportPrice = 2000000, Quantity = 45, CategoryId = fashId, SKU = "FASH001", Description = "Classic white sneakers with timeless design" },
                new Product { Name = "Adidas Ultraboost 22", Price = 3990000, ImportPrice = 3000000, Quantity = 38, CategoryId = fashId, SKU = "FASH002", Description = "Premium running shoes with Boost cushioning" },
                new Product { Name = "Levi's 501 Original Jeans", Price = 1790000, ImportPrice = 1200000, Quantity = 60, CategoryId = fashId, SKU = "FASH003", Description = "Iconic straight-fit denim jeans" },
                new Product { Name = "Uniqlo Heattech Shirt", Price = 490000, ImportPrice = 300000, Quantity = 150, CategoryId = fashId, SKU = "FASH004", Description = "Thermal base layer for cold weather" },
                new Product { Name = "The North Face Jacket", Price = 5990000, ImportPrice = 4500000, Quantity = 25, CategoryId = fashId, SKU = "FASH005", Description = "Waterproof insulated winter jacket" },
                
                new Product { Name = "Champion Hoodie", Price = 890000, ImportPrice = 600000, Quantity = 80, CategoryId = fashId, SKU = "FASH006", Description = "Classic pullover hoodie with logo" },
                new Product { Name = "Polo Ralph Lauren Shirt", Price = 2290000, ImportPrice = 1700000, Quantity = 42, CategoryId = fashId, SKU = "FASH007", Description = "Premium cotton polo shirt" },
                new Product { Name = "Calvin Klein Underwear Pack", Price = 690000, ImportPrice = 450000, Quantity = 95, CategoryId = fashId, SKU = "FASH008", Description = "3-pack cotton boxer briefs" },
                new Product { Name = "Lacoste Tennis Shoes", Price = 3290000, ImportPrice = 2500000, Quantity = 30, CategoryId = fashId, SKU = "FASH009", Description = "Classic court sneakers with crocodile logo" },
                new Product { Name = "Tommy Hilfiger Backpack", Price = 2490000, ImportPrice = 1800000, Quantity = 35, CategoryId = fashId, SKU = "FASH010", Description = "Stylish everyday backpack with laptop compartment" },
                
                new Product { Name = "Ray-Ban Aviator Sunglasses", Price = 3990000, ImportPrice = 3200000, Quantity = 28, CategoryId = fashId, SKU = "FASH011", Description = "Iconic aviator sunglasses with UV protection" },
                new Product { Name = "Casio G-Shock Watch", Price = 3290000, ImportPrice = 2600000, Quantity = 40, CategoryId = fashId, SKU = "FASH012", Description = "Shock-resistant digital sports watch" },
                new Product { Name = "Michael Kors Handbag", Price = 7990000, ImportPrice = 6000000, Quantity = 18, CategoryId = fashId, SKU = "FASH013", Description = "Leather tote bag with logo hardware" },
                new Product { Name = "Vans Old Skool Sneakers", Price = 1790000, ImportPrice = 1300000, Quantity = 52, CategoryId = fashId, SKU = "FASH014", Description = "Classic skateboard shoes with side stripe" },
                new Product { Name = "H&M Slim Fit Chinos", Price = 690000, ImportPrice = 450000, Quantity = 70, CategoryId = fashId, SKU = "FASH015", Description = "Versatile cotton chino pants" },
                
                new Product { Name = "Zara Leather Jacket", Price = 4990000, ImportPrice = 3800000, Quantity = 22, CategoryId = fashId, SKU = "FASH016", Description = "Genuine leather biker jacket" },
                new Product { Name = "Gap Crewneck Sweater", Price = 890000, ImportPrice = 600000, Quantity = 65, CategoryId = fashId, SKU = "FASH017", Description = "Soft cotton blend sweater" },
                new Product { Name = "Converse Chuck Taylor", Price = 1290000, ImportPrice = 900000, Quantity = 85, CategoryId = fashId, SKU = "FASH018", Description = "Classic high-top canvas sneakers" },
                new Product { Name = "Puma Track Pants", Price = 790000, ImportPrice = 550000, Quantity = 72, CategoryId = fashId, SKU = "FASH019", Description = "Athletic jogger pants with logo" },
                new Product { Name = "New Balance 574", Price = 2390000, ImportPrice = 1800000, Quantity = 48, CategoryId = fashId, SKU = "FASH020", Description = "Retro-inspired running shoes" },
                
                new Product { Name = "Timberland Boots", Price = 4290000, ImportPrice = 3300000, Quantity = 26, CategoryId = fashId, SKU = "FASH021", Description = "Premium waterproof leather boots" },
                new Product { Name = "Columbia Fleece Vest", Price = 1490000, ImportPrice = 1100000, Quantity = 38, CategoryId = fashId, SKU = "FASH022", Description = "Lightweight insulated vest for layering" },
            };

            // ===== BOOKS & STATIONERY (22 Products) =====
            var booksProducts = new List<Product>
            {
                new Product { Name = "Clean Code - Robert C. Martin", Price = 549000, ImportPrice = 400000, Quantity = 45, CategoryId = bookId, SKU = "BOOK001", Description = "A must-read for software developers on writing clean, maintainable code" },
                new Product { Name = "Design Patterns - Gang of Four", Price = 689000, ImportPrice = 500000, Quantity = 38, CategoryId = bookId, SKU = "BOOK002", Description = "Classic book on software design patterns" },
                new Product { Name = "The Pragmatic Programmer", Price = 599000, ImportPrice = 450000, Quantity = 42, CategoryId = bookId, SKU = "BOOK003", Description = "Your journey to master the art of programming" },
                new Product { Name = "Code Complete - Steve McConnell", Price = 749000, ImportPrice = 550000, Quantity = 35, CategoryId = bookId, SKU = "BOOK004", Description = "Comprehensive guide to software construction" },
                new Product { Name = "Introduction to Algorithms - CLRS", Price = 899000, ImportPrice = 650000, Quantity = 28, CategoryId = bookId, SKU = "BOOK005", Description = "The definitive textbook on algorithms" },
                
                new Product { Name = "Head First Design Patterns", Price = 649000, ImportPrice = 480000, Quantity = 50, CategoryId = bookId, SKU = "BOOK006", Description = "Learn design patterns in an engaging way" },
                new Product { Name = "Refactoring - Martin Fowler", Price = 679000, ImportPrice = 500000, Quantity = 40, CategoryId = bookId, SKU = "BOOK007", Description = "Improving the design of existing code" },
                new Product { Name = "Domain-Driven Design - Eric Evans", Price = 729000, ImportPrice = 530000, Quantity = 32, CategoryId = bookId, SKU = "BOOK008", Description = "Tackling complexity in the heart of software" },
                new Product { Name = "The Art of Computer Programming", Price = 1299000, ImportPrice = 950000, Quantity = 15, CategoryId = bookId, SKU = "BOOK009", Description = "Donald Knuth's legendary series on algorithms" },
                new Product { Name = "Cracking the Coding Interview", Price = 589000, ImportPrice = 430000, Quantity = 65, CategoryId = bookId, SKU = "BOOK010", Description = "Essential guide for tech interview preparation" },
                
                new Product { Name = "Moleskine Classic Notebook A5", Price = 389000, ImportPrice = 280000, Quantity = 120, CategoryId = bookId, SKU = "STAT001", Description = "Premium hardcover notebook with ruled pages" },
                new Product { Name = "Leuchtturm1917 Dotted Journal", Price = 449000, ImportPrice = 320000, Quantity = 90, CategoryId = bookId, SKU = "STAT002", Description = "High-quality bullet journal with numbered pages" },
                new Product { Name = "Pilot G2 Pen Set (12 Pack)", Price = 189000, ImportPrice = 120000, Quantity = 200, CategoryId = bookId, SKU = "STAT003", Description = "Smooth gel ink pens in assorted colors" },
                new Product { Name = "Staedtler Triplus Fineliner Set", Price = 329000, ImportPrice = 240000, Quantity = 85, CategoryId = bookId, SKU = "STAT004", Description = "Triangular fine-tip markers for precise writing" },
                new Product { Name = "Tombow Dual Brush Pens (24 Set)", Price = 1090000, ImportPrice = 800000, Quantity = 42, CategoryId = bookId, SKU = "STAT005", Description = "Professional brush pens for lettering and art" },
                
                new Product { Name = "Rhodia Dotpad A4", Price = 259000, ImportPrice = 180000, Quantity = 75, CategoryId = bookId, SKU = "STAT006", Description = "Premium dot grid notepad for sketching" },
                new Product { Name = "Post-it Notes Super Sticky Pack", Price = 149000, ImportPrice = 100000, Quantity = 180, CategoryId = bookId, SKU = "STAT007", Description = "Colorful sticky notes that stick better" },
                new Product { Name = "Sharpie Permanent Markers (12)", Price = 179000, ImportPrice = 120000, Quantity = 140, CategoryId = bookId, SKU = "STAT008", Description = "Bold permanent markers for labeling" },
                new Product { Name = "MUJI Gel Ink Ballpoint 0.5mm", Price = 89000, ImportPrice = 60000, Quantity = 250, CategoryId = bookId, SKU = "STAT009", Description = "Minimalist smooth-writing pens" },
                new Product { Name = "Faber-Castell Pencil Set", Price = 229000, ImportPrice = 160000, Quantity = 110, CategoryId = bookId, SKU = "STAT010", Description = "Professional graphite pencils for drawing" },
                
                new Product { Name = "Kokuyo Campus Notebook B5", Price = 129000, ImportPrice = 85000, Quantity = 160, CategoryId = bookId, SKU = "STAT011", Description = "Japanese style student notebook" },
                new Product { Name = "Midori MD Notebook A5", Price = 349000, ImportPrice = 250000, Quantity = 68, CategoryId = bookId, SKU = "STAT012", Description = "Premium writing notebook with cream paper" },
            };

            // Add all products
            context.Products.AddRange(electronicsProducts);
            context.Products.AddRange(fashionProducts);
            context.Products.AddRange(booksProducts);
            context.SaveChanges();

            // ===== ADD PRODUCT IMAGES (3+ per product) =====
            var allProducts = context.Products.ToList();
            var productImages = new List<ProductImage>();

            // Placeholder image URLs - in production, these would be actual product images
            var placeholderImages = new[]
            {
                "ms-appx:///Assets/StoreLogo.png",
                "ms-appx:///Assets/Square44x44Logo.png",
                "ms-appx:///Assets/Wide310x150Logo.png"
            };

            foreach (var product in allProducts)
            {
                // Add 3-5 images per product
                int imageCount = new Random(product.Id).Next(3, 6);
                for (int i = 0; i < imageCount; i++)
                {
                    productImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImagePath = placeholderImages[i % placeholderImages.Length],
                        DisplayOrder = i + 1
                    });
                }

                // Also set the main image
                product.Image = placeholderImages[0];
            }

            context.ProductImages.AddRange(productImages);
            context.SaveChanges();

            // ===== ADD SAMPLE ORDERS FOR DASHBOARD =====
            if (!context.Orders.Any())
            {
                var random = new Random();
                var customerNames = new[] { "Nguyễn Văn A", "Trần Thị B", "Lê Hoàng C", "Phạm Minh D", "Đặng Thu E", "Vũ Quang F", "Hoàng Lan G", "Bùi Hải H" };
                
                // Create orders for the past 30 days including today
                for (int i = 0; i < 30; i++)
                {
                    DateTime orderDate;
                    int ordersForDay;
                    
                    // More orders for recent days
                    if (i == 0) // Today
                    {
                        orderDate = DateTime.Now;
                        ordersForDay = random.Next(3, 7);
                    }
                    else if (i == 1) // Yesterday
                    {
                        orderDate = DateTime.Now.AddDays(-1);
                        ordersForDay = random.Next(2, 6);
                    }
                    else
                    {
                        orderDate = DateTime.Now.AddDays(-i);
                        ordersForDay = random.Next(1, 4);
                    }

                    for (int j = 0; j < ordersForDay; j++)
                    {
                        var order = new Order
                        {
                            CustomerName = customerNames[random.Next(customerNames.Length)],
                            CustomerPhone = $"090{random.Next(1000000, 9999999)}",
                            CustomerAddress = $"{random.Next(1, 999)} Nguyễn Huệ, Q.1, TP.HCM",
                            CreatedAt = orderDate.AddHours(random.Next(8, 20)).AddMinutes(random.Next(0, 60))
                        };

                        // Add 1-5 random items to each order
                        int itemCount = random.Next(1, 6);
                        var orderItems = new List<OrderItem>();
                        int orderTotal = 0;

                        for (int k = 0; k < itemCount; k++)
                        {
                            var product = allProducts[random.Next(allProducts.Count)];
                            int quantity = random.Next(1, 4);
                            int itemTotal = product.Price * quantity;
                            orderTotal += itemTotal;

                            orderItems.Add(new OrderItem
                            {
                                ProductId = product.Id,
                                Quantity = quantity,
                                Price = product.Price,
                                TotalPrice = itemTotal
                            });
                        }

                        order.TotalPrice = orderTotal;
                        order.Items = orderItems;
                        context.Orders.Add(order);
                    }
                }

                context.SaveChanges();
            }

            // Update some products to have low stock for testing
            var lowStockSKUs = new[] { "ELEC007", "FASH005", "BOOK009", "ELEC013" };
            var lowStockQuantities = new[] { 3, 2, 1, 4 };
            
            for (int i = 0; i < lowStockSKUs.Length; i++)
            {
                var product = context.Products.FirstOrDefault(p => p.SKU == lowStockSKUs[i]);
                if (product != null)
                {
                    product.Quantity = lowStockQuantities[i];
                }
            }

            context.SaveChanges();
        }
    }
}
