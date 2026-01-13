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
                var yesterday = today.AddDays(-1);
                
                var todayOrders = context.Orders
                    .Where(o => o.CreatedAt >= today && o.CreatedAt < today.AddDays(1))
                    .ToList();

                var yesterdayOrders = context.Orders
                    .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today)
                    .ToList();

                TodayOrdersText.Text = todayOrders.Count.ToString();
                
                var todayRevenue = todayOrders.Sum(o => o.TotalPrice);
                var yesterdayRevenue = yesterdayOrders.Sum(o => o.TotalPrice);
                
                TodayRevenueText.Text = $"₫{todayRevenue:N0}";
                
                // Calculate difference
                var diff = todayRevenue - yesterdayRevenue;
                if (diff >= 0)
                {
                    TodayRevenueSubText.Text = $"+₫{diff:N0} from yesterday";
                    TodayRevenueSubText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 76, 175, 80)); // Green
                }
                else
                {
                    TodayRevenueSubText.Text = $"-₫{Math.Abs(diff):N0} from yesterday";
                    TodayRevenueSubText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 244, 67, 54)); // Red
                }

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

                // 6. Draw Revenue Chart
                DrawRevenueChart(context);
            }
        }

        private void DrawRevenueChart(ShopDbContext context)
        {
            // Get revenue data for current month
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var dailyRevenue = new Dictionary<int, int>();
            
            // Initialize all days of month with 0
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                dailyRevenue[day] = 0;
            }

            // Get orders for current month
            var monthOrders = context.Orders
                .Where(o => o.CreatedAt >= firstDayOfMonth && o.CreatedAt <= lastDayOfMonth)
                .ToList();

            // Aggregate revenue by day
            foreach (var order in monthOrders)
            {
                int day = order.CreatedAt.Day;
                dailyRevenue[day] += order.TotalPrice;
            }

            // Find max revenue for scaling
            int maxRevenue = dailyRevenue.Values.Max();
            if (maxRevenue == 0) maxRevenue = 1; // Avoid division by zero

            // Clear previous chart
            RevenueChart.Children.Clear();
            ChartLabels.Children.Clear();

            double chartWidth = RevenueChart.ActualWidth > 0 ? RevenueChart.ActualWidth : 1000;
            double chartHeight = RevenueChart.ActualHeight > 0 ? RevenueChart.ActualHeight : 220;
            
            double barWidth = Math.Max(15, chartWidth / (lastDayOfMonth.Day + 2));
            double spacing = 2;

            // Draw bars
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                int revenue = dailyRevenue[day];
                double barHeight = (revenue / (double)maxRevenue) * (chartHeight - 40);

                var bar = new Microsoft.UI.Xaml.Shapes.Rectangle
                {
                    Width = barWidth - spacing,
                    Height = Math.Max(2, barHeight),
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 33, 150, 243)), // Blue
                    RadiusX = 2,
                    RadiusY = 2
                };

                Microsoft.UI.Xaml.Controls.Canvas.SetLeft(bar, day * barWidth);
                Microsoft.UI.Xaml.Controls.Canvas.SetTop(bar, chartHeight - barHeight - 20);
                RevenueChart.Children.Add(bar);

                // Add label every 5 days
                if (day % 5 == 1 || day == lastDayOfMonth.Day)
                {
                    var label = new TextBlock
                    {
                        Text = day.ToString(),
                        FontSize = 10,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Windows.UI.Color.FromArgb(255, 128, 128, 128)),
                        Width = barWidth * 5,
                        TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center
                    };
                    ChartLabels.Children.Add(label);
                }
            }

            // Update summary
            int totalMonthRevenue = dailyRevenue.Values.Sum();
            ChartSummaryText.Text = $"Total revenue this month: ₫{totalMonthRevenue:N0} from {monthOrders.Count} orders";
        }
    }
}
