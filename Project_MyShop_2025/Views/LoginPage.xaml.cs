using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Helpers;
using System.Linq;
using System;
using Project_MyShop_2025; // Critical for casting (App)Application.Current

namespace Project_MyShop_2025.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            LoadVersionInfo();
            CheckRememberedUser();
        }

        private void LoadVersionInfo()
        {
            var version = Package.Current.Id.Version;
            VersionTextBlock.Text = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private void CheckRememberedUser()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("RememberMe_Username"))
            {
                var username = localSettings.Values["RememberMe_Username"] as string;
                
                var app = (App)Application.Current;
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        NavigateToMain();
                    }
                }
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter username and password");
                return;
            }

            var app = (App)Application.Current;
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                
                // Retrieve user
                var user = context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && PasswordHelper.VerifyPassword(password, user.Password))
                {
                    // Login Success
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        var localSettings = ApplicationData.Current.LocalSettings;
                        localSettings.Values["RememberMe_Username"] = username;
                    }

                    NavigateToMain();
                }
                else
                {
                    ShowError("Invalid username or password");
                }
            }
        }

        private async void Signup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Sign Up",
                PrimaryButtonText = "Create Account",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var stackPanel = new StackPanel { Spacing = 10 };
            var userBox = new TextBox { Header = "Username", PlaceholderText = "Enter username" };
            var passBox = new PasswordBox { Header = "Password", PlaceholderText = "Enter password" };
            var confirmPassBox = new PasswordBox { Header = "Confirm Password", PlaceholderText = "Re-enter password" };

            stackPanel.Children.Add(userBox);
            stackPanel.Children.Add(passBox);
            stackPanel.Children.Add(confirmPassBox);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var username = userBox.Text;
                var password = passBox.Password;
                var confirm = confirmPassBox.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Username and password are required");
                    return;
                }

                if (password != confirm)
                {
                    ShowError("Passwords do not match");
                    return;
                }

                try
                {
                    var app = (App)Application.Current;
                    using (var scope = app.Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                        if (context.Users.Any(u => u.Username == username))
                        {
                            ShowError("Username already exists");
                            return;
                        }

                        var hashedPassword = PasswordHelper.HashPassword(password);
                        var user = new User { Username = username, Password = hashedPassword, Role = "User" };
                        context.Users.Add(user);
                        context.SaveChanges();
                    }
                    ShowError("Account created! Please login.", false);
                }
                catch (Exception ex)
                {
                    ShowError($"Error creating account: {ex.Message}");
                }
            }
        }

        private void NavigateToConfig_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConfigPage));
        }

        private void NavigateToMain()
        {
            // Navigate to Dashboard.
            // CAUTION: If using shell, we might just want to change content.
            // But we have RootFrame in MainWindow, so straightforward navigation works.
            Frame.Navigate(typeof(DashboardPage));
        }

        private async void ShowError(string message, bool isError = true)
        {
            var dialog = new ContentDialog
            {
                Title = isError ? "Error" : "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
