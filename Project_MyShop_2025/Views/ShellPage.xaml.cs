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
                UpdateUserProfile(username ?? "User");
            }
            else
            {
                UpdateUserProfile("User");
            }
        }

        private void UpdateUserProfile(string username)
        {
            UserNameText.Text = $"Welcome, {username}";
            
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

        private void UserProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new MenuFlyout();
            
            var localSettings = ApplicationData.Current.LocalSettings;
            var username = localSettings.Values.ContainsKey("RememberMe_Username") 
                ? localSettings.Values["RememberMe_Username"] as string ?? "User"
                : "User";
            
            // Add header text (using MenuFlyoutItem as header)
            var headerItem = new MenuFlyoutItem
            {
                Text = $"Welcome, {username}",
                IsEnabled = false
            };
            flyout.Items.Add(headerItem);
            
            // Add separator
            flyout.Items.Add(new MenuFlyoutSeparator());
            
            // Add Settings item
            var settingsItem = new MenuFlyoutItem
            {
                Text = "Settings",
                Icon = new FontIcon { Glyph = "\uE713" }
            };
            settingsItem.Click += SettingsButton_Click;
            flyout.Items.Add(settingsItem);
            
            // Add Logout item
            var logoutItem = new MenuFlyoutItem
            {
                Text = "Logout",
                Icon = new FontIcon { Glyph = "\uE8F8" }
            };
            logoutItem.Click += Logout_Click;
            flyout.Items.Add(logoutItem);
            
            flyout.ShowAt(UserProfileButton);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ConfigPage));
            NavView.SelectedItem = null;
        }

        private void GlobalSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // Navigate to appropriate page based on search
            // For now, navigate to Products page with search
            if (ContentFrame.Content is ProductsPage productsPage)
            {
                // Trigger search in ProductsPage if it has search functionality
            }
            else
            {
                ContentFrame.Navigate(typeof(ProductsPage));
            }
        }

        private void GlobalSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Could implement autocomplete suggestions here
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
