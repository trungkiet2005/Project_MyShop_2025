using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Project_MyShop_2025.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            var version = Package.Current.Id.Version;
            VersionTextBlock.Text = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Login Logic
            // For now, simple validation or just proceed
        }

        private void NavigateToConfig_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConfigPage));
        }
    }
}
