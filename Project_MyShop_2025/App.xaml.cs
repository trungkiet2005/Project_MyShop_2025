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
using Project_MyShop_2025.Core.Services.Interfaces;
using Project_MyShop_2025.Core.Services.Implementations;

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
        /// Gets the service provider for dependency injection.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Set database path trong thư mục project (cùng thư mục với .exe)
            try
            {
                // Lấy đường dẫn thư mục chứa .exe (output directory)
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exeDirectory = System.IO.Path.GetDirectoryName(exePath);
                
                // Database sẽ được lưu trong cùng thư mục với .exe
                if (!string.IsNullOrEmpty(exeDirectory))
                {
                    var dbPath = System.IO.Path.Combine(exeDirectory, "myshop.db");
                    Project_MyShop_2025.Core.Data.DatabasePathHelper.SetDatabasePath(dbPath);
                    System.Diagnostics.Debug.WriteLine($"Database path set to: {dbPath}");
                }
                else
                {
                    // Fallback to BaseDirectory if exeDirectory is null
                    var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myshop.db");
                    Project_MyShop_2025.Core.Data.DatabasePathHelper.SetDatabasePath(dbPath);
                    System.Diagnostics.Debug.WriteLine($"Database path set to (fallback): {dbPath}");
                }
            }
            catch (Exception ex)
            {
                // Fallback: sử dụng default path (BaseDirectory)
                System.Diagnostics.Debug.WriteLine($"Error setting database path: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Using default path: {AppDomain.CurrentDomain.BaseDirectory}");
            }

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Window & ViewModels
            services.AddTransient<MainWindow>();
            services.AddTransient<ViewModels.MainViewModel>();
            
            // MVVM ViewModels - Transient để tạo mới mỗi lần navigate
            services.AddTransient<ViewModels.DashboardViewModel>();
            services.AddTransient<ViewModels.ProductsViewModel>();
            services.AddTransient<ViewModels.OrdersViewModel>();

            // Database Context
            services.AddDbContext<Project_MyShop_2025.Core.Data.ShopDbContext>(options =>
            {
                var connectionString = Project_MyShop_2025.Core.Data.DatabasePathHelper.GetConnectionString();
                options.UseSqlite(connectionString);
                // Suppress pending model changes warning - we'll use EnsureCreated instead
                options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            // Business Services - Scoped để share context trong một request
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<Services.PrintService>();
            services.AddScoped<Core.Services.Interfaces.IPrintService>(provider => provider.GetRequiredService<Services.PrintService>());
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
                    var ordersWithItems = context.Orders
                        .Include(o => o.Items)
                        .Where(o => o.Items.Any())
                        .Count();
                    var categoryCount = context.Categories.Count();
                    System.Diagnostics.Debug.WriteLine($"Seeded: {categoryCount} categories, {productCount} products, {orderCount} total orders, {ordersWithItems} orders with items");
                    System.Diagnostics.Debug.WriteLine($"Database exists final: {System.IO.File.Exists(dbPath)}");
                    System.Diagnostics.Debug.WriteLine($"Database file size: {(System.IO.File.Exists(dbPath) ? new System.IO.FileInfo(dbPath).Length : 0)} bytes");
                    
                    // Nếu không có orders với items, thử force seed lại
                    if (ordersWithItems == 0 && productCount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: No orders with items found after seed. Attempting to force seed orders...");
                        await System.Threading.Tasks.Task.Run(() => Project_MyShop_2025.Core.Data.DbSeeder.SeedOrders(context, force: true));
                        var newOrderCount = context.Orders
                            .Include(o => o.Items)
                            .Where(o => o.Items.Any())
                            .Count();
                        System.Diagnostics.Debug.WriteLine($"After force seed: {newOrderCount} orders with items");
                    }
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
