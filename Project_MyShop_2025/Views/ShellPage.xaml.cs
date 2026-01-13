using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

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
                UserNameText.Text = $"Welcome, {username}";
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
                        // TODO: Navigate to Products page
                        break;
                    case "Orders":
                        // TODO: Navigate to Orders page
                        break;
                    case "Reports":
                        // TODO: Navigate to Reports page
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
