using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using System;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ShellPage : Page
    {
        private readonly ApplicationDataContainer _localSettings;

        public ShellPage()
        {
            this.InitializeComponent();
            _localSettings = ApplicationData.Current.LocalSettings;

            // Navigate to last page or Dashboard by default
            NavigateToLastPageOrDefault();

            // Load username if available
            if (_localSettings.Values.ContainsKey("RememberMe_Username"))
            {
                var username = _localSettings.Values["RememberMe_Username"] as string;
                UpdateUserProfile(username ?? "Admin");
            }
            else
            {
                UpdateUserProfile("Admin");
            }
        }

        private void NavigateToLastPageOrDefault()
        {
            var rememberLastPage = _localSettings.Values["RememberLastPage"] as bool? ?? true;
            var lastPage = _localSettings.Values["LastPage"] as string ?? "Dashboard";

            if (rememberLastPage && !string.IsNullOrEmpty(lastPage))
            {
                NavigateToPage(lastPage);
                SelectNavigationItem(lastPage);
            }
            else
            {
                ContentFrame.Navigate(typeof(DashboardPage));
                NavView.SelectedItem = NavView.MenuItems[0];
            }
        }

        private void SelectNavigationItem(string tag)
        {
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == tag)
                {
                    NavView.SelectedItem = navItem;
                    return;
                }
            }
            // Default to first item if not found
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavigateToPage(string tag)
        {
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
                case "Chatbot":
                    ContentFrame.Navigate(typeof(ChatbotPage));
                    break;
                default:
                    ContentFrame.Navigate(typeof(DashboardPage));
                    break;
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
                if (!string.IsNullOrEmpty(tag))
                {
                    NavigateToPage(tag);
                    
                    // Save last page
                    _localSettings.Values["LastPage"] = tag;
                }
            }
            else if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(ConfigPage));
                // Don't save Settings as last page
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _localSettings.Values.Remove("RememberMe_Username");
            
            // Navigate back to login using parent frame
            if (Frame != null)
            {
                Frame.Navigate(typeof(LoginPage));
            }
        }
    }
}
