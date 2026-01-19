using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Windows.UI;
using Microsoft.UI.Dispatching;

namespace Project_MyShop_2025.Views
{
    public class BestSellerItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public string? Image { get; set; }
        public double ProgressPercentage { get; set; }
    }

    public class OrderViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string TotalPrice { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public Brush StatusBackground { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        public Brush StatusForeground { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.Black);
    }

    public sealed partial class DashboardPage : Page
    {
        private string _currentChartRange = "month";

        public DashboardPage()
        {
            this.InitializeComponent();
            this.Loaded += DashboardPage_Loaded;
            this.SizeChanged += DashboardPage_SizeChanged;
            
            // Wait for layout to complete before drawing sparklines
            // LayoutUpdated event handler removed to avoid nullability warning
        }

        private bool _sparklinesDrawn = false;

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
            
            // Draw sparklines after a short delay to ensure canvas has dimensions
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                DrawAllSparklines();
            };
            timer.Start();
        }


        private void DrawAllSparklines()
        {
            if (_sparklinesDrawn) return;
            
            var app = (App)Application.Current;
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                DrawSparkline(TotalProductsSparkline, GetProductCountTrend(context, 7));
                DrawSparkline(TodayRevenueSparkline, GetRevenueTrend(context, 7));
                DrawSparkline(TodayOrdersSparkline, GetOrderCountTrend(context, 7));
                _sparklinesDrawn = true;
            }
        }

        private void DashboardPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Redraw charts when window is resized
            if (RevenueChart != null && RevenueChart.Children.Count > 0)
            {
                var app = (App)Application.Current;
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                    DrawRevenueChart(context, _currentChartRange);
                }
            }
        }

        private void LoadDashboardData()
        {
            var app = (App)Application.Current;
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

                // 1. Total Products with comparison
                LoadTotalProducts(context);

                // 2. Low Stock Products
                var lowStockProducts = context.Products
                    .Where(p => p.Quantity < 5)
                    .OrderBy(p => p.Quantity)
                    .Take(5)
                    .ToList();
                LowStockList.ItemsSource = lowStockProducts;
                LowStockText.Text = context.Products.Count(p => p.Quantity < 5).ToString();

                // 3. Best Selling Products with images and progress
                LoadBestSellers(context);

                // 4. Today's Orders & Revenue with comparison
                LoadTodayMetrics(context);

                // 5. Recent Orders with status badges
                LoadRecentOrders(context);

                // 6. Draw Revenue Chart
                DrawRevenueChart(context, _currentChartRange);
            }
        }

        private void LoadTotalProducts(ShopDbContext context)
        {
            var totalProducts = context.Products.Count();
            TotalProductsText.Text = totalProducts.ToString();

            // Compare with last month - count products that existed before this month
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastMonthCount = context.Products
                .Count(); // For now, just use total count as comparison base

            if (lastMonthCount > 0)
            {
                var change = ((double)(totalProducts - lastMonthCount) / lastMonthCount) * 100;
                if (change >= 0)
                {
                    TotalProductsChangeText.Text = $"⬆ {change:F1}% vs last month";
                    TotalProductsChangeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80));
                    TotalProductsChangeText.Visibility = Visibility.Visible;
                }
                else
                {
                    TotalProductsChangeText.Text = $"⬇ {Math.Abs(change):F1}% vs last month";
                    TotalProductsChangeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54));
                    TotalProductsChangeText.Visibility = Visibility.Visible;
                }
            }

            // Sparkline will be drawn in LayoutUpdated event
        }

        private void LoadTodayMetrics(ShopDbContext context)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var lastWeek = today.AddDays(-7);

            var todayOrders = context.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < today.AddDays(1))
                .ToList();

            var yesterdayOrders = context.Orders
                .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today)
                .ToList();

            var lastWeekOrders = context.Orders
                .Where(o => o.CreatedAt >= lastWeek && o.CreatedAt < today)
                .ToList();

            // Orders Today
            TodayOrdersText.Text = todayOrders.Count.ToString();
            if (yesterdayOrders.Count > 0)
            {
                var orderChange = ((double)(todayOrders.Count - yesterdayOrders.Count) / yesterdayOrders.Count) * 100;
                if (orderChange >= 0)
                {
                    TodayOrdersChangeText.Text = $"⬆ {orderChange:F1}%";
                    TodayOrdersChangeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80));
                }
                else
                {
                    TodayOrdersChangeText.Text = $"⬇ {Math.Abs(orderChange):F1}%";
                    TodayOrdersChangeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54));
                }
            }
            // Sparklines will be drawn in LayoutUpdated event

            // Revenue Today
            var todayRevenue = todayOrders.Sum(o => o.TotalPrice);
            var yesterdayRevenue = yesterdayOrders.Sum(o => o.TotalPrice);

            TodayRevenueText.Text = $"₫{todayRevenue:N0}";

            var diff = todayRevenue - yesterdayRevenue;
            if (diff >= 0)
            {
                TodayRevenueSubText.Text = $"+₫{diff:N0} from yesterday";
                TodayRevenueSubText.Foreground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80));
            }
            else
            {
                TodayRevenueSubText.Text = $"-₫{Math.Abs(diff):N0} from yesterday";
                TodayRevenueSubText.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54));
            }

            // Percentage change
            if (yesterdayRevenue > 0)
            {
                var revenueChange = ((double)(todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100;
                if (revenueChange >= 0)
                {
                    TodayRevenueChangeText.Text = $"⬆ {revenueChange:F1}%";
                    TodayRevenueChangeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80));
                }
                else
                {
                    TodayRevenueChangeText.Text = $"⬇ {Math.Abs(revenueChange):F1}%";
                    TodayRevenueChangeText.Foreground = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54));
                }
            }
            // Sparkline will be drawn in LayoutUpdated event
        }

        private void LoadBestSellers(ShopDbContext context)
        {
            var bestSellers = context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name, oi.Product.Image })
                .Select(g => new BestSellerItem
                {
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Image = g.Key.Image
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            // Calculate progress percentage (relative to top seller)
            if (bestSellers.Any())
            {
                var maxSold = bestSellers.First().TotalSold;
                foreach (var item in bestSellers)
                {
                    item.ProgressPercentage = maxSold > 0 ? (item.TotalSold / (double)maxSold) * 100 : 0;
                }
            }

            BestSellersList.ItemsSource = bestSellers;
        }

        private void LoadRecentOrders(ShopDbContext context)
        {
            var recentOrders = context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new OrderViewModel
                {
                    Id = $"#{o.Id}",
                    CustomerName = o.CustomerName ?? "N/A",
                    CreatedAt = o.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    TotalPrice = $"₫{o.TotalPrice:N0}",
                    Status = o.Status
                })
                .ToList();

            // Set status colors
            foreach (var order in recentOrders)
            {
                switch (order.Status)
                {
                    case OrderStatus.Created:
                        order.StatusText = "Pending";
                        order.StatusBackground = new SolidColorBrush(Color.FromArgb(255, 255, 193, 7)); // Yellow
                        order.StatusForeground = new SolidColorBrush(Color.FromArgb(255, 133, 88, 0)); // Dark Yellow
                        break;
                    case OrderStatus.Paid:
                        order.StatusText = "Completed";
                        order.StatusBackground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)); // Green
                        order.StatusForeground = new SolidColorBrush(Microsoft.UI.Colors.White);
                        break;
                    case OrderStatus.Cancelled:
                        order.StatusText = "Cancelled";
                        order.StatusBackground = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54)); // Red
                        order.StatusForeground = new SolidColorBrush(Microsoft.UI.Colors.White);
                        break;
                }
            }

            RecentOrdersList.ItemsSource = recentOrders;
        }

        private List<int> GetProductCountTrend(ShopDbContext context, int days)
        {
            // Since Product doesn't have CreatedAt, we'll use a simple trend based on total count
            // This is a simplified version - in a real app, you'd track product creation dates
            var totalCount = context.Products.Count();
            var trend = new List<int>();
            for (int i = 0; i < days; i++)
            {
                // Simulate a slight variation for visualization
                trend.Add(totalCount);
            }
            return trend;
        }

        private List<int> GetOrderCountTrend(ShopDbContext context, int days)
        {
            var trend = new List<int>();
            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var count = context.Orders
                    .Where(o => o.CreatedAt >= date && o.CreatedAt < date.AddDays(1))
                    .Count();
                trend.Add(count);
            }
            return trend;
        }

        private List<int> GetRevenueTrend(ShopDbContext context, int days)
        {
            var trend = new List<int>();
            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var revenue = context.Orders
                    .Where(o => o.CreatedAt >= date && o.CreatedAt < date.AddDays(1))
                    .Sum(o => (int?)o.TotalPrice) ?? 0;
                trend.Add(revenue);
            }
            return trend;
        }

        private void DrawSparkline(Canvas canvas, List<int> values)
        {
            canvas.Children.Clear();
            if (values == null || values.Count < 2) return;

            var max = values.Max();
            var min = values.Min();
            var range = max - min;
            if (range == 0) range = 1;

            // Wait for canvas to have actual dimensions
            if (canvas.ActualWidth <= 0 || canvas.ActualHeight <= 0)
                return;

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            var points = new List<Windows.Foundation.Point>();
            for (int i = 0; i < values.Count; i++)
            {
                double x = (i / (double)(values.Count - 1)) * width;
                double normalizedValue = (values[i] - min) / (double)range;
                double y = height - (normalizedValue * height);
                points.Add(new Windows.Foundation.Point(x, y));
            }

            // Draw line
            var pointCollection = new PointCollection();
            foreach (var point in points)
            {
                pointCollection.Add(point);
            }
            var polyline = new Polyline
            {
                Points = pointCollection,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)),
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round
            };
            canvas.Children.Add(polyline);
        }

        private void DrawRevenueChart(ShopDbContext context, string range)
        {
            // Check if chart controls are initialized
            if (RevenueChart == null || ChartLabels == null || ChartGridLines == null || YAxisLabels == null)
            {
                return; // Controls not ready yet
            }

            DateTime startDate, endDate;
            string subtitle;

            var now = DateTime.Now;
            switch (range)
            {
                case "7":
                    startDate = now.AddDays(-7);
                    endDate = now;
                    subtitle = "Last 7 days";
                    break;
                case "30":
                    startDate = now.AddDays(-30);
                    endDate = now;
                    subtitle = "Last 30 days";
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);
                    endDate = now;
                    subtitle = "This year (by month)";
                    break;
                default: // month
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = now;
                    subtitle = "This month";
                    break;
            }

            if (ChartSubtitleText != null)
            {
                ChartSubtitleText.Text = $"Daily revenue - {subtitle}";
            }

            Dictionary<string, int> dailyRevenue;
            Dictionary<string, int> dailyOrders;

            if (range == "year")
            {
                // Group by month
                dailyRevenue = new Dictionary<string, int>();
                dailyOrders = new Dictionary<string, int>();
                for (int month = 1; month <= now.Month; month++)
                {
                    var monthStart = new DateTime(now.Year, month, 1);
                    var monthEnd = month == now.Month ? now : monthStart.AddMonths(1);
                    var key = monthStart.ToString("MMM");
                    dailyRevenue[key] = context.Orders
                        .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd)
                        .Sum(o => (int?)o.TotalPrice) ?? 0;
                    dailyOrders[key] = context.Orders
                        .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd)
                        .Count();
                }
            }
            else
            {
                // Group by day
                dailyRevenue = new Dictionary<string, int>();
                dailyOrders = new Dictionary<string, int>();
                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    var nextDate = currentDate.AddDays(1);
                    var key = currentDate.ToString("dd/MM");
                    dailyRevenue[key] = context.Orders
                        .Where(o => o.CreatedAt >= currentDate && o.CreatedAt < nextDate)
                        .Sum(o => (int?)o.TotalPrice) ?? 0;
                    dailyOrders[key] = context.Orders
                        .Where(o => o.CreatedAt >= currentDate && o.CreatedAt < nextDate)
                        .Count();
                    currentDate = nextDate;
                }
            }

            // Clear previous chart
            RevenueChart.Children.Clear();
            ChartLabels.Children.Clear();
            ChartGridLines.Children.Clear();
            YAxisLabels.Children.Clear();

            if (!dailyRevenue.Any())
            {
                if (ChartSummaryText != null)
                {
                    ChartSummaryText.Text = "No data available for the selected period";
                }
                return;
            }

            double chartWidth = RevenueChart.ActualWidth > 0 ? RevenueChart.ActualWidth : 1000;
            double chartHeight = RevenueChart.ActualHeight > 0 ? RevenueChart.ActualHeight : 280;

            var keys = dailyRevenue.Keys.ToList();
            int maxRevenue = dailyRevenue.Values.Max();
            int maxOrders = dailyOrders.Values.Max();
            if (maxRevenue == 0) maxRevenue = 1;
            if (maxOrders == 0) maxOrders = 1;

            double barWidth = Math.Max(20, (chartWidth - 60) / keys.Count - 4);
            double spacing = 4;
            double xOffset = 30;

            // Draw grid lines
            for (int i = 0; i <= 5; i++)
            {
                double y = (i / 5.0) * (chartHeight - 40) + 10;
                var line = new Line
                {
                    X1 = xOffset,
                    Y1 = y,
                    X2 = chartWidth - 20,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(51, 0, 0, 0)),
                    StrokeThickness = 1
                };
                ChartGridLines.Children.Add(line);

                // Y-axis label - calculate spacing to distribute evenly
                var yValue = maxRevenue - (i * maxRevenue / 5);
                double labelSpacing = (chartHeight - 40) / 5;
                var yLabel = new TextBlock
                {
                    Text = FormatNumber(yValue),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = labelSpacing,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                YAxisLabels.Children.Add(yLabel);
            }

            // Draw bars and line
            var orderPoints = new List<Windows.Foundation.Point>();
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                int revenue = dailyRevenue[key];
                int orders = dailyOrders[key];

                double x = xOffset + i * (barWidth + spacing);
                double barHeight = (revenue / (double)maxRevenue) * (chartHeight - 40);

                // Revenue bar
                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(2, barHeight),
                    Fill = new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)),
                    RadiusX = 2,
                    RadiusY = 2
                };

                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, chartHeight - barHeight - 20);
                RevenueChart.Children.Add(bar);

                // Tooltip
                var tooltip = new ToolTip
                {
                    Content = $"Date: {key}\nRevenue: ₫{revenue:N0}\nOrders: {orders}"
                };
                ToolTipService.SetToolTip(bar, tooltip);

                // Order count line point
                double orderY = chartHeight - 20 - ((orders / (double)maxOrders) * (chartHeight - 40));
                orderPoints.Add(new Windows.Foundation.Point(x + barWidth / 2, orderY));

                // X-axis label
                var label = new TextBlock
                {
                    Text = key,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
                    Width = barWidth + spacing,
                    TextAlignment = TextAlignment.Center
                };
                ChartLabels.Children.Add(label);
            }

            // Draw order count line
            if (orderPoints.Count > 1)
            {
                var pointCollection = new PointCollection();
                foreach (var point in orderPoints)
                {
                    pointCollection.Add(point);
                }
                var polyline = new Polyline
                {
                    Points = pointCollection,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0)),
                    StrokeThickness = 3,
                    StrokeLineJoin = PenLineJoin.Round
                };
                RevenueChart.Children.Add(polyline);
            }

            // Update summary
            int totalRevenue = dailyRevenue.Values.Sum();
            int totalOrders = dailyOrders.Values.Sum();
            ChartSummaryText.Text = $"Total: ₫{totalRevenue:N0} from {totalOrders} orders | Avg: ₫{(totalOrders > 0 ? totalRevenue / totalOrders : 0):N0} per order";
        }

        private string FormatNumber(int value)
        {
            if (value >= 1000000)
                return $"{value / 1000000.0:F1}M";
            if (value >= 1000)
                return $"{value / 1000.0:F1}k";
            return value.ToString();
        }

        private void ChartDateRangeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChartDateRangeCombo?.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _currentChartRange = tag;
                var app = (App)Application.Current;
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                    DrawRevenueChart(context, _currentChartRange);
                }
            }
        }

        private void LowStockAction_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new MenuFlyout();
            
            var viewAllItem = new MenuFlyoutItem
            {
                Text = "View All Low Stock",
                Icon = new FontIcon { Glyph = "\uE8A5" }
            };
            viewAllItem.Click += ViewAllLowStock_Click;
            
            var quickRestockItem = new MenuFlyoutItem
            {
                Text = "Quick Restock",
                Icon = new FontIcon { Glyph = "\uE896" }
            };
            quickRestockItem.Click += QuickRestock_Click;
            
            flyout.Items.Add(viewAllItem);
            flyout.Items.Add(quickRestockItem);
            
            flyout.ShowAt(LowStockActionButton);
        }

        private void ViewAllLowStock_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Products page with low stock filter
            if (Frame != null)
            {
                Frame.Navigate(typeof(ProductsPage));
            }
        }

        private void QuickRestock_Click(object sender, RoutedEventArgs e)
        {
            // Could open a quick restock dialog here
        }
    }
}
