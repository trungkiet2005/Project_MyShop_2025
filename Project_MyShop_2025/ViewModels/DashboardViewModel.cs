using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Project_MyShop_2025.ViewModels
{
    /// <summary>
    /// ViewModel for the Dashboard page.
    /// Displays summary statistics, charts, and recent activity.
    /// </summary>
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        #region Observable Properties

        /// <summary>
        /// Total number of products in the system.
        /// </summary>
        [ObservableProperty]
        private int _totalProducts;

        /// <summary>
        /// Number of orders placed today.
        /// </summary>
        [ObservableProperty]
        private int _todayOrderCount;

        /// <summary>
        /// Total revenue for today (VND).
        /// </summary>
        [ObservableProperty]
        private int _todayRevenue;

        /// <summary>
        /// Formatted display string for today's revenue.
        /// </summary>
        public string TodayRevenueFormatted => $"{TodayRevenue:N0} ₫";

        /// <summary>
        /// Top 5 products with low stock.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Product> _lowStockProducts = new();

        /// <summary>
        /// Top 5 best-selling products.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<BestSellerItem> _bestSellers = new();

        /// <summary>
        /// 3 most recent orders.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Order> _recentOrders = new();

        #endregion

        #region Constructor

        public DashboardViewModel(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
            Title = "Dashboard";
        }

        #endregion

        #region Commands

        /// <summary>
        /// Refresh all dashboard data.
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDashboardDataAsync();
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedToAsync()
        {
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                // Load total products
                TotalProducts = await _productService.GetTotalProductCountAsync();

                // Load today's stats
                TodayOrderCount = await _orderService.GetTodayOrderCountAsync();
                TodayRevenue = await _orderService.GetTodayRevenueAsync();
                OnPropertyChanged(nameof(TodayRevenueFormatted));

                // Load low stock products
                var lowStock = await _productService.GetLowStockProductsAsync(5, 5);
                LowStockProducts.Clear();
                foreach (var product in lowStock)
                {
                    LowStockProducts.Add(product);
                }

                // Load best sellers
                var bestSellers = await _productService.GetBestSellersAsync(5);
                BestSellers.Clear();
                foreach (var (product, totalSold) in bestSellers)
                {
                    BestSellers.Add(new BestSellerItem
                    {
                        Product = product,
                        TotalSold = totalSold
                    });
                }

                // Load recent orders
                var recentOrders = await _orderService.GetRecentOrdersAsync(3);
                RecentOrders.Clear();
                foreach (var order in recentOrders)
                {
                    RecentOrders.Add(order);
                }
            });
        }

        #endregion
    }

    /// <summary>
    /// Display model for best-selling products.
    /// </summary>
    public class BestSellerItem
    {
        public Product Product { get; set; } = null!;
        public int TotalSold { get; set; }
        public string ProductName => Product?.Name ?? "";
        public string TotalSoldFormatted => $"{TotalSold} sold";
    }
}
