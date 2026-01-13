using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Project_MyShop_2025.Views
{
    public class BestSellerItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
    }

    public class OrderViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string TotalPrice { get; set; } = string.Empty;
    }

    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            this.InitializeComponent();
            this.Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            var app = (App)Application.Current;
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

                // 1. Total Products
                var totalProducts = context.Products.Count();
                TotalProductsText.Text = totalProducts.ToString();

                // 2. Low Stock Products (quantity < 5)
                var lowStockProducts = context.Products
                    .Where(p => p.Quantity < 5)
                    .OrderBy(p => p.Quantity)
                    .Take(5)
                    .ToList();
                LowStockList.ItemsSource = lowStockProducts;
                LowStockText.Text = context.Products.Count(p => p.Quantity < 5).ToString();

                // 3. Best Selling Products (Top 5 by total quantity sold)
                var bestSellers = context.OrderItems
                    .Include(oi => oi.Product)
                    .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                    .Select(g => new BestSellerItem
                    {
                        ProductName = g.Key.Name,
                        TotalSold = g.Sum(oi => oi.Quantity)
                    })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(5)
                    .ToList();
                BestSellersList.ItemsSource = bestSellers;

                // 4. Today's Orders & Revenue
                var today = DateTime.Today;
                var todayOrders = context.Orders
                    .Where(o => o.CreatedAt >= today && o.CreatedAt < today.AddDays(1))
                    .ToList();

                TodayOrdersText.Text = todayOrders.Count.ToString();
                
                var todayRevenue = todayOrders.Sum(o => o.TotalPrice);
                TodayRevenueText.Text = $"₫{todayRevenue:N0}";
                
                // Placeholder percentage - can calculate from previous week's data
                TodayRevenueSubText.Text = "+0% from last week";

                // 5. Recent 3 Orders with formatting
                var recentOrders = context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(3)
                    .Select(o => new OrderViewModel
                    {
                        Id = $"#{o.Id}",
                        CustomerName = o.CustomerName ?? "N/A",
                        CreatedAt = o.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        TotalPrice = $"₫{o.TotalPrice:N0}"
                    })
                    .ToList();
                RecentOrdersList.ItemsSource = recentOrders;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Clear Remember Me
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("RememberMe_Username");
            
            // Navigate back to Login
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
