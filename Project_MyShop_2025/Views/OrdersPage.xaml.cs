using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Extensions.DependencyInjection;
using Project_MyShop_2025.Core.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Project_MyShop_2025.Views
{
    public sealed partial class OrdersPage : Page
    {
        private readonly ShopDbContext _context;
        private List<Order> _allOrders = new();
        private List<Order> _filteredOrders = new();
        private List<StatusFilterItem> _statusFilters = new();
        
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private OrderStatus? _selectedStatus = null;
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private string _searchText = "";
        private string _sortBy = "DateDesc";
        private IPrintService _printService;
        private Project_MyShop_2025.Core.Services.Interfaces.IAutoSaveService _autoSaveService;
        private ICustomerService _customerService;
        private IPromotionService _promotionService;

        public OrdersPage()
        {
            this.InitializeComponent();
            
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            var connectionString = Project_MyShop_2025.Core.Data.DatabasePathHelper.GetConnectionString();
            optionsBuilder.UseSqlite(connectionString);
            _context = new ShopDbContext(optionsBuilder.Options);

            this.Loaded += OrdersPage_Loaded;
            
            // Resolve PrintService
            if (Application.Current is App app)
            {
                _printService = app.Services.GetService<IPrintService>();
                _autoSaveService = app.Services.GetService<Project_MyShop_2025.Core.Services.Interfaces.IAutoSaveService>();
                _customerService = app.Services.GetService<ICustomerService>();
                _promotionService = app.Services.GetService<IPromotionService>();
            }
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Load page size from settings
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var pageSizeSetting = localSettings.Values["OrdersPerPage"] as string ?? "20";
            if (int.TryParse(pageSizeSetting, out int pageSize))
            {
                _pageSize = pageSize;
            }

            // Update the page size combo box to match
            foreach (ComboBoxItem item in PageSizeComboBox.Items)
            {
                if (item.Tag?.ToString() == _pageSize.ToString())
                {
                    PageSizeComboBox.SelectedItem = item;
                    break;
                }
            }

            LoadStatusPills();
            await LoadOrders();
        }

        private void LoadStatusPills()
        {
            _statusFilters = new List<StatusFilterItem>
            {
                new StatusFilterItem { Status = null, Name = "All", IsSelected = true },
                new StatusFilterItem { Status = OrderStatus.Created, Name = "Created", IsSelected = false },
                new StatusFilterItem { Status = OrderStatus.Paid, Name = "Paid", IsSelected = false },
                new StatusFilterItem { Status = OrderStatus.Cancelled, Name = "Cancelled", IsSelected = false }
            };

            UpdateStatusPills();
        }

        private void UpdateStatusPills()
        {
            StatusPillsPanel.Children.Clear();
            
            foreach (var statusItem in _statusFilters)
            {
                var isSelected = statusItem.Status == _selectedStatus;
                
                // Determine color based on status
                string bgColor, fgColor, borderColor;
                if (isSelected)
                {
                    switch (statusItem.Status)
                    {
                        case OrderStatus.Created:
                            bgColor = "#DBEAFE"; fgColor = "#1D4ED8"; borderColor = "#3B82F6";
                            break;
                        case OrderStatus.Paid:
                            bgColor = "#DCFCE7"; fgColor = "#15803D"; borderColor = "#22C55E";
                            break;
                        case OrderStatus.Cancelled:
                            bgColor = "#FEE2E2"; fgColor = "#DC2626"; borderColor = "#EF4444";
                            break;
                        default: // All
                            bgColor = "#3B82F6"; fgColor = "#FFFFFF"; borderColor = "#3B82F6";
                            break;
                    }
                }
                else
                {
                    bgColor = "#F1F5F9"; fgColor = "#475569"; borderColor = "#E2E8F0";
                }
                
                var pill = new Button
                {
                    Content = statusItem.Name,
                    Background = new SolidColorBrush(GetColorFromHex(bgColor)),
                    Foreground = new SolidColorBrush(GetColorFromHex(fgColor)),
                    BorderBrush = new SolidColorBrush(GetColorFromHex(borderColor)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(16, 8, 16, 8),
                    Tag = statusItem.Status,
                    FontSize = 13,
                    FontWeight = isSelected ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
                };
                
                pill.Click += StatusPill_Click;
                StatusPillsPanel.Children.Add(pill);
            }
        }

        private void StatusPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _selectedStatus = button.Tag as OrderStatus?;
                UpdateStatusPills();
                ApplyFilters();
            }
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

        private async Task LoadOrders()
        {
            _allOrders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredOrders = _allOrders.ToList();

            // Status filter
            if (_selectedStatus.HasValue)
            {
                _filteredOrders = _filteredOrders
                    .Where(o => o.Status == _selectedStatus.Value)
                    .ToList();
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                _filteredOrders = _filteredOrders
                    .Where(o => (o.CustomerName?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                               o.Id.ToString().Contains(_searchText))
                    .ToList();
            }

            // Date range filter
            if (_fromDate.HasValue)
            {
                _filteredOrders = _filteredOrders
                    .Where(o => o.CreatedAt.Date >= _fromDate.Value.Date)
                    .ToList();
            }

            if (_toDate.HasValue)
            {
                _filteredOrders = _filteredOrders
                    .Where(o => o.CreatedAt.Date <= _toDate.Value.Date)
                    .ToList();
            }

            // Amount range filter
            if (MinAmountBox != null && double.TryParse(MinAmountBox.Text, out double minAmount))
            {
                _filteredOrders = _filteredOrders.Where(o => o.TotalPrice >= minAmount).ToList();
            }

            if (MaxAmountBox != null && double.TryParse(MaxAmountBox.Text, out double maxAmount))
            {
                _filteredOrders = _filteredOrders.Where(o => o.TotalPrice <= maxAmount).ToList();
            }

            // Apply sorting
            _filteredOrders = _sortBy switch
            {
                "DateAsc" => _filteredOrders.OrderBy(o => o.CreatedAt).ToList(),
                "AmountDesc" => _filteredOrders.OrderByDescending(o => o.TotalPrice).ToList(),
                "AmountAsc" => _filteredOrders.OrderBy(o => o.TotalPrice).ToList(),
                _ => _filteredOrders.OrderByDescending(o => o.CreatedAt).ToList() // DateDesc
            };

            // Update pagination
            _currentPage = 1;
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            var totalItems = _filteredOrders.Count;
            
            if (_pageSize == -1)
            {
                _totalPages = 1;
                DisplayOrders(_filteredOrders);
            }
            else
            {
                _totalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
                if (_totalPages == 0) _totalPages = 1;
                
                if (_currentPage > _totalPages)
                    _currentPage = _totalPages;

                var pagedOrders = _filteredOrders
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                DisplayOrders(pagedOrders);
            }

            // Update UI
            if (PageInfoText != null)
                PageInfoText.Text = $"Page {_currentPage} of {_totalPages}";
            
            if (PrevPageButton != null)
                PrevPageButton.IsEnabled = _currentPage > 1;
            
            if (NextPageButton != null)
                NextPageButton.IsEnabled = _currentPage < _totalPages;
            
            if (ResultsCountText != null)
                ResultsCountText.Text = $"{_filteredOrders.Count} order{(_filteredOrders.Count != 1 ? "s" : "")}";
        }

        private void DisplayOrders(List<Order> orders)
        {
            var displayOrders = orders.Select(o => 
            {
                // Determine status colors
                string statusBg, statusFg;
                switch (o.Status)
                {
                    case OrderStatus.Created:
                        statusBg = "#DBEAFE"; statusFg = "#1D4ED8";
                        break;
                    case OrderStatus.Paid:
                        statusBg = "#DCFCE7"; statusFg = "#15803D";
                        break;
                    case OrderStatus.Cancelled:
                        statusBg = "#FEE2E2"; statusFg = "#DC2626";
                        break;
                    default:
                        statusBg = "#F1F5F9"; statusFg = "#64748B";
                        break;
                }

                return new OrderDisplayModel
                {
                    OrderId = $"#{o.Id}",
                    CustomerName = o.CustomerName ?? "Guest Customer",
                    CreatedAt = o.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    TotalPriceFormatted = $"₫{o.TotalPrice:N0}",
                    Status = o.Status,
                    StatusText = o.Status.ToString(),
                    StatusBackground = new SolidColorBrush(GetColorFromHex(statusBg)),
                    StatusForeground = new SolidColorBrush(GetColorFromHex(statusFg)),
                    ItemsCount = $"{o.Items.Count} item{(o.Items.Count != 1 ? "s" : "")}",
                    ItemsSummary = o.Items.Count > 0 
                        ? string.Join(", ", o.Items.Take(2).Select(i => i.Product?.Name ?? "Unknown"))
                        : "No items",
                    Order = o
                };
            }).ToList();

            OrdersListView.ItemsSource = displayOrders;
        }

        // Event Handlers
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text?.Trim() ?? "";
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                _sortBy = item.Tag.ToString() ?? "DateDesc";
                if (_allOrders.Any())
                {
                    ApplyFilters();
                }
            }
        }

        private void DateFilter_Changed(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            _fromDate = FromDatePicker.Date?.DateTime;
            _toDate = ToDatePicker.Date?.DateTime;
            ApplyFilters();
        }

        private void ClearDateFilter_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.Date = null;
            ToDatePicker.Date = null;
            _fromDate = null;
            _toDate = null;
            ApplyFilters();
        }

        private void AmountFilter_Changed(object sender, TextChangedEventArgs e)
        {
            // Wait for apply button or apply immediately if desired (let's wait for apply/enter or just implicit)
            // Implementation choice: Apply immediately for text boxes usually annoying, but let's stick to "Apply" button pattern for Advanced
            // Actually, in ProductsPage we wait for Apply. Here we have Apply button in flyout.
        }

        private void HighValueFilter_Click(object sender, RoutedEventArgs e)
        {
            MinAmountBox.Text = "1000000";
            MaxAmountBox.Text = "";
            ApplyFilters();
        }

        private void CancelledFilter_Click(object sender, RoutedEventArgs e)
        {
            // This overlaps with status pills but provides a quick way inside the advanced menu
            // We can set the status pill programmatically
            _selectedStatus = OrderStatus.Cancelled;
            UpdateStatusPills();
            ApplyFilters();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            FromDatePicker.Date = null;
            ToDatePicker.Date = null;
            MinAmountBox.Text = "";
            MaxAmountBox.Text = "";
            _selectedStatus = null;
            SortComboBox.SelectedIndex = 0; // Default Newest
            UpdateStatusPills();
            ApplyFilters();
        }

        private void ApplyAdvancedFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
            }
        }

        private void PageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeComboBox != null && PageSizeComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                _pageSize = int.Parse(item.Tag.ToString() ?? "20");
                UpdatePagination();
            }
        }

        private void OrderCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246));
                border.BorderThickness = new Thickness(2);
            }
        }

        private void OrderCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 226, 232, 240));
                border.BorderThickness = new Thickness(1);
            }
        }

        private async void ViewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string orderIdStr)
            {
                var orderId = int.Parse(orderIdStr.Replace("#", ""));
                await ShowOrderDetails(orderId);
            }
        }

        private async void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string orderIdStr)
            {
                var orderId = int.Parse(orderIdStr.Replace("#", ""));
                await ShowEditOrderDialog(orderId);
            }
        }

        private async void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string orderIdStr)
            {
                var orderId = int.Parse(orderIdStr.Replace("#", ""));
                await ShowUpdateStatusDialog(orderId);
            }
        }

        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string orderIdStr)
            {
                var orderId = int.Parse(orderIdStr.Replace("#", ""));
                await DeleteOrder(orderId);
            }
        }

        private async void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCreateOrderDialog();
        }



        // Dialog Methods
        private async Task ShowOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return;

            var dialog = new ContentDialog
            {
                Title = $"Order #{order.Id}",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot,
                MaxWidth = 600
            };

            var content = new StackPanel { Spacing = 16 };

            // Order Info
            var orderInfo = new StackPanel { Spacing = 8 };
            orderInfo.Children.Add(new TextBlock { Text = "Order Information", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 16 });
            orderInfo.Children.Add(new TextBlock { Text = $"Order ID: #{order.Id}" });
            orderInfo.Children.Add(new TextBlock { Text = $"Date: {order.CreatedAt:dd/MM/yyyy HH:mm:ss}" });
            orderInfo.Children.Add(new TextBlock { Text = $"Status: {order.Status}" });
            orderInfo.Children.Add(new TextBlock { Text = $"Total: ₫{order.TotalPrice:N0}", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 18, Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246)) });
            content.Children.Add(orderInfo);

            // Customer Info
            if (!string.IsNullOrWhiteSpace(order.CustomerName) || !string.IsNullOrWhiteSpace(order.CustomerPhone) || !string.IsNullOrWhiteSpace(order.CustomerAddress))
            {
                var customerInfo = new StackPanel { Spacing = 8 };
                customerInfo.Children.Add(new TextBlock { Text = "Customer Information", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 16, Margin = new Thickness(0, 8, 0, 0) });
                if (!string.IsNullOrWhiteSpace(order.CustomerName))
                    customerInfo.Children.Add(new TextBlock { Text = $"Name: {order.CustomerName}" });
                if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
                    customerInfo.Children.Add(new TextBlock { Text = $"Phone: {order.CustomerPhone}" });
                if (!string.IsNullOrWhiteSpace(order.CustomerAddress))
                    customerInfo.Children.Add(new TextBlock { Text = $"Address: {order.CustomerAddress}", TextWrapping = TextWrapping.Wrap });
                content.Children.Add(customerInfo);
            }

            // Order Items
            var itemsInfo = new StackPanel { Spacing = 8 };
            itemsInfo.Children.Add(new TextBlock { Text = "Order Items", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 16, Margin = new Thickness(0, 8, 0, 0) });

            if (order.Items.Any())
            {
                var itemsStack = new StackPanel { Spacing = 8 };
                foreach (var item in order.Items)
                {
                    var itemPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
                    itemPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"{item.Product?.Name ?? "Unknown"} x{item.Quantity}",
                        FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                        Width = 300
                    });
                    itemPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"₫{item.Price:N0}",
                        Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 100, 116, 139)),
                        Margin = new Thickness(16, 0, 0, 0),
                        Width = 100
                    });
                    itemPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"₫{item.TotalPrice:N0}",
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(16, 0, 0, 0)
                    });
                    itemsStack.Children.Add(itemPanel);
                }
                itemsInfo.Children.Add(new ScrollViewer { Content = itemsStack, MaxHeight = 300 });
            }
            else
            {
                itemsInfo.Children.Add(new TextBlock { Text = "No items in this order.", Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 148, 163, 184)) });
            }
            content.Children.Add(itemsInfo);

            dialog.Content = new ScrollViewer { Content = content, MaxHeight = 500 };
            await dialog.ShowAsync();
        }

        private async void PrintOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string orderIdStr && int.TryParse(orderIdStr, out int orderId))
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null && _printService != null)
                {
                    if (_printService.IsPrintingSupported)
                    {
                        await _printService.PrintOrderAsync(order);
                    }
                    else
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "Printing Not Supported",
                            Content = "Printing is not supported on this device.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await dialog.ShowAsync();
                    }
                }
            }
        }

        private async Task ShowEditOrderDialog(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return;

            var dialog = new ContentDialog
            {
                Title = "Edit Order",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var nameBox = new TextBox { Header = "Customer Name", Text = order.CustomerName ?? "", Margin = new Thickness(0, 8, 0, 0) };
            var phoneBox = new TextBox { Header = "Customer Phone", Text = order.CustomerPhone ?? "", Margin = new Thickness(0, 8, 0, 0) };
            var addressBox = new TextBox { Header = "Customer Address", Text = order.CustomerAddress ?? "", Margin = new Thickness(0, 8, 0, 0), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(nameBox);
            content.Children.Add(phoneBox);
            content.Children.Add(addressBox);

            dialog.Content = new ScrollViewer { Content = content, MaxHeight = 400 };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                order.CustomerName = nameBox.Text;
                order.CustomerPhone = phoneBox.Text;
                order.CustomerAddress = addressBox.Text;

                await _context.SaveChangesAsync();
                await LoadOrders();
            }
        }

        private async Task ShowUpdateStatusDialog(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            var dialog = new ContentDialog
            {
                Title = "Update Order Status",
                PrimaryButtonText = "Update",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var statusCombo = new ComboBox { Header = "Status", Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            statusCombo.Items.Add(new ComboBoxItem { Content = "Created", Tag = OrderStatus.Created });
            statusCombo.Items.Add(new ComboBoxItem { Content = "Paid", Tag = OrderStatus.Paid });
            statusCombo.Items.Add(new ComboBoxItem { Content = "Cancelled", Tag = OrderStatus.Cancelled });
            statusCombo.SelectedItem = statusCombo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (OrderStatus)i.Tag == order.Status);

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(new TextBlock { Text = $"Order #{order.Id}", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) });
            content.Children.Add(statusCombo);

            dialog.Content = content;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (statusCombo.SelectedItem is ComboBoxItem selectedItem)
                {
                    order.Status = (OrderStatus)selectedItem.Tag;
                    await _context.SaveChangesAsync();
                    await LoadOrders();
                }
            }
        }

        private async Task DeleteOrder(int orderId)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = "Are you sure you want to delete this order? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    await LoadOrders();
                }
            }
        }

        private async Task ShowCreateOrderDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Create New Order",
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                MaxWidth = 800
            };

            var rootGrid = new Grid();
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Left: Customer
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }); // Right: Products
            
            // --- Left Column: Customer & Info ---
            var leftPanel = new StackPanel { Spacing = 12 };
            Grid.SetColumn(leftPanel, 0);

            // Customer Search
            var searchBox = new AutoSuggestBox 
            { 
                Header = "Search Customer (Name/Phone)", 
                PlaceholderText = "Type to search...",
                QueryIcon = new SymbolIcon(Symbol.Find),
                DisplayMemberPath = "Name" // Assumes binding to Customer object
            };
            
            var nameBox = new TextBox { Header = "Customer Name" };
            var phoneBox = new TextBox { Header = "Phone Number" };
            var addressBox = new TextBox { Header = "Address", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 60 };

            Customer? selectedCustomer = null;

            searchBox.TextChanged += async (s, args) => 
            {
                if ((int)args.Reason == 0) // AutoSuggestBoxTextChangedReason.UserInput
                {
                    if (string.IsNullOrWhiteSpace(s.Text)) 
                    {
                        s.ItemsSource = null;
                        return;
                    }
                    if (s.Text.Length < 2) return;

                    var criteria = new Project_MyShop_2025.Core.Services.Interfaces.CustomerSearchCriteria 
                    { 
                        Keyword = s.Text, 
                        PageSize = 5 
                    };
                    var result = await _customerService.GetCustomersAsync(criteria);
                    s.ItemsSource = result.Items;
                }
            };

            searchBox.SuggestionChosen += (s, args) => 
            {
                if (args.SelectedItem is Customer c)
                {
                    selectedCustomer = c;
                    nameBox.Text = c.Name;
                    phoneBox.Text = c.Phone;
                    addressBox.Text = c.Address ?? "";
                }
            };
            
            // Clear selected customer if name/phone manually edited to avoid mismatch
            TextChangedEventHandler clearCustomerHandler = (s, e) => { /* Optional: Logic to clear selectedCustomer if mismatch */ };
            nameBox.TextChanged += clearCustomerHandler;

            leftPanel.Children.Add(searchBox);
            leftPanel.Children.Add(new MenuFlyoutSeparator { Margin = new Thickness(0, 10, 0, 10)});
            leftPanel.Children.Add(nameBox);
            leftPanel.Children.Add(phoneBox);
            leftPanel.Children.Add(addressBox);


            // --- Right Column: Products & Cart ---
            var rightPanel = new StackPanel { Spacing = 12 };
            Grid.SetColumn(rightPanel, 2);

            // Product Selection
            var productsHeader = new TextBlock { Text = "Add Products", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
            var productsList = new ListView { Height = 200, SelectionMode = ListViewSelectionMode.Multiple, BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray), BorderThickness = new Thickness(1) };
            
            // Load products
            var products = await _context.Products.ToListAsync();
            productsList.ItemsSource = products;
            
            var xaml = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                <Grid Padding=""8"">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width=""*"" />
                        <ColumnDefinition Width=""Auto"" />
                    </Grid.ColumnDefinitions>
                    <TextBlock Text=""{Binding Name}"" FontWeight=""Medium"" />
                    <TextBlock Grid.Column=""1"" Text=""{Binding Price, StringFormat='{}{0:N0}₫'}"" Foreground=""Gray"" Margin=""10,0,0,0""/>
                </Grid>
            </DataTemplate>";
            productsList.ItemTemplate = (DataTemplate)XamlReader.Load(xaml);

            // Controls for adding
            var controlsPanel = new Grid();
            controlsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Qty
            controlsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            controlsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Price
            controlsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            controlsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Add Btn

            var quantityBox = new TextBox { Header = "Qty", Text = "1" };
            Grid.SetColumn(quantityBox, 0);
            var priceBox = new TextBox { Header = "Price Override", PlaceholderText = "Default" };
            Grid.SetColumn(priceBox, 2);
            var addItemButton = new Button { Content = "Add Item", VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0,0,0,1) };
            Grid.SetColumn(addItemButton, 4);

            controlsPanel.Children.Add(quantityBox);
            controlsPanel.Children.Add(priceBox);
            controlsPanel.Children.Add(addItemButton);

            // Cart Items
            var cartHeader = new TextBlock { Text = "Order Items", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0,10,0,0) };
            var itemsListView = new ListView { Height = 150, Background = new SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke), BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray), BorderThickness = new Thickness(1) };

            var selectedProducts = new System.Collections.ObjectModel.ObservableCollection<(Product product, int quantity, int price)>();
            itemsListView.ItemsSource = selectedProducts; // Needs a template or string projection? We'll maintain a parallel list for display string? 
            // Better: use an ObservableCollection of ViewModel/DTO
            // For quick impl, let's use list of strings for display
            var cartDisplayList = new System.Collections.ObjectModel.ObservableCollection<string>();
            itemsListView.ItemsSource = cartDisplayList;

            addItemButton.Click += (s, e) =>
            {
                if (productsList.SelectedItems != null && productsList.SelectedItems.Count > 0)
                {
                    foreach (Product selectedProduct in productsList.SelectedItems)
                    {
                         // Check if already added? (Simple logic: allow duplicates or sum quantity?)
                         // Let's create new line for simplicity
                        var qty = int.TryParse(quantityBox.Text, out int q) ? q : 1;
                        var price = int.TryParse(priceBox.Text, out int p) ? p : selectedProduct.Price;
                        
                        selectedProducts.Add((selectedProduct, qty, price));
                        cartDisplayList.Add($"{selectedProduct.Name} x{qty} @ ₫{price:N0} = ₫{qty*price:N0}");
                    }
                    
                    // Trigger draft save
                     if (_autoSaveService != null) { _ = SaveOrderDraft(); }
                }
            };

            // Promotion Section
            var promoHeader = new TextBlock { Text = "Promotion", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0,10,0,0) };
            var promoPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var promoCodeBox = new TextBox { PlaceholderText = "Code", Width = 120 };
            var applyPromoBtn = new Button { Content = "Apply" };
            var promoStatusText = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
            
            Promotion? selectedPromotion = null;

            applyPromoBtn.Click += async (s, e) => {
                var code = promoCodeBox.Text;
                if (string.IsNullOrWhiteSpace(code)) return;
                
                int subTotal = selectedProducts.Sum(x => x.quantity * x.price);
                var valid = await _promotionService.ValidateCodeAsync(code, subTotal);
                
                if (valid != null)
                {
                    selectedPromotion = valid;
                    int discount = await _promotionService.CalculateDiscountAsync(code, subTotal);
                    promoStatusText.Text = $"Valid: -{discount:N0}₫";
                    promoStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
                }
                else
                {
                    selectedPromotion = null;
                    promoStatusText.Text = "Invalid Code";
                    promoStatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
                }
            };

            promoPanel.Children.Add(promoCodeBox);
            promoPanel.Children.Add(applyPromoBtn);
            promoPanel.Children.Add(promoStatusText);

            rightPanel.Children.Add(productsHeader);
            rightPanel.Children.Add(productsList);
            rightPanel.Children.Add(controlsPanel);
            rightPanel.Children.Add(cartHeader);
            rightPanel.Children.Add(itemsListView);
            rightPanel.Children.Add(promoHeader);
            rightPanel.Children.Add(promoPanel);
            
            // Assemble Grid
            rootGrid.Children.Add(leftPanel);
            rootGrid.Children.Add(rightPanel);
            
            dialog.Content = rootGrid;

            // Load Draft Logic (Adapted)
             async Task SaveOrderDraft()
            {
                if (_autoSaveService == null) return;
                var draft = new OrderDraft
                {
                    CustomerName = nameBox.Text,
                    CustomerPhone = phoneBox.Text,
                    CustomerAddress = addressBox.Text,
                    Items = selectedProducts.Select(sp => new OrderItemDraft 
                    { 
                        ProductId = sp.product.Id, 
                        Quantity = sp.quantity,
                        Price = sp.price
                    }).ToList()
                };
                await _autoSaveService.SaveDraftAsync("Draft_Order", draft);
            }
            // (Initial Load Draft logic could be added here similar to before, checking _autoSaveService)
            if (_autoSaveService != null)
            {
                 var draft = await _autoSaveService.LoadDraftAsync<OrderDraft>("Draft_Order");
                 if (draft != null)
                 {
                     nameBox.Text = draft.CustomerName ?? "";
                     phoneBox.Text = draft.CustomerPhone ?? "";
                     addressBox.Text = draft.CustomerAddress ?? "";
                     if (draft.Items != null)
                     {
                         foreach(var item in draft.Items)
                         {
                             var p = products.FirstOrDefault(x => x.Id == item.ProductId);
                             if (p != null) {
                                 selectedProducts.Add((p, item.Quantity, item.Price));
                                 cartDisplayList.Add($"{p.Name} x{item.Quantity} @ ₫{item.Price:N0} = ₫{item.Quantity*item.Price:N0}");
                             }
                         }
                     }
                 }
                 
                TextChangedEventHandler textHandler = async (s, e) => { await SaveOrderDraft(); };
                nameBox.TextChanged += textHandler;
                phoneBox.TextChanged += textHandler;
                addressBox.TextChanged += textHandler;
            }


            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && selectedProducts.Any())
            {
                // Create Order
                var newOrder = new Order
                {
                    CustomerName = nameBox.Text,
                    CustomerPhone = phoneBox.Text,
                    CustomerAddress = addressBox.Text,
                    CreatedAt = DateTime.Now,
                    Status = OrderStatus.Created
                };

                // Link Customer
                if (selectedCustomer != null)
                {
                    newOrder.CustomerId = selectedCustomer.Id;
                }
                else if (!string.IsNullOrWhiteSpace(newOrder.CustomerPhone))
                {
                    // Try find by phone or create
                    var existing = await _customerService.GetCustomerByPhoneAsync(newOrder.CustomerPhone);
                    if (existing != null)
                    {
                        newOrder.CustomerId = existing.Id;
                    }
                    else
                    {
                        // Auto-create customer?
                        var newC = new Customer 
                        { 
                            Name = newOrder.CustomerName ?? "Guest", 
                            Phone = newOrder.CustomerPhone,
                            Address = newOrder.CustomerAddress
                        };
                        var created = await _customerService.CreateCustomerAsync(newC);
                        newOrder.CustomerId = created.Id;
                    }
                }

                // Add Items
                foreach (var sp in selectedProducts)
                {
                    newOrder.Items.Add(new OrderItem
                    {
                        ProductId = sp.product.Id,
                        Product = null, // EF will resolve
                        Quantity = sp.quantity,
                        Price = sp.price,
                        TotalPrice = sp.quantity * sp.price
                    });
                }
                
                // Promotion
                if (selectedPromotion != null)
                {
                    newOrder.PromotionId = selectedPromotion.Id;
                    newOrder.Promotion = selectedPromotion; // For immediate calculation
                }
                
                newOrder.CalculateTotals();
                // Ensure PromotionId is set effectively for DB
                newOrder.Promotion = null; // Avoid EF trying to add new Promotion
                if (selectedPromotion != null) newOrder.PromotionId = selectedPromotion.Id;

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();
                
                // Apply Promotion usage count
                if (selectedPromotion != null)
                {
                    await _promotionService.ApplyPromotionAsync(selectedPromotion.Id);
                }

                // Clear draft
                 if (_autoSaveService != null) await _autoSaveService.ClearDraftAsync("Draft_Order");

                await LoadOrders();
            }
        }
    }

    // Helper classes
    public class StatusFilterItem
    {
        public OrderStatus? Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class OrderDisplayModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string TotalPriceFormatted { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public SolidColorBrush StatusBackground { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.Gray);
        public SolidColorBrush StatusForeground { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.White);
        public string ItemsCount { get; set; } = string.Empty;
        public string ItemsSummary { get; set; } = string.Empty;
        public Order Order { get; set; } = null!;
    }

    public class OrderDraft
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public List<OrderItemDraft> Items { get; set; } = new();
    }

    public class OrderItemDraft
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
