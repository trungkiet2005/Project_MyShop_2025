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

        public ConfigPage()
        {
            this.InitializeComponent();
            _localSettings = ApplicationData.Current.LocalSettings;
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
