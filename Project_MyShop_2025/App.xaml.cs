using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project_MyShop_2025
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public Window? Window { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Set database path trong LocalAppData của app
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var dbPath = System.IO.Path.Combine(localFolder.Path, "myshop.db");
                Project_MyShop_2025.Core.Data.DatabasePathHelper.SetDatabasePath(dbPath);
            }
            catch
            {
                // Fallback: sử dụng default path
            }

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<ViewModels.MainViewModel>();

            services.AddDbContext<Project_MyShop_2025.Core.Data.ShopDbContext>(options =>
            {
                var connectionString = Project_MyShop_2025.Core.Data.DatabasePathHelper.GetConnectionString();
                options.UseSqlite(connectionString);
                // Suppress pending model changes warning - we'll use EnsureCreated instead
                options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = Services.GetService<MainWindow>();
            if (Window != null)
            {
                Window.Activate();
            }

            // Initialize database asynchronously after window is shown
            _ = InitializeDatabaseAsync();
        }

        private async System.Threading.Tasks.Task InitializeDatabaseAsync()
        {
            try
            {
                // Log database path để debug
                var dbPath = Project_MyShop_2025.Core.Data.DatabasePathHelper.GetDatabasePath();
                System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
                System.Diagnostics.Debug.WriteLine($"Database exists before: {System.IO.File.Exists(dbPath)}");

                using (var scope = Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<Project_MyShop_2025.Core.Data.ShopDbContext>();
                    
                    // Ensure database is created
                    System.Diagnostics.Debug.WriteLine("Creating database...");
                    await System.Threading.Tasks.Task.Run(() => context.Database.EnsureCreated());
                    System.Diagnostics.Debug.WriteLine($"Database exists after EnsureCreated: {System.IO.File.Exists(dbPath)}");
                    
                    // Seed data
                    System.Diagnostics.Debug.WriteLine("Seeding data...");
                    await System.Threading.Tasks.Task.Run(() => Project_MyShop_2025.Core.Data.DbSeeder.Seed(context));
                    
                    // Verify seed
                    var productCount = context.Products.Count();
                    var orderCount = context.Orders.Count();
                    var categoryCount = context.Categories.Count();
                    System.Diagnostics.Debug.WriteLine($"Seeded: {categoryCount} categories, {productCount} products, {orderCount} orders");
                    System.Diagnostics.Debug.WriteLine($"Database exists final: {System.IO.File.Exists(dbPath)}");
                    System.Diagnostics.Debug.WriteLine($"Database file size: {(System.IO.File.Exists(dbPath) ? new System.IO.FileInfo(dbPath).Length : 0)} bytes");
                }
            }
            catch (Exception ex)
            {
                // Log error - trong production nên dùng proper logging
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Show error dialog
                if (Window != null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Database Error",
                        Content = $"Error initializing database:\n{ex.Message}\n\nCheck Debug Output for details.",
                        CloseButtonText = "OK",
                        XamlRoot = Window.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
        }
    }
}
