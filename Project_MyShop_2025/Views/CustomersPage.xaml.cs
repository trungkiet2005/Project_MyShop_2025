using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project_MyShop_2025.Views
{
    public sealed partial class CustomersPage : Page
    {
        private readonly ICustomerService _customerService;
        private readonly ApplicationDataContainer _localSettings;
        
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private string _searchText = "";
        private string _sortBy = "NameAsc";
        private bool? _activeFilter = null;
        private bool _isInitialized = false;

        public CustomersPage()
        {
            var app = (App)Application.Current;
            _customerService = app.Services.GetRequiredService<ICustomerService>();
            _localSettings = ApplicationData.Current.LocalSettings;

            this.InitializeComponent();
            
            LoadPageSizeFromSettings();
            Loaded += CustomersPage_Loaded;
        }

        private async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitialized = true;
            await LoadCustomersAsync();
        }

        private void LoadPageSizeFromSettings()
        {
            var savedPageSize = _localSettings.Values["CustomersPageSize"] as int?;
            if (savedPageSize.HasValue)
            {
                _pageSize = savedPageSize.Value;
                foreach (ComboBoxItem item in PageSizeComboBox.Items)
                {
                    if (item.Tag?.ToString() == _pageSize.ToString())
                    {
                        PageSizeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private async Task LoadCustomersAsync()
        {
            if (_customerService == null || CustomersListView == null) return;

            try
            {
                var criteria = new CustomerSearchCriteria
                {
                    Keyword = _searchText,
                    IsActive = _activeFilter,
                    SortBy = _sortBy,
                    Page = _currentPage,
                    PageSize = _pageSize == -1 ? int.MaxValue : _pageSize
                };

                var result = await _customerService.GetCustomersAsync(criteria);
                
                var displayModels = new List<CustomerDisplayModel>();
                foreach (var customer in result.Items)
                {
                    var totalSpent = await _customerService.GetTotalSpentAsync(customer.Id);
                    var orderCount = await _customerService.GetOrderCountAsync(customer.Id);
                    
                    displayModels.Add(new CustomerDisplayModel
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Phone = customer.Phone ?? "N/A",
                        Email = customer.Email ?? "",
                        Address = customer.Address ?? "",
                        LoyaltyPoints = customer.LoyaltyPoints,
                        IsActive = customer.IsActive,
                        TotalSpent = totalSpent,
                        OrderCount = orderCount,
                        CreatedAt = customer.CreatedAt
                    });
                }

                CustomersListView.ItemsSource = displayModels;
                
                _totalPages = result.TotalPages > 0 ? result.TotalPages : 1;
                UpdatePagination(result.TotalCount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
        }

        private void UpdatePagination(int totalCount)
        {
            // Null checks to prevent NullReferenceException during initialization
            if (ResultsCountText != null)
                ResultsCountText.Text = $"{totalCount} customer{(totalCount != 1 ? "s" : "")}";
            if (PageInfoText != null)
                PageInfoText.Text = $"Page {_currentPage} of {_totalPages}";
            if (PrevPageButton != null)
                PrevPageButton.IsEnabled = _currentPage > 1;
            if (NextPageButton != null)
                NextPageButton.IsEnabled = _currentPage < _totalPages;
        }

        #region Event Handlers

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text;
            _currentPage = 1;
            await LoadCustomersAsync();
        }

        private async void ActiveFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Don't process during initialization
            if (!_isInitialized) return;

            if (ActiveFilterCombo.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag?.ToString();
                _activeFilter = tag switch
                {
                    "Active" => true,
                    "Inactive" => false,
                    _ => null
                };
                _currentPage = 1;
                await LoadCustomersAsync();
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't process during initialization
            if (!_isInitialized) return;

            if (SortComboBox.SelectedItem is ComboBoxItem item)
            {
                _sortBy = item.Tag?.ToString() ?? "NameAsc";
                _currentPage = 1;
                await LoadCustomersAsync();
            }
        }

        private async void PageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't process during initialization - wait until page is fully loaded
            if (!_isInitialized) return;

            if (PageSizeComboBox.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Tag?.ToString(), out int pageSize))
                {
                    _pageSize = pageSize;
                    if (_localSettings != null)
                    {
                        _localSettings.Values["CustomersPageSize"] = _pageSize;
                    }
                    _currentPage = 1;
                    await LoadCustomersAsync();
                }
            }
        }

        private async void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadCustomersAsync();
            }
        }

        private async void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadCustomersAsync();
            }
        }

        private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCustomerDialog(null);
        }

        private async void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer != null)
                {
                    await ShowCustomerDialog(customer);
                }
            }
        }

        private async void ViewCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer != null)
                {
                    await ShowCustomerDetailsDialog(customer);
                }
            }
        }

        private async void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int customerId)
            {
                var dialog = new ContentDialog
                {
                    Title = "Deactivate Customer",
                    Content = "Are you sure you want to deactivate this customer? They will be marked as inactive but their order history will be preserved.",
                    PrimaryButtonText = "Deactivate",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await _customerService.DeleteCustomerAsync(customerId);
                    await LoadCustomersAsync();
                }
            }
        }

        private void CustomerCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74)); // Green
            }
        }

        private void CustomerCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 226, 232, 240)); // Gray
            }
        }

        #endregion

        #region Dialog Methods

        private async Task ShowCustomerDialog(Customer? customer)
        {
            var isEdit = customer != null;
            
            var nameBox = new TextBox { PlaceholderText = "Full Name *", Text = customer?.Name ?? "", Margin = new Thickness(0, 0, 0, 12) };
            var phoneBox = new TextBox { PlaceholderText = "Phone Number", Text = customer?.Phone ?? "", Margin = new Thickness(0, 0, 0, 12) };
            var emailBox = new TextBox { PlaceholderText = "Email Address", Text = customer?.Email ?? "", Margin = new Thickness(0, 0, 0, 12) };
            var addressBox = new TextBox { PlaceholderText = "Address", Text = customer?.Address ?? "", Margin = new Thickness(0, 0, 0, 12), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 80 };
            var notesBox = new TextBox { PlaceholderText = "Notes", Text = customer?.Notes ?? "", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 80 };

            var content = new StackPanel { Width = 400 };
            content.Children.Add(new TextBlock { Text = "Name *", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(nameBox);
            content.Children.Add(new TextBlock { Text = "Phone", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(phoneBox);
            content.Children.Add(new TextBlock { Text = "Email", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(emailBox);
            content.Children.Add(new TextBlock { Text = "Address", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(addressBox);
            content.Children.Add(new TextBlock { Text = "Notes", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(notesBox);

            if (isEdit)
            {
                var pointsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 12, 0, 0) };
                pointsPanel.Children.Add(new TextBlock { Text = $"Loyalty Points: {customer!.LoyaltyPoints}", VerticalAlignment = VerticalAlignment.Center });
                content.Children.Add(pointsPanel);
            }

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Edit Customer" : "Add New Customer",
                Content = content,
                PrimaryButtonText = isEdit ? "Save" : "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    await ShowErrorDialog("Name is required.");
                    return;
                }

                if (isEdit)
                {
                    customer!.Name = nameBox.Text.Trim();
                    customer.Phone = string.IsNullOrWhiteSpace(phoneBox.Text) ? null : phoneBox.Text.Trim();
                    customer.Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim();
                    customer.Address = string.IsNullOrWhiteSpace(addressBox.Text) ? null : addressBox.Text.Trim();
                    customer.Notes = string.IsNullOrWhiteSpace(notesBox.Text) ? null : notesBox.Text.Trim();
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    var newCustomer = new Customer
                    {
                        Name = nameBox.Text.Trim(),
                        Phone = string.IsNullOrWhiteSpace(phoneBox.Text) ? null : phoneBox.Text.Trim(),
                        Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim(),
                        Address = string.IsNullOrWhiteSpace(addressBox.Text) ? null : addressBox.Text.Trim(),
                        Notes = string.IsNullOrWhiteSpace(notesBox.Text) ? null : notesBox.Text.Trim()
                    };
                    await _customerService.CreateCustomerAsync(newCustomer);
                }

                await LoadCustomersAsync();
            }
        }

        private async Task ShowCustomerDetailsDialog(Customer customer)
        {
            var totalSpent = await _customerService.GetTotalSpentAsync(customer.Id);
            var orderCount = await _customerService.GetOrderCountAsync(customer.Id);

            var content = new StackPanel { Width = 400, Spacing = 8 };
            
            content.Children.Add(CreateDetailRow("Name", customer.Name));
            content.Children.Add(CreateDetailRow("Phone", customer.Phone ?? "N/A"));
            content.Children.Add(CreateDetailRow("Email", customer.Email ?? "N/A"));
            content.Children.Add(CreateDetailRow("Address", customer.Address ?? "N/A"));
            content.Children.Add(CreateDetailRow("Member Since", customer.CreatedAt.ToString("dd/MM/yyyy")));
            content.Children.Add(CreateDetailRow("Loyalty Points", customer.LoyaltyPoints.ToString("#,##0")));
            content.Children.Add(CreateDetailRow("Total Orders", orderCount.ToString()));
            content.Children.Add(CreateDetailRow("Total Spent", $"{totalSpent:N0} ₫"));
            
            if (!string.IsNullOrEmpty(customer.Notes))
            {
                content.Children.Add(new Border { Height = 1, Background = new SolidColorBrush(ColorHelper.FromArgb(255, 226, 232, 240)), Margin = new Thickness(0, 8, 0, 8) });
                content.Children.Add(new TextBlock { Text = "Notes:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                content.Children.Add(new TextBlock { Text = customer.Notes, TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 100, 116, 139)) });
            }

            var dialog = new ContentDialog
            {
                Title = "Customer Details",
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private StackPanel CreateDetailRow(string label, string value)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            row.Children.Add(new TextBlock { Text = $"{label}:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Width = 120 });
            row.Children.Add(new TextBlock { Text = value, Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 100, 116, 139)) });
            return row;
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        #endregion
    }

    public class CustomerDisplayModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public int LoyaltyPoints { get; set; }
        public bool IsActive { get; set; }
        public int TotalSpent { get; set; }
        public int OrderCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Initials => string.Join("", Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0].ToString().ToUpper()));
        public string LoyaltyPointsFormatted => LoyaltyPoints.ToString("#,##0");
        public string TotalSpentFormatted => $"{TotalSpent:N0} ₫";
        public string OrderCountText => $"{OrderCount} order{(OrderCount != 1 ? "s" : "")}";
        public Visibility EmailVisibility => string.IsNullOrEmpty(Email) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility InactiveVisibility => IsActive ? Visibility.Collapsed : Visibility.Visible;
        public SolidColorBrush StatusBackground => new SolidColorBrush(ColorHelper.FromArgb(255, 254, 226, 226));
        public SolidColorBrush StatusForeground => new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
