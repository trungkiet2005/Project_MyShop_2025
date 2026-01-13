using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ReportsPage : Page
    {
        private readonly ShopDbContext _context;
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private string _periodType = "Day";

        public ReportsPage()
        {
            this.InitializeComponent();
            
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            var connectionString = Project_MyShop_2025.Core.Data.DatabasePathHelper.GetConnectionString();
            optionsBuilder.UseSqlite(connectionString);
            _context = new ShopDbContext(optionsBuilder.Options);

            this.Loaded += ReportsPage_Loaded;
            this.SizeChanged += ReportsPage_SizeChanged;
        }

        private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set default date range from start of current month to today
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            _toDate = today;
            _fromDate = firstDayOfMonth;
            
            FromDatePicker.Date = new DateTimeOffset(firstDayOfMonth);
            ToDatePicker.Date = new DateTimeOffset(today);

            // Wait for layout to complete before loading charts
            this.UpdateLayout();
            await Task.Delay(100);
            await LoadCharts();
        }

        private async void ReportsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Redraw charts when window size changes (with delay to ensure layout is updated)
            await Task.Delay(100);
            await LoadCharts();
        }

        private async Task LoadCharts()
        {
            await DrawProductSalesChart();
            await DrawRevenueProfitChart();
        }

        private async void DateFilter_Changed(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            _fromDate = FromDatePicker.Date?.DateTime;
            _toDate = ToDatePicker.Date?.DateTime;
            await LoadCharts();
        }

        private async void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PeriodComboBox.SelectedItem is ComboBoxItem item && item.Tag is string period)
            {
                _periodType = period;
                await LoadCharts();
            }
        }

        private async Task DrawProductSalesChart()
        {
            if (!_fromDate.HasValue || !_toDate.HasValue) return;

            try
            {
                // Update layout first
                ProductSalesChart.UpdateLayout();
                await Task.Delay(50); // Small delay to ensure layout is complete

                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

                // Group sales by period
                var salesData = new Dictionary<string, int>();
                
                foreach (var order in orders)
                {
                    string periodKey = GetPeriodKey(order.CreatedAt);
                    
                    foreach (var item in order.Items)
                    {
                        if (!salesData.ContainsKey(periodKey))
                            salesData[periodKey] = 0;
                        salesData[periodKey] += item.Quantity;
                    }
                }

                // Sort by date
                var sortedKeys = salesData.Keys.OrderBy(k => k).ToList();
                if (!sortedKeys.Any())
                {
                    ProductSalesSummaryText.Text = "No sales data for selected period";
                    ProductSalesChart.Children.Clear();
                    ProductSalesLabels.Children.Clear();
                    return;
                }

                // Clear previous chart
                ProductSalesChart.Children.Clear();
                ProductSalesLabels.Children.Clear();

                // Get actual dimensions from parent Border or Grid
                double chartWidth = 0;
                double chartHeight = 0;
                
                var parentBorder = ProductSalesChart.Parent as FrameworkElement;
                if (parentBorder != null)
                {
                    chartWidth = parentBorder.ActualWidth;
                    chartHeight = parentBorder.ActualHeight;
                    
                    // If Border doesn't have size, try to get from Grid parent
                    if (chartWidth <= 0 || chartHeight <= 0)
                    {
                        var gridParent = parentBorder.Parent as FrameworkElement;
                        if (gridParent != null)
                        {
                            chartWidth = gridParent.ActualWidth > 0 ? gridParent.ActualWidth : 1000;
                            chartHeight = gridParent.ActualHeight > 0 ? gridParent.ActualHeight : 300;
                        }
                    }
                }
                
                // Fallback values if still no size
                if (chartWidth <= 0) chartWidth = 1000;
                if (chartHeight <= 0) chartHeight = 300;

                // Set explicit size on canvas to ensure it has dimensions
                ProductSalesChart.Width = chartWidth;
                ProductSalesChart.Height = chartHeight;

            int maxQuantity = salesData.Values.Max();
            if (maxQuantity == 0) maxQuantity = 1;

            double pointSpacing = Math.Max(20, chartWidth / (sortedKeys.Count + 1));
            double baseY = chartHeight - 40;

            // Draw line chart
            var points = new List<Windows.Foundation.Point>();
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                string key = sortedKeys[i];
                int quantity = salesData[key];
                double x = (i + 1) * pointSpacing;
                double y = baseY - ((quantity / (double)maxQuantity) * (chartHeight - 80));
                points.Add(new Windows.Foundation.Point(x, y));
            }

            // Draw lines connecting points
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = new Microsoft.UI.Xaml.Shapes.Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243)),
                    StrokeThickness = 3
                };
                ProductSalesChart.Children.Add(line);
            }

            // Draw points
            foreach (var point in points)
            {
                var ellipse = new Microsoft.UI.Xaml.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243)),
                    Stroke = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255)),
                    StrokeThickness = 2
                };
                Microsoft.UI.Xaml.Controls.Canvas.SetLeft(ellipse, point.X - 4);
                Microsoft.UI.Xaml.Controls.Canvas.SetTop(ellipse, point.Y - 4);
                ProductSalesChart.Children.Add(ellipse);
            }

            // Add labels
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                if (i % Math.Max(1, sortedKeys.Count / 10) == 0 || i == sortedKeys.Count - 1)
                {
                    var label = new TextBlock
                    {
                        Text = FormatPeriodLabel(sortedKeys[i]),
                        FontSize = 10,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128)),
                        Width = pointSpacing * Math.Max(1, sortedKeys.Count / 10),
                        TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center
                    };
                    ProductSalesLabels.Children.Add(label);
                }
            }

                int totalSales = salesData.Values.Sum();
                ProductSalesSummaryText.Text = $"Total products sold: {totalSales} units over {sortedKeys.Count} {_periodType.ToLower()} period(s)";
            }
            catch (Exception ex)
            {
                ProductSalesSummaryText.Text = $"Error loading chart: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in DrawProductSalesChart: {ex}");
            }
        }

        private async Task DrawRevenueProfitChart()
        {
            if (!_fromDate.HasValue || !_toDate.HasValue) return;

            try
            {
                // Update layout first
                RevenueProfitChart.UpdateLayout();
                await Task.Delay(50); // Small delay to ensure layout is complete

                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

                // Calculate revenue and profit by period
                var revenueData = new Dictionary<string, int>();
                var profitData = new Dictionary<string, int>();

                foreach (var order in orders)
                {
                    string periodKey = GetPeriodKey(order.CreatedAt);
                    
                    if (!revenueData.ContainsKey(periodKey))
                    {
                        revenueData[periodKey] = 0;
                        profitData[periodKey] = 0;
                    }

                    foreach (var item in order.Items)
                    {
                        revenueData[periodKey] += item.TotalPrice;
                        
                        // Calculate profit: Revenue - (ImportPrice * Quantity)
                        int importCost = (item.Product?.ImportPrice ?? 0) * item.Quantity;
                        profitData[periodKey] += item.TotalPrice - importCost;
                    }
                }

                var sortedKeys = revenueData.Keys.OrderBy(k => k).ToList();
                if (!sortedKeys.Any())
                {
                    RevenueProfitSummaryText.Text = "No revenue data for selected period";
                    RevenueProfitChart.Children.Clear();
                    RevenueProfitLabels.Children.Clear();
                    return;
                }

                // Clear previous chart
                RevenueProfitChart.Children.Clear();
                RevenueProfitLabels.Children.Clear();

                // Get actual dimensions from parent Border or Grid
                double chartWidth = 0;
                double chartHeight = 0;
                
                var parentBorder = RevenueProfitChart.Parent as FrameworkElement;
                if (parentBorder != null)
                {
                    chartWidth = parentBorder.ActualWidth;
                    chartHeight = parentBorder.ActualHeight;
                    
                    // If Border doesn't have size, try to get from Grid parent
                    if (chartWidth <= 0 || chartHeight <= 0)
                    {
                        var gridParent = parentBorder.Parent as FrameworkElement;
                        if (gridParent != null)
                        {
                            chartWidth = gridParent.ActualWidth > 0 ? gridParent.ActualWidth : 1000;
                            chartHeight = gridParent.ActualHeight > 0 ? gridParent.ActualHeight : 300;
                        }
                    }
                }
                
                // Fallback values if still no size
                if (chartWidth <= 0) chartWidth = 1000;
                if (chartHeight <= 0) chartHeight = 300;

                // Set explicit size on canvas to ensure it has dimensions
                RevenueProfitChart.Width = chartWidth;
                RevenueProfitChart.Height = chartHeight;

            int maxValue = Math.Max(revenueData.Values.Max(), profitData.Values.Any() ? profitData.Values.Max() : 0);
            if (maxValue == 0) maxValue = 1;

            double barWidth = Math.Max(20, (chartWidth - 100) / (sortedKeys.Count * 3));
            double spacing = barWidth / 3;

            // Draw bars
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                string key = sortedKeys[i];
                int revenue = revenueData[key];
                int profit = profitData.ContainsKey(key) ? profitData[key] : 0;

                double x = (i * barWidth * 3) + 50;

                // Revenue bar
                double revenueHeight = (revenue / (double)maxValue) * (chartHeight - 80);
                var revenueBar = new Microsoft.UI.Xaml.Shapes.Rectangle
                {
                    Width = barWidth - spacing,
                    Height = Math.Max(2, revenueHeight),
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243)), // Blue
                    RadiusX = 2,
                    RadiusY = 2
                };
                Microsoft.UI.Xaml.Controls.Canvas.SetLeft(revenueBar, x);
                Microsoft.UI.Xaml.Controls.Canvas.SetTop(revenueBar, chartHeight - revenueHeight - 40);
                RevenueProfitChart.Children.Add(revenueBar);

                // Profit bar
                double profitHeight = (profit / (double)maxValue) * (chartHeight - 80);
                var profitBar = new Microsoft.UI.Xaml.Shapes.Rectangle
                {
                    Width = barWidth - spacing,
                    Height = Math.Max(2, profitHeight),
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 76, 175, 80)), // Green
                    RadiusX = 2,
                    RadiusY = 2
                };
                Microsoft.UI.Xaml.Controls.Canvas.SetLeft(profitBar, x + barWidth);
                Microsoft.UI.Xaml.Controls.Canvas.SetTop(profitBar, chartHeight - profitHeight - 40);
                RevenueProfitChart.Children.Add(profitBar);

                // Add label
                if (i % Math.Max(1, sortedKeys.Count / 10) == 0 || i == sortedKeys.Count - 1)
                {
                    var label = new TextBlock
                    {
                        Text = FormatPeriodLabel(key),
                        FontSize = 10,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128)),
                        Width = barWidth * 3,
                        TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center
                    };
                    RevenueProfitLabels.Children.Add(label);
                }
            }

                int totalRevenue = revenueData.Values.Sum();
                int totalProfit = profitData.Values.Sum();
                RevenueProfitSummaryText.Text = $"Total Revenue: ₫{totalRevenue:N0} | Total Profit: ₫{totalProfit:N0} | Profit Margin: {(totalRevenue > 0 ? (totalProfit * 100.0 / totalRevenue):0):F1}%";
            }
            catch (Exception ex)
            {
                RevenueProfitSummaryText.Text = $"Error loading chart: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in DrawRevenueProfitChart: {ex}");
            }
        }

        private string GetPeriodKey(DateTime date)
        {
            return _periodType switch
            {
                "Day" => date.ToString("yyyy-MM-dd"),
                "Week" => $"{date.Year}-W{GetWeekOfYear(date)}",
                "Month" => date.ToString("yyyy-MM"),
                "Year" => date.Year.ToString(),
                _ => date.ToString("yyyy-MM-dd")
            };
        }

        private string FormatPeriodLabel(string key)
        {
            if (_periodType == "Day" && DateTime.TryParse(key, out DateTime day))
                return day.ToString("MM/dd");
            else if (_periodType == "Week")
                return key.Replace("-W", " W");
            else if (_periodType == "Month" && DateTime.TryParse(key + "-01", out DateTime month))
                return month.ToString("MM/yyyy");
            else if (_periodType == "Year")
                return key;
            return key;
        }

        private int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        }
    }
}

