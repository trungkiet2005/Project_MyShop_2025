using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using System;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            // Navigate to Dashboard by default
            ContentFrame.Navigate(typeof(DashboardPage));
            NavView.SelectedItem = NavView.MenuItems[0];

            // Load username if available
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("RememberMe_Username"))
            {
                var username = localSettings.Values["RememberMe_Username"] as string;
                UpdateUserProfile(username ?? "Admin");
            }
            else
            {
                UpdateUserProfile("Admin");
            }
        }

        private void UpdateUserProfile(string username)
        {
            UserNameText.Text = username;
            
            // Set user initial (first letter)
            if (!string.IsNullOrEmpty(username))
            {
                UserInitialText.Text = username.Substring(0, 1).ToUpper();
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var tag = args.SelectedItemContainer.Tag?.ToString();
                switch (tag)
                {
                    case "Dashboard":
                        ContentFrame.Navigate(typeof(DashboardPage));
                        break;
                    case "Products":
                        ContentFrame.Navigate(typeof(ProductsPage));
                        break;
                    case "Orders":
                        ContentFrame.Navigate(typeof(OrdersPage));
                        break;
                    case "Reports":
                        ContentFrame.Navigate(typeof(ReportsPage));
                        break;
                }
            }
            else if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(ConfigPage));
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("RememberMe_Username");
            
            // Navigate back to login using parent frame
            if (Frame != null)
            {
                Frame.Navigate(typeof(LoginPage));
            }
        }
    }
}
