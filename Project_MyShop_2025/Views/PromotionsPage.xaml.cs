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

namespace Project_MyShop_2025.Views
{
    public sealed partial class PromotionsPage : Page
    {
        private readonly IPromotionService _promotionService;
        private readonly ICategoryService _categoryService;
        private string _currentFilter = "All";

        public PromotionsPage()
        {
            this.InitializeComponent();
            
            var app = (App)Application.Current;
            _promotionService = app.Services.GetRequiredService<IPromotionService>();
            _categoryService = app.Services.GetRequiredService<ICategoryService>();
            
            Loaded += PromotionsPage_Loaded;
        }

        private async void PromotionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPromotionsAsync();
        }

        private async Task LoadPromotionsAsync()
        {
            try
            {
                var allPromotions = await _promotionService.GetAllPromotionsAsync();
                var now = DateTime.Now;
                
                var filtered = _currentFilter switch
                {
                    "Active" => allPromotions.Where(p => p.IsValid).ToList(),
                    "Expired" => allPromotions.Where(p => !p.IsValid || p.EndDate < now).ToList(),
                    _ => allPromotions
                };

                var displayModels = filtered.Select(p => new PromotionDisplayModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Type = p.Type,
                    DiscountValue = p.DiscountValue,
                    FreeQuantity = p.FreeQuantity,
                    MinOrderAmount = p.MinOrderAmount,
                    MaxDiscountAmount = p.MaxDiscountAmount,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    UsageLimit = p.UsageLimit,
                    UsedCount = p.UsedCount
                }).ToList();

                PromotionsGrid.ItemsSource = displayModels;
                ResultsCountText.Text = $"{displayModels.Count} promotion{(displayModels.Count != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading promotions: {ex.Message}");
            }
        }

        private async void FilterTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filter)
            {
                _currentFilter = filter;
                
                // Update tab styles
                AllTab.Background = new SolidColorBrush(filter == "All" ? ColorHelper.FromArgb(255, 59, 130, 246) : ColorHelper.FromArgb(255, 241, 245, 249));
                AllTab.Foreground = new SolidColorBrush(filter == "All" ? Colors.White : ColorHelper.FromArgb(255, 71, 85, 105));
                
                ActiveTab.Background = new SolidColorBrush(filter == "Active" ? ColorHelper.FromArgb(255, 59, 130, 246) : ColorHelper.FromArgb(255, 241, 245, 249));
                ActiveTab.Foreground = new SolidColorBrush(filter == "Active" ? Colors.White : ColorHelper.FromArgb(255, 71, 85, 105));
                
                ExpiredTab.Background = new SolidColorBrush(filter == "Expired" ? ColorHelper.FromArgb(255, 59, 130, 246) : ColorHelper.FromArgb(255, 241, 245, 249));
                ExpiredTab.Foreground = new SolidColorBrush(filter == "Expired" ? Colors.White : ColorHelper.FromArgb(255, 71, 85, 105));

                await LoadPromotionsAsync();
            }
        }

        private async void AddPromotionButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowPromotionDialog(null);
        }

        private async void EditPromotion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int promotionId)
            {
                var promotion = await _promotionService.GetPromotionByIdAsync(promotionId);
                if (promotion != null)
                {
                    await ShowPromotionDialog(promotion);
                }
            }
        }

        private async void DeletePromotion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int promotionId)
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Promotion",
                    Content = "Are you sure you want to delete this promotion? This action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await _promotionService.DeletePromotionAsync(promotionId);
                    await LoadPromotionsAsync();
                }
            }
        }

        private void PromotionCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 217, 119, 6)); // Amber
            }
        }

        private void PromotionCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 226, 232, 240));
            }
        }

        private async Task ShowPromotionDialog(Promotion? promotion)
        {
            var isEdit = promotion != null;
            
            var codeBox = new TextBox { PlaceholderText = "SUMMER2025", Text = promotion?.Code ?? "", Margin = new Thickness(0, 0, 0, 12) };
            var nameBox = new TextBox { PlaceholderText = "Summer Sale", Text = promotion?.Name ?? "", Margin = new Thickness(0, 0, 0, 12) };
            var descBox = new TextBox { PlaceholderText = "Description...", Text = promotion?.Description ?? "", Margin = new Thickness(0, 0, 0, 12) };
            
            var typeCombo = new ComboBox { Width = 200, Margin = new Thickness(0, 0, 0, 12) };
            typeCombo.Items.Add(new ComboBoxItem { Content = "Percentage (%)", Tag = PromotionType.Percentage });
            typeCombo.Items.Add(new ComboBoxItem { Content = "Fixed Amount (₫)", Tag = PromotionType.FixedAmount });
            typeCombo.Items.Add(new ComboBoxItem { Content = "Buy X Get Y", Tag = PromotionType.BuyXGetY });
            typeCombo.SelectedIndex = promotion != null ? (int)promotion.Type : 0;

            var valueBox = new NumberBox { PlaceholderText = "10", Value = (double)(promotion?.DiscountValue ?? 0), Minimum = 0, Margin = new Thickness(0, 0, 0, 12) };
            var minOrderBox = new NumberBox { PlaceholderText = "100000", Value = promotion?.MinOrderAmount ?? 0, Minimum = 0, Margin = new Thickness(0, 0, 0, 12) };
            var maxDiscountBox = new NumberBox { PlaceholderText = "500000", Value = promotion?.MaxDiscountAmount ?? 0, Minimum = 0, Margin = new Thickness(0, 0, 0, 12) };
            var usageLimitBox = new NumberBox { PlaceholderText = "100", Value = promotion?.UsageLimit ?? 0, Minimum = 0, Margin = new Thickness(0, 0, 0, 12) };
            
            var startDatePicker = new CalendarDatePicker { Date = promotion?.StartDate ?? DateTime.Now, Margin = new Thickness(0, 0, 0, 12) };
            var endDatePicker = new CalendarDatePicker { Date = promotion?.EndDate ?? DateTime.Now.AddDays(30), Margin = new Thickness(0, 0, 0, 12) };
            
            var isActiveToggle = new ToggleSwitch { IsOn = promotion?.IsActive ?? true, Margin = new Thickness(0, 0, 0, 12) };

            var content = new StackPanel { Width = 400 };
            var scrollViewer = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 500 };
            
            content.Children.Add(new TextBlock { Text = "Promotion Code *", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(codeBox);
            content.Children.Add(new TextBlock { Text = "Name *", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(nameBox);
            content.Children.Add(new TextBlock { Text = "Description", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(descBox);
            content.Children.Add(new TextBlock { Text = "Discount Type", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(typeCombo);
            content.Children.Add(new TextBlock { Text = "Discount Value", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(valueBox);
            content.Children.Add(new TextBlock { Text = "Min Order Amount (₫)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(minOrderBox);
            content.Children.Add(new TextBlock { Text = "Max Discount (₫)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(maxDiscountBox);
            content.Children.Add(new TextBlock { Text = "Usage Limit (0 = unlimited)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(usageLimitBox);
            content.Children.Add(new TextBlock { Text = "Start Date", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(startDatePicker);
            content.Children.Add(new TextBlock { Text = "End Date", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(endDatePicker);
            content.Children.Add(new TextBlock { Text = "Active", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            content.Children.Add(isActiveToggle);

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Edit Promotion" : "Create New Promotion",
                Content = scrollViewer,
                PrimaryButtonText = isEdit ? "Save" : "Create",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(codeBox.Text) || string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    await ShowErrorDialog("Code and Name are required.");
                    return;
                }

                var type = (typeCombo.SelectedItem as ComboBoxItem)?.Tag as PromotionType? ?? PromotionType.Percentage;

                if (isEdit)
                {
                    promotion!.Code = codeBox.Text.Trim().ToUpper();
                    promotion.Name = nameBox.Text.Trim();
                    promotion.Description = string.IsNullOrWhiteSpace(descBox.Text) ? null : descBox.Text.Trim();
                    promotion.Type = type;
                    promotion.DiscountValue = (decimal)valueBox.Value;
                    promotion.MinOrderAmount = minOrderBox.Value > 0 ? (int)minOrderBox.Value : null;
                    promotion.MaxDiscountAmount = maxDiscountBox.Value > 0 ? (int)maxDiscountBox.Value : null;
                    promotion.UsageLimit = usageLimitBox.Value > 0 ? (int)usageLimitBox.Value : null;
                    promotion.StartDate = startDatePicker.Date?.DateTime ?? DateTime.Now;
                    promotion.EndDate = endDatePicker.Date?.DateTime ?? DateTime.Now.AddDays(30);
                    promotion.IsActive = isActiveToggle.IsOn;
                    await _promotionService.UpdatePromotionAsync(promotion);
                }
                else
                {
                    var newPromotion = new Promotion
                    {
                        Code = codeBox.Text.Trim().ToUpper(),
                        Name = nameBox.Text.Trim(),
                        Description = string.IsNullOrWhiteSpace(descBox.Text) ? null : descBox.Text.Trim(),
                        Type = type,
                        DiscountValue = (decimal)valueBox.Value,
                        MinOrderAmount = minOrderBox.Value > 0 ? (int)minOrderBox.Value : null,
                        MaxDiscountAmount = maxDiscountBox.Value > 0 ? (int)maxDiscountBox.Value : null,
                        UsageLimit = usageLimitBox.Value > 0 ? (int)usageLimitBox.Value : null,
                        StartDate = startDatePicker.Date?.DateTime ?? DateTime.Now,
                        EndDate = endDatePicker.Date?.DateTime ?? DateTime.Now.AddDays(30),
                        IsActive = isActiveToggle.IsOn
                    };
                    await _promotionService.CreatePromotionAsync(newPromotion);
                }

                await LoadPromotionsAsync();
            }
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
    }

    public class PromotionDisplayModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public PromotionType Type { get; set; }
        public decimal DiscountValue { get; set; }
        public int? FreeQuantity { get; set; }
        public int? MinOrderAmount { get; set; }
        public int? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }

        public bool IsValid => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate && (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);

        public string TypeText => Type switch
        {
            PromotionType.Percentage => "Percentage",
            PromotionType.FixedAmount => "Fixed",
            PromotionType.BuyXGetY => "Buy X Get Y",
            _ => "Unknown"
        };

        public SolidColorBrush TypeBackground => Type switch
        {
            PromotionType.Percentage => new SolidColorBrush(ColorHelper.FromArgb(255, 16, 185, 129)), // Green
            PromotionType.FixedAmount => new SolidColorBrush(ColorHelper.FromArgb(255, 59, 130, 246)), // Blue
            PromotionType.BuyXGetY => new SolidColorBrush(ColorHelper.FromArgb(255, 168, 85, 247)), // Purple
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 100, 116, 139))
        };

        public SolidColorBrush TypeForeground => TypeBackground;

        public string DiscountDisplay => Type switch
        {
            PromotionType.Percentage => $"{DiscountValue}%",
            PromotionType.FixedAmount => $"{DiscountValue:N0}₫",
            PromotionType.BuyXGetY => $"Buy {(int)DiscountValue} Get {FreeQuantity ?? 1}",
            _ => ""
        };

        public string DiscountDescription => Type switch
        {
            PromotionType.Percentage => "off your order",
            PromotionType.FixedAmount => "discount",
            PromotionType.BuyXGetY => "free items",
            _ => ""
        };

        public Visibility MinOrderVisibility => MinOrderAmount.HasValue ? Visibility.Visible : Visibility.Collapsed;
        public string MinOrderText => $"Min order: {MinOrderAmount:N0}₫";

        public Visibility MaxDiscountVisibility => MaxDiscountAmount.HasValue && Type == PromotionType.Percentage ? Visibility.Visible : Visibility.Collapsed;
        public string MaxDiscountText => $"Max discount: {MaxDiscountAmount:N0}₫";

        public Visibility UsageLimitVisibility => UsageLimit.HasValue ? Visibility.Visible : Visibility.Collapsed;
        public string UsageLimitText => $"Used {UsedCount}/{UsageLimit} times";

        public string DateRangeText => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";

        public string StatusText => IsValid ? "Active" : (DateTime.Now > EndDate ? "Expired" : "Inactive");
        public SolidColorBrush StatusBackground => IsValid 
            ? new SolidColorBrush(ColorHelper.FromArgb(255, 220, 252, 231)) // Green
            : new SolidColorBrush(ColorHelper.FromArgb(255, 254, 226, 226)); // Red
        public SolidColorBrush StatusForeground => IsValid 
            ? new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74))
            : new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
