using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
        private string _selectedQuickPeriod = "Month";

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
            // Always set ToDate to today first
            var today = DateTime.Now.Date;
            _toDate = today;
            ToDatePicker.Date = new DateTimeOffset(today);
            
            // Set default to "This Month"
            SetQuickPeriod("Month");
            UpdateQuickPeriodButtons();
            
            this.UpdateLayout();
            await Task.Delay(100);
            await LoadAllData();
        }

        private async void ReportsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await Task.Delay(100);
            await LoadCharts();
        }

        private void SetQuickPeriod(string period)
        {
            _selectedQuickPeriod = period;
            var today = DateTime.Now.Date;
            
            switch (period)
            {
                case "Today":
                    _fromDate = today;
                    _toDate = today;
                    break;
                case "Week":
                    // Start of week (Monday)
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    _fromDate = today.AddDays(-diff);
                    _toDate = today;
                    break;
                case "Month":
                    _fromDate = new DateTime(today.Year, today.Month, 1);
                    _toDate = today;
                    break;
                case "Year":
                    _fromDate = new DateTime(today.Year, 1, 1);
                    _toDate = today;
                    break;
            }

            // Update date pickers
            if (FromDatePicker != null && _fromDate.HasValue)
                FromDatePicker.Date = new DateTimeOffset(_fromDate.Value);
            if (ToDatePicker != null && _toDate.HasValue)
                ToDatePicker.Date = new DateTimeOffset(_toDate.Value);

            // Update date range text
            if (DateRangeText != null)
            {
                DateRangeText.Text = $"{_fromDate:dd/MM} - {_toDate:dd/MM/yyyy}";
            }
        }

        private void UpdateQuickPeriodButtons()
        {
            var buttons = new[] { TodayButton, WeekButton, MonthButton, YearButton };
            var periods = new[] { "Today", "Week", "Month", "Year" };

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    bool isSelected = periods[i] == _selectedQuickPeriod;
                    buttons[i].Background = new SolidColorBrush(
                        isSelected ? GetColorFromHex("#EFF6FF") : GetColorFromHex("#F1F5F9"));
                    buttons[i].Foreground = new SolidColorBrush(
                        isSelected ? GetColorFromHex("#3B82F6") : GetColorFromHex("#475569"));
                }
            }
        }

        private async void QuickPeriod_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string period)
            {
                SetQuickPeriod(period);
                UpdateQuickPeriodButtons();
                await LoadAllData();
            }
        }

        private async Task LoadAllData()
        {
            await LoadKPIs();
            await LoadCharts();
            await LoadTopProducts();
            await LoadOrdersByStatus();
        }

        private async Task LoadKPIs()
        {
            if (!_fromDate.HasValue || !_toDate.HasValue) return;

            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

                // Calculate KPIs
                int totalRevenue = orders.Sum(o => o.TotalPrice);
                int totalProfit = 0;
                int totalProductsSold = 0;

                foreach (var order in orders)
                {
                    foreach (var item in order.Items)
                    {
                        totalProductsSold += item.Quantity;
                        int importCost = (item.Product?.ImportPrice ?? 0) * item.Quantity;
                        totalProfit += item.TotalPrice - importCost;
                    }
                }

                int totalOrders = orders.Count;
                double avgOrderValue = totalOrders > 0 ? (double)totalRevenue / totalOrders : 0;
                double profitMargin = totalRevenue > 0 ? (double)totalProfit / totalRevenue * 100 : 0;
                double avgProductsPerOrder = totalOrders > 0 ? (double)totalProductsSold / totalOrders : 0;

                // Update UI
                TotalRevenueText.Text = $"₫{totalRevenue:N0}";
                TotalProfitText.Text = $"₫{totalProfit:N0}";
                TotalOrdersText.Text = totalOrders.ToString();
                ProductsSoldText.Text = totalProductsSold.ToString();
                
                ProfitMarginText.Text = $"{profitMargin:F1}% profit margin";
                AvgOrderValueText.Text = $"₫{avgOrderValue:N0} avg. value";
                AvgProductsPerOrderText.Text = $"{avgProductsPerOrder:F1} avg. per order";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading KPIs: {ex}");
            }
        }

        private async Task LoadTopProducts()
        {
            if (!_fromDate.HasValue || !_toDate.HasValue) return;

            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

                var productSales = new Dictionary<int, (string Name, int Quantity, int Revenue)>();

                foreach (var order in orders)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.Product != null)
                        {
                            if (!productSales.ContainsKey(item.ProductId))
                                productSales[item.ProductId] = (item.Product.Name, 0, 0);

                            var current = productSales[item.ProductId];
                            productSales[item.ProductId] = (current.Name, current.Quantity + item.Quantity, current.Revenue + item.TotalPrice);
                        }
                    }
                }

                var topProducts = productSales
                    .OrderByDescending(p => p.Value.Revenue)
                    .Take(5)
                    .Select((p, index) => new TopProductModel
                    {
                        Rank = (index + 1).ToString(),
                        ProductName = p.Value.Name,
                        Quantity = $"{p.Value.Quantity} sold",
                        Revenue = $"₫{p.Value.Revenue:N0}"
                    })
                    .ToList();

                TopProductsList.ItemsSource = topProducts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top products: {ex}");
            }
        }

        private async Task LoadOrdersByStatus()
        {
            if (!_fromDate.HasValue || !_toDate.HasValue) return;

            try
            {
                var orders = await _context.Orders
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

                var statusCounts = orders.GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                OrderStatusPanel.Children.Clear();

                var statusConfigs = new[]
                {
                    (Status: OrderStatus.Created, Name: "Created", BgColor: "#DBEAFE", FgColor: "#1D4ED8"),
                    (Status: OrderStatus.Paid, Name: "Paid", BgColor: "#DCFCE7", FgColor: "#15803D"),
                    (Status: OrderStatus.Cancelled, Name: "Cancelled", BgColor: "#FEE2E2", FgColor: "#DC2626")
                };

                int total = orders.Count;

                foreach (var config in statusConfigs)
                {
                    int count = statusCounts.GetValueOrDefault(config.Status, 0);
                    double percentage = total > 0 ? (double)count / total * 100 : 0;

                    var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

                    // Status badge
                    var badge = new Border
                    {
                        Background = new SolidColorBrush(GetColorFromHex(config.BgColor)),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8, 4, 8, 4),
                        Margin = new Thickness(0, 0, 12, 0)
                    };
                    badge.Child = new TextBlock
                    {
                        Text = config.Name,
                        FontSize = 12,
                        FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                        Foreground = new SolidColorBrush(GetColorFromHex(config.FgColor))
                    };
                    Grid.SetColumn(badge, 0);
                    row.Children.Add(badge);

                    // Progress bar
                    var progressContainer = new Border
                    {
                        Background = new SolidColorBrush(GetColorFromHex("#F1F5F9")),
                        CornerRadius = new CornerRadius(4),
                        Height = 8,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var progress = new Border
                    {
                        Background = new SolidColorBrush(GetColorFromHex(config.FgColor)),
                        CornerRadius = new CornerRadius(4),
                        Width = Math.Max(0, percentage * 2), // Scale to max 200px
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    progressContainer.Child = progress;
                    Grid.SetColumn(progressContainer, 1);
                    row.Children.Add(progressContainer);

                    // Count text
                    var countText = new TextBlock
                    {
                        Text = $"{count} ({percentage:F0}%)",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(GetColorFromHex("#64748B")),
                        Margin = new Thickness(12, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(countText, 2);
                    row.Children.Add(countText);

                    OrderStatusPanel.Children.Add(row);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders by status: {ex}");
            }
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
            _selectedQuickPeriod = ""; // Clear quick period selection
            UpdateQuickPeriodButtons();
            await LoadAllData();
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
                ProductSalesChart.UpdateLayout();
                await Task.Delay(50);

                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

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

                var sortedKeys = salesData.Keys.OrderBy(k => k).ToList();
                if (!sortedKeys.Any())
                {
                    ProductSalesSummaryText.Text = "No data";
                    ProductSalesChart.Children.Clear();
                    ProductSalesLabels.Children.Clear();
                    return;
                }

                ProductSalesChart.Children.Clear();
                ProductSalesLabels.Children.Clear();

                double chartWidth = GetChartWidth(ProductSalesChart);
                double chartHeight = GetChartHeight(ProductSalesChart);

                ProductSalesChart.Width = chartWidth;
                ProductSalesChart.Height = chartHeight;

                int maxQuantity = salesData.Values.Max();
                if (maxQuantity == 0) maxQuantity = 1;

                double pointSpacing = Math.Max(20, chartWidth / (sortedKeys.Count + 1));
                double baseY = chartHeight - 40;

                // Draw area fill
                var points = new List<Windows.Foundation.Point>();
                for (int i = 0; i < sortedKeys.Count; i++)
                {
                    string key = sortedKeys[i];
                    int quantity = salesData[key];
                    double x = (i + 1) * pointSpacing;
                    double y = baseY - ((quantity / (double)maxQuantity) * (chartHeight - 80));
                    points.Add(new Windows.Foundation.Point(x, y));
                }

                // Draw gradient area under line
                if (points.Count > 1)
                {
                    var areaPoints = new Microsoft.UI.Xaml.Media.PointCollection();
                    areaPoints.Add(new Windows.Foundation.Point(points[0].X, baseY));
                    foreach (var p in points)
                        areaPoints.Add(p);
                    areaPoints.Add(new Windows.Foundation.Point(points[points.Count - 1].X, baseY));

                    var areaPolygon = new Microsoft.UI.Xaml.Shapes.Polygon
                    {
                        Points = areaPoints,
                        Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 59, 130, 246))
                    };
                    ProductSalesChart.Children.Add(areaPolygon);
                }

                // Draw lines
                for (int i = 0; i < points.Count - 1; i++)
                {
                    var line = new Microsoft.UI.Xaml.Shapes.Line
                    {
                        X1 = points[i].X,
                        Y1 = points[i].Y,
                        X2 = points[i + 1].X,
                        Y2 = points[i + 1].Y,
                        Stroke = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246)),
                        StrokeThickness = 3
                    };
                    ProductSalesChart.Children.Add(line);
                }

                // Draw points
                foreach (var point in points)
                {
                    var ellipse = new Microsoft.UI.Xaml.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246)),
                        Stroke = new SolidColorBrush(Microsoft.UI.Colors.White),
                        StrokeThickness = 2
                    };
                    Microsoft.UI.Xaml.Controls.Canvas.SetLeft(ellipse, point.X - 5);
                    Microsoft.UI.Xaml.Controls.Canvas.SetTop(ellipse, point.Y - 5);
                    ProductSalesChart.Children.Add(ellipse);
                }

                // Labels
                AddChartLabels(sortedKeys, pointSpacing, ProductSalesLabels);

                int totalSales = salesData.Values.Sum();
                ProductSalesSummaryText.Text = $"{totalSales} units sold";
            }
            catch (Exception ex)
            {
                ProductSalesSummaryText.Text = "Error";
                System.Diagnostics.Debug.WriteLine($"Error in DrawProductSalesChart: {ex}");
            }
        }

        private async Task DrawRevenueProfitChart()
        {
            if (!_fromDate.HasValue || !_toDate.HasValue) return;

            try
            {
                RevenueProfitChart.UpdateLayout();
                await Task.Delay(50);

                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

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
                        int importCost = (item.Product?.ImportPrice ?? 0) * item.Quantity;
                        profitData[periodKey] += item.TotalPrice - importCost;
                    }
                }

                var sortedKeys = revenueData.Keys.OrderBy(k => k).ToList();
                if (!sortedKeys.Any())
                {
                    RevenueProfitChart.Children.Clear();
                    RevenueProfitLabels.Children.Clear();
                    return;
                }

                RevenueProfitChart.Children.Clear();
                RevenueProfitLabels.Children.Clear();

                double chartWidth = GetChartWidth(RevenueProfitChart);
                double chartHeight = GetChartHeight(RevenueProfitChart);

                RevenueProfitChart.Width = chartWidth;
                RevenueProfitChart.Height = chartHeight;

                int maxValue = Math.Max(revenueData.Values.Max(), profitData.Values.Any() ? profitData.Values.Max() : 0);
                if (maxValue == 0) maxValue = 1;

                double barWidth = Math.Max(16, (chartWidth - 100) / (sortedKeys.Count * 3));
                double spacing = barWidth / 4;

                for (int i = 0; i < sortedKeys.Count; i++)
                {
                    string key = sortedKeys[i];
                    int revenue = revenueData[key];
                    int profit = profitData.GetValueOrDefault(key, 0);

                    double x = (i * barWidth * 3) + 50;

                    // Revenue bar
                    double revenueHeight = (revenue / (double)maxValue) * (chartHeight - 80);
                    var revenueBar = new Microsoft.UI.Xaml.Shapes.Rectangle
                    {
                        Width = barWidth - spacing,
                        Height = Math.Max(2, revenueHeight),
                        Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246)),
                        RadiusX = 4,
                        RadiusY = 4
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
                        Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 34, 197, 94)),
                        RadiusX = 4,
                        RadiusY = 4
                    };
                    Microsoft.UI.Xaml.Controls.Canvas.SetLeft(profitBar, x + barWidth);
                    Microsoft.UI.Xaml.Controls.Canvas.SetTop(profitBar, chartHeight - profitHeight - 40);
                    RevenueProfitChart.Children.Add(profitBar);
                }

                AddChartLabels(sortedKeys, barWidth * 3, RevenueProfitLabels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawRevenueProfitChart: {ex}");
            }
        }

        private double GetChartWidth(FrameworkElement chart)
        {
            var parent = chart.Parent as FrameworkElement;
            if (parent != null && parent.ActualWidth > 0)
                return parent.ActualWidth;
            return 800;
        }

        private double GetChartHeight(FrameworkElement chart)
        {
            var parent = chart.Parent as FrameworkElement;
            if (parent != null && parent.ActualHeight > 0)
                return parent.ActualHeight;
            return 280;
        }

        private void AddChartLabels(List<string> keys, double spacing, StackPanel labelPanel)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (i % Math.Max(1, keys.Count / 8) == 0 || i == keys.Count - 1)
                {
                    var label = new TextBlock
                    {
                        Text = FormatPeriodLabel(keys[i]),
                        FontSize = 11,
                        Foreground = new SolidColorBrush(GetColorFromHex("#64748B")),
                        Width = spacing * Math.Max(1, keys.Count / 8),
                        TextAlignment = TextAlignment.Center
                    };
                    labelPanel.Children.Add(label);
                }
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
                return day.ToString("dd/MM");
            else if (_periodType == "Week")
                return key.Replace("-W", " W");
            else if (_periodType == "Month" && DateTime.TryParse(key + "-01", out DateTime month))
                return month.ToString("MM/yy");
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

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_fromDate.HasValue || !_toDate.HasValue)
            {
                await ShowMessageDialog("Error", "Please select a date range first.");
                return;
            }

            try
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                savePicker.SuggestedFileName = $"SalesReport_{_fromDate:yyyyMMdd}_{_toDate:yyyyMMdd}";

                // Initialize with window handle
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file == null) return;

                // Gather data
                var orders = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date && o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToListAsync();

                // Prepare summary data
                int totalRevenue = orders.Sum(o => o.TotalPrice);
                int totalProfit = 0;
                int totalProductsSold = 0;

                foreach (var order in orders)
                {
                    foreach (var item in order.Items)
                    {
                        totalProductsSold += item.Quantity;
                        int importCost = (item.Product?.ImportPrice ?? 0) * item.Quantity;
                        totalProfit += item.TotalPrice - importCost;
                    }
                }

                // Create export data structure
                var summaryData = new List<object>
                {
                    new { Metric = "Report Period", Value = $"{_fromDate:dd/MM/yyyy} - {_toDate:dd/MM/yyyy}" },
                    new { Metric = "Total Revenue", Value = $"{totalRevenue:N0}₫" },
                    new { Metric = "Total Profit", Value = $"{totalProfit:N0}₫" },
                    new { Metric = "Total Orders", Value = orders.Count.ToString() },
                    new { Metric = "Total Products Sold", Value = totalProductsSold.ToString() },
                    new { Metric = "Average Order Value", Value = $"{(orders.Count > 0 ? totalRevenue / orders.Count : 0):N0}₫" },
                    new { Metric = "Profit Margin", Value = $"{(totalRevenue > 0 ? (double)totalProfit / totalRevenue * 100 : 0):F1}%" }
                };

                var ordersData = orders.Select(o => new
                {
                    OrderId = o.Id,
                    Date = o.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    Status = o.Status.ToString(),
                    ItemCount = o.Items.Count,
                    TotalPrice = o.TotalPrice
                }).ToList();

                // Export using MiniExcel
                var sheets = new Dictionary<string, object>
                {
                    ["Summary"] = summaryData,
                    ["Orders"] = ordersData
                };

                await MiniExcelLibs.MiniExcel.SaveAsAsync(file.Path, sheets);

                await ShowMessageDialog("Export Successful", $"Report exported to:\n{file.Path}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
                await ShowMessageDialog("Export Error", ex.Message);
            }
        }

        private async Task ShowMessageDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private static Windows.UI.Color GetColorFromHex(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }
    }

    public class TopProductModel
    {
        public string Rank { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string Quantity { get; set; } = "";
        public string Revenue { get; set; } = "";
    }
}
