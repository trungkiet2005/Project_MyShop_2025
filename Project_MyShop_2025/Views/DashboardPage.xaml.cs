using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Project_MyShop_2025.Views
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            this.InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Clear Remember Me
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Remove("RememberMe_Username");
            
            // Navigate back to Login
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
