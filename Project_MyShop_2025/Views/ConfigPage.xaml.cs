using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ConfigPage : Page
    {
        public ConfigPage()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            ServerAddressBox.Text = localSettings.Values["ServerAddress"] as string ?? "localhost";
            ServerPortBox.Text = localSettings.Values["ServerPort"] as string ?? "5432"; // Default PostgreSQL port
            DatabaseNameBox.Text = localSettings.Values["DatabaseName"] as string ?? "MyShop";
            UsernameBox.Text = localSettings.Values["DbUsername"] as string ?? "postgres";
            PasswordBox.Password = localSettings.Values["DbPassword"] as string ?? "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ServerAddress"] = ServerAddressBox.Text;
            localSettings.Values["ServerPort"] = ServerPortBox.Text;
            localSettings.Values["DatabaseName"] = DatabaseNameBox.Text;
            localSettings.Values["DbUsername"] = UsernameBox.Text;
            localSettings.Values["DbPassword"] = PasswordBox.Password;

            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 128, 0)); // Green
            StatusTextBlock.Text = "Settings saved successfully!";
            
            // Navigate back or to Login? For now effectively just save.
            // Frame.GoBack(); 
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement actual connection test logic
            // For now just simulate a check
            if (string.IsNullOrWhiteSpace(ServerAddressBox.Text))
            {
                StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                StatusTextBlock.Text = "Server address is required.";
            }
            else
            {
                 StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 128, 0));
                 StatusTextBlock.Text = "Connection parameters format valid (Mock test).";
            }
        }
    }
}
