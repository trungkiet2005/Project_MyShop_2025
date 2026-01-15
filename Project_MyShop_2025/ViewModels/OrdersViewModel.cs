using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Project_MyShop_2025.ViewModels
{
    /// <summary>
    /// ViewModel for the Orders management page.
    /// Handles order listing, filtering, searching, and CRUD operations.
    /// Integrates with PromotionService for discount application.
    /// </summary>
    public partial class OrdersViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IPromotionService _promotionService;

        #region Observable Properties

        /// <summary>
        /// Collection of orders to display.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Order> _orders = new();

        /// <summary>
        /// Currently selected order for details/editing.
        /// </summary>
        [ObservableProperty]
        private Order? _selectedOrder;

        /// <summary>
        /// Search keyword for order ID, customer name/phone.
        /// </summary>
        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        /// <summary>
        /// Filter by start date.
        /// </summary>
        [ObservableProperty]
        private DateTime? _fromDate;

        /// <summary>
        /// Filter by end date.
        /// </summary>
        [ObservableProperty]
        private DateTime? _toDate;

        /// <summary>
        /// Filter by order status.
        /// </summary>
        [ObservableProperty]
        private OrderStatus? _statusFilter;

        /// <summary>
        /// Sort by field.
        /// </summary>
        [ObservableProperty]
        private string _sortBy = "DateDesc";

        /// <summary>
        /// Current page number.
        /// </summary>
        [ObservableProperty]
        private int _currentPage = 1;

        /// <summary>
        /// Items per page.
        /// </summary>
        [ObservableProperty]
        private int _pageSize = 20;

        /// <summary>
        /// Total pages.
        /// </summary>
        [ObservableProperty]
        private int _totalPages = 1;

        /// <summary>
        /// Total orders matching filter.
        /// </summary>
        [ObservableProperty]
        private int _totalCount;

        /// <summary>
        /// Available customers for order creation.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();

        /// <summary>
        /// Available products for order creation.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Product> _availableProducts = new();

        /// <summary>
        /// Promotion code entered by user.
        /// </summary>
        [ObservableProperty]
        private string _promotionCode = string.Empty;

        /// <summary>
        /// Currently validated promotion.
        /// </summary>
        [ObservableProperty]
        private Promotion? _appliedPromotion;

        /// <summary>
        /// Calculated discount amount.
        /// </summary>
        [ObservableProperty]
        private int _discountAmount;

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        #endregion

        #region Constructor

        public OrdersViewModel(
            IOrderService orderService,
            IProductService productService,
            ICustomerService customerService,
            IPromotionService promotionService)
        {
            _orderService = orderService;
            _productService = productService;
            _customerService = customerService;
            _promotionService = promotionService;
            Title = "Orders";
        }

        #endregion

        #region Commands

        /// <summary>
        /// Load orders with current filters.
        /// </summary>
        [RelayCommand]
        private async Task LoadOrdersAsync()
        {
            await ExecuteAsync(async () =>
            {
                var criteria = new OrderSearchCriteria
                {
                    Keyword = SearchKeyword,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    Status = StatusFilter,
                    SortBy = SortBy,
                    Page = CurrentPage,
                    PageSize = PageSize
                };

                var result = await _orderService.GetOrdersAsync(criteria);

                Orders.Clear();
                foreach (var order in result.Items)
                {
                    Orders.Add(order);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;

                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
            });
        }

        /// <summary>
        /// Search with current criteria.
        /// </summary>
        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadOrdersAsync();
        }

        /// <summary>
        /// Clear all filters.
        /// </summary>
        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SearchKeyword = string.Empty;
            FromDate = null;
            ToDate = null;
            StatusFilter = null;
            SortBy = "DateDesc";
            CurrentPage = 1;
            await LoadOrdersAsync();
        }

        /// <summary>
        /// Apply promotion code to calculate discount.
        /// </summary>
        [RelayCommand]
        private async Task ApplyPromotionAsync(int orderSubtotal)
        {
            if (string.IsNullOrWhiteSpace(PromotionCode))
            {
                AppliedPromotion = null;
                DiscountAmount = 0;
                return;
            }

            await ExecuteAsync(async () =>
            {
                var promotion = await _promotionService.ValidateCodeAsync(PromotionCode, orderSubtotal);
                if (promotion != null)
                {
                    AppliedPromotion = promotion;
                    DiscountAmount = promotion.CalculateDiscount(orderSubtotal);
                }
                else
                {
                    AppliedPromotion = null;
                    DiscountAmount = 0;
                    ErrorMessage = "Invalid or expired promotion code";
                }
            });
        }

        /// <summary>
        /// Update order status.
        /// </summary>
        [RelayCommand]
        private async Task UpdateStatusAsync((int orderId, OrderStatus newStatus) args)
        {
            await ExecuteAsync(async () =>
            {
                var success = await _orderService.UpdateOrderStatusAsync(args.orderId, args.newStatus);
                if (success)
                {
                    await LoadOrdersAsync();
                }
            });
        }

        /// <summary>
        /// Delete an order.
        /// </summary>
        [RelayCommand]
        private async Task DeleteOrderAsync(Order? order)
        {
            if (order == null)
                return;

            await ExecuteAsync(async () =>
            {
                var success = await _orderService.DeleteOrderAsync(order.Id);
                if (success)
                {
                    Orders.Remove(order);
                    TotalCount--;
                }
            });
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                await LoadOrdersAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                await LoadOrdersAsync();
            }
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedToAsync()
        {
            await LoadOrdersAsync();
        }

        partial void OnStatusFilterChanged(OrderStatus? value)
        {
            CurrentPage = 1;
            _ = LoadOrdersAsync();
        }

        partial void OnSortByChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadOrdersAsync();
        }

        #endregion
    }
}
