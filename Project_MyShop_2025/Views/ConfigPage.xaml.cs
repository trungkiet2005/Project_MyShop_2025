using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ConfigPage : Page
    {
        private readonly ApplicationDataContainer _localSettings;
        private readonly Project_MyShop_2025.Core.Services.Interfaces.IDatabaseService _databaseService;

        public ConfigPage()
        {
            this.InitializeComponent();
            _localSettings = ApplicationData.Current.LocalSettings;
            
            // Resolve DatabaseService
            if (Application.Current is App app)
            {
                _databaseService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<Project_MyShop_2025.Core.Services.Interfaces.IDatabaseService>(app.Services);
            }

            LoadSettings();
        }

        private void LoadSettings()
        {
            // Display Settings
            var productsPerPage = _localSettings.Values["ProductsPerPage"] as string ?? "20";
            SelectComboBoxItem(ProductsPerPageCombo, productsPerPage);

            var ordersPerPage = _localSettings.Values["OrdersPerPage"] as string ?? "20";
            SelectComboBoxItem(OrdersPerPageCombo, ordersPerPage);

            var rememberLastPage = _localSettings.Values["RememberLastPage"] as bool? ?? true;
            RememberLastPageToggle.IsOn = rememberLastPage;

            // Database Settings
            ServerAddressBox.Text = _localSettings.Values["ServerAddress"] as string ?? "localhost";
            ServerPortBox.Text = _localSettings.Values["ServerPort"] as string ?? "5432";
            DatabaseNameBox.Text = _localSettings.Values["DatabaseName"] as string ?? "MyShop";
            UsernameBox.Text = _localSettings.Values["DbUsername"] as string ?? "postgres";
            PasswordBox.Password = _localSettings.Values["DbPassword"] as string ?? "";

            // AI Settings
            GeminiApiKeyBox.Password = _localSettings.Values["GeminiApiKey"] as string ?? "";
        }

        private void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Tag?.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        private void ProductsPerPageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsPerPageCombo?.SelectedItem is ComboBoxItem item)
            {
                _localSettings.Values["ProductsPerPage"] = item.Tag?.ToString() ?? "20";
            }
        }

        private void OrdersPerPageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersPerPageCombo?.SelectedItem is ComboBoxItem item)
            {
                _localSettings.Values["OrdersPerPage"] = item.Tag?.ToString() ?? "20";
            }
        }

        private void RememberLastPageToggle_Toggled(object sender, RoutedEventArgs e)
        {
            _localSettings.Values["RememberLastPage"] = RememberLastPageToggle.IsOn;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _localSettings.Values["ServerAddress"] = ServerAddressBox.Text;
            _localSettings.Values["ServerPort"] = ServerPortBox.Text;
            _localSettings.Values["DatabaseName"] = DatabaseNameBox.Text;
            _localSettings.Values["DbUsername"] = UsernameBox.Text;
            _localSettings.Values["DbPassword"] = PasswordBox.Password;

            StatusTextBlock.Foreground = new SolidColorBrush(GetColorFromHex("#22C55E"));
            StatusTextBlock.Text = "✓ Database settings saved successfully!";
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerAddressBox.Text))
            {
                StatusTextBlock.Foreground = new SolidColorBrush(GetColorFromHex("#EF4444"));
                StatusTextBlock.Text = "✗ Server address is required.";
            }
            else
            {
                StatusTextBlock.Foreground = new SolidColorBrush(GetColorFromHex("#22C55E"));
                StatusTextBlock.Text = "✓ Connection parameters are valid (Mock test).";
            }
        }

        private void SaveApiKey_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = GeminiApiKeyBox.Password?.Trim() ?? "";
            _localSettings.Values["GeminiApiKey"] = apiKey;

            if (!string.IsNullOrEmpty(apiKey))
            {
                ApiKeyStatusText.Foreground = new SolidColorBrush(GetColorFromHex("#22C55E"));
                ApiKeyStatusText.Text = "✓ API key saved successfully!";
            }
            else
            {
                ApiKeyStatusText.Foreground = new SolidColorBrush(GetColorFromHex("#F59E0B"));
                ApiKeyStatusText.Text = "⚠ API key cleared.";
            }
        }

        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService == null) return;

            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("SQLite Database", new System.Collections.Generic.List<string>() { ".db" });
            picker.SuggestedFileName = $"MyShop_Backup_{DateTime.Now:yyyyMMdd_HHmm}";

            // Init picker with window handle
            var window = (Application.Current as App)?.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    await _databaseService.BackupAsync(file.Path);
                    BackupStatusText.Foreground = new SolidColorBrush(GetColorFromHex("#22C55E"));
                    BackupStatusText.Text = $"✓ Backup saved to: {file.Name}";
                }
                catch (Exception ex)
                {
                    BackupStatusText.Foreground = new SolidColorBrush(GetColorFromHex("#EF4444"));
                    BackupStatusText.Text = $"✗ Backup failed: {ex.Message}";
                }
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService == null) return;

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".db");

            // Init picker with window handle
            var window = (Application.Current as App)?.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirm Restore",
                    Content = $"Are you sure you want to restore from '{file.Name}'?\nCurrent data will be replaced. The application may need manual restart.",
                    PrimaryButtonText = "Restore",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        await _databaseService.RestoreAsync(file.Path);
                        BackupStatusText.Foreground = new SolidColorBrush(GetColorFromHex("#22C55E"));
                        BackupStatusText.Text = $"✓ Restore successful. Please restart the app.";
                        
                        // Show restart prompt
                        var restartDialog = new ContentDialog
                        {
                            Title = "Restore Successful",
                            Content = "Data restored successfully. You should restart the application now to reload data.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await restartDialog.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        BackupStatusText.Foreground = new SolidColorBrush(GetColorFromHex("#EF4444"));
                        BackupStatusText.Text = $"✗ Restore failed: {ex.Message}";
                    }
                }
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
    }
}
