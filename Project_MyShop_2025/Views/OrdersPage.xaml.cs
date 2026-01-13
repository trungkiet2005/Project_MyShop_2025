using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

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

        public OrdersPage()
        {
            this.InitializeComponent();
            
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            optionsBuilder.UseSqlite("Data Source=myshop.db");
            _context = new ShopDbContext(optionsBuilder.Options);

            this.Loaded += OrdersPage_Loaded;
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStatusFilters();
            await LoadOrders();
        }

        private async Task LoadStatusFilters()
        {
            _statusFilters = new List<StatusFilterItem>
            {
                new StatusFilterItem { Status = null, Name = "All Status", IsSelected = true }
            };

            _statusFilters.Add(new StatusFilterItem { Status = OrderStatus.Created, Name = "Created", IsSelected = false });
            _statusFilters.Add(new StatusFilterItem { Status = OrderStatus.Paid, Name = "Paid", IsSelected = false });
            _statusFilters.Add(new StatusFilterItem { Status = OrderStatus.Cancelled, Name = "Cancelled", IsSelected = false });

            StatusFilterList.ItemsSource = _statusFilters;
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
                ResultsCountText.Text = $"Showing {_filteredOrders.Count} order{(_filteredOrders.Count != 1 ? "s" : "")}";
        }

        private void DisplayOrders(List<Order> orders)
        {
            var displayOrders = orders.Select(o => new OrderDisplayModel
            {
                OrderId = $"#{o.Id}",
                CustomerName = o.CustomerName ?? "Guest",
                CreatedAt = o.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                TotalPriceFormatted = $"₫{o.TotalPrice:N0}",
                Status = o.Status,
                StatusText = o.Status.ToString(),
                StatusColor = o.Status switch
                {
                    OrderStatus.Created => "#2196F3",
                    OrderStatus.Paid => "#4CAF50",
                    OrderStatus.Cancelled => "#F44336",
                    _ => "#9E9E9E"
                },
                ItemsSummary = o.Items.Count > 0 
                    ? $"{o.Items.Count} item(s) - {string.Join(", ", o.Items.Take(3).Select(i => $"{i.Product?.Name ?? "Unknown"} x{i.Quantity}"))}"
                    : "No items",
                Order = o
            }).ToList();

            OrdersListView.ItemsSource = displayOrders;
        }

        // Event Handlers
        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterList.SelectedItem is StatusFilterItem selected)
            {
                _selectedStatus = selected.Status;
                
                // Update visual selection
                foreach (var status in _statusFilters)
                {
                    status.IsSelected = status.Status == selected.Status;
                }
                
                ApplyFilters();
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
            orderInfo.Children.Add(new TextBlock { Text = "Order Information", FontWeight = new Windows.UI.Text.FontWeight(600), FontSize = 16 });
            orderInfo.Children.Add(new TextBlock { Text = $"Order ID: #{order.Id}" });
            orderInfo.Children.Add(new TextBlock { Text = $"Date: {order.CreatedAt:dd/MM/yyyy HH:mm:ss}" });
            orderInfo.Children.Add(new TextBlock { Text = $"Status: {order.Status}" });
            orderInfo.Children.Add(new TextBlock { Text = $"Total: ₫{order.TotalPrice:N0}", FontWeight = new Windows.UI.Text.FontWeight(600), FontSize = 18, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243)) });
            content.Children.Add(orderInfo);

            // Customer Info
            if (!string.IsNullOrWhiteSpace(order.CustomerName) || !string.IsNullOrWhiteSpace(order.CustomerPhone) || !string.IsNullOrWhiteSpace(order.CustomerAddress))
            {
                var customerInfo = new StackPanel { Spacing = 8 };
                customerInfo.Children.Add(new TextBlock { Text = "Customer Information", FontWeight = new Windows.UI.Text.FontWeight(600), FontSize = 16, Margin = new Thickness(0, 8, 0, 0) });
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
            itemsInfo.Children.Add(new TextBlock { Text = "Order Items", FontWeight = new Windows.UI.Text.FontWeight(600), FontSize = 16, Margin = new Thickness(0, 8, 0, 0) });

            if (order.Items.Any())
            {
                var itemsStack = new StackPanel { Spacing = 8 };
                foreach (var item in order.Items)
                {
                    var itemPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
                    itemPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"{item.Product?.Name ?? "Unknown"} x{item.Quantity}",
                        FontWeight = new Windows.UI.Text.FontWeight(500),
                        Width = 300
                    });
                    itemPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"₫{item.Price:N0}",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 102, 102, 102)),
                        Margin = new Thickness(16, 0, 0, 0),
                        Width = 100
                    });
                    itemPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"₫{item.TotalPrice:N0}",
                        FontWeight = new Windows.UI.Text.FontWeight(600),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(16, 0, 0, 0)
                    });
                    itemsStack.Children.Add(itemPanel);
                }
                itemsInfo.Children.Add(new ScrollViewer { Content = itemsStack, MaxHeight = 300 });
            }
            else
            {
                itemsInfo.Children.Add(new TextBlock { Text = "No items in this order.", Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 158, 158, 158)) });
            }
            content.Children.Add(itemsInfo);

            dialog.Content = new ScrollViewer { Content = content, MaxHeight = 500 };
            await dialog.ShowAsync();
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
            content.Children.Add(new TextBlock { Text = $"Order #{order.Id}", FontWeight = new Windows.UI.Text.FontWeight(600), Margin = new Thickness(0, 0, 0, 8) });
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
                MaxWidth = 700
            };

            var nameBox = new TextBox { Header = "Customer Name", Margin = new Thickness(0, 8, 0, 0) };
            var phoneBox = new TextBox { Header = "Customer Phone", Margin = new Thickness(0, 8, 0, 0) };
            var addressBox = new TextBox { Header = "Customer Address", Margin = new Thickness(0, 8, 0, 0), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };

            var productsList = new ListView { Header = "Select Products", MaxHeight = 300, SelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode.Multiple };
            var products = await _context.Products.ToListAsync();
            productsList.ItemsSource = products;

            var quantityBox = new TextBox { Header = "Quantity", Text = "1", Margin = new Thickness(0, 8, 0, 0) };
            var priceBox = new TextBox { Header = "Unit Price", Text = "0", Margin = new Thickness(0, 8, 0, 0) };

            var selectedProducts = new List<(Product product, int quantity, int price)>();

            productsList.SelectionChanged += (s, e) =>
            {
                // Handle product selection
            };

            var addItemButton = new Button { Content = "Add Item", Margin = new Thickness(0, 8, 0, 0) };
            var itemsListView = new ListView { Header = "Order Items", MaxHeight = 200 };

            addItemButton.Click += (s, e) =>
            {
                if (productsList.SelectedItem is Product selectedProduct)
                {
                    var qty = int.TryParse(quantityBox.Text, out int q) ? q : 1;
                    var price = int.TryParse(priceBox.Text, out int p) ? p : selectedProduct.Price;
                    selectedProducts.Add((selectedProduct, qty, price));
                    itemsListView.ItemsSource = selectedProducts.Select(sp => $"{sp.product.Name} x{sp.quantity} @ ₫{sp.price:N0}").ToList();
                }
            };

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(nameBox);
            content.Children.Add(phoneBox);
            content.Children.Add(addressBox);
            content.Children.Add(productsList);
            content.Children.Add(quantityBox);
            content.Children.Add(priceBox);
            content.Children.Add(addItemButton);
            content.Children.Add(itemsListView);

            dialog.Content = new ScrollViewer { Content = content, MaxHeight = 500 };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && selectedProducts.Any())
            {
                var order = new Order
                {
                    CustomerName = nameBox.Text,
                    CustomerPhone = phoneBox.Text,
                    CustomerAddress = addressBox.Text,
                    CreatedAt = DateTime.Now,
                    Status = OrderStatus.Created,
                    TotalPrice = 0
                };

                foreach (var (product, quantity, price) in selectedProducts)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        Price = price,
                        TotalPrice = quantity * price
                    };
                    order.Items.Add(orderItem);
                    order.TotalPrice += orderItem.TotalPrice;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
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
        public string StatusColor { get; set; } = string.Empty;
        public string ItemsSummary { get; set; } = string.Empty;
        public Order Order { get; set; } = null!;
    }
}

