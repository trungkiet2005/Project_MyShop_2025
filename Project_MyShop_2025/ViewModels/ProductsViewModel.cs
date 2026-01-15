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
    /// ViewModel for the Products management page.
    /// Handles product listing, filtering, searching, and CRUD operations.
    /// </summary>
    public partial class ProductsViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        #region Observable Properties

        /// <summary>
        /// Collection of products to display.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        /// <summary>
        /// Collection of categories for filtering.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        /// <summary>
        /// Currently selected category for filtering.
        /// </summary>
        [ObservableProperty]
        private Category? _selectedCategory;

        /// <summary>
        /// Currently selected product for details/editing.
        /// </summary>
        [ObservableProperty]
        private Product? _selectedProduct;

        /// <summary>
        /// Search keyword for product name/SKU.
        /// </summary>
        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        /// <summary>
        /// Minimum price filter.
        /// </summary>
        [ObservableProperty]
        private int? _minPrice;

        /// <summary>
        /// Maximum price filter.
        /// </summary>
        [ObservableProperty]
        private int? _maxPrice;

        /// <summary>
        /// Minimum stock filter.
        /// </summary>
        [ObservableProperty]
        private int? _minStock;

        /// <summary>
        /// Maximum stock filter.
        /// </summary>
        [ObservableProperty]
        private int? _maxStock;

        /// <summary>
        /// Sort by field.
        /// </summary>
        [ObservableProperty]
        private string _sortBy = "NameAsc";

        /// <summary>
        /// Current page number (1-indexed).
        /// </summary>
        [ObservableProperty]
        private int _currentPage = 1;

        /// <summary>
        /// Items per page.
        /// </summary>
        [ObservableProperty]
        private int _pageSize = 20;

        /// <summary>
        /// Total number of pages.
        /// </summary>
        [ObservableProperty]
        private int _totalPages = 1;

        /// <summary>
        /// Total number of products matching current filter.
        /// </summary>
        [ObservableProperty]
        private int _totalCount;

        /// <summary>
        /// Whether previous page button should be enabled.
        /// </summary>
        public bool CanGoPrevious => CurrentPage > 1;

        /// <summary>
        /// Whether next page button should be enabled.
        /// </summary>
        public bool CanGoNext => CurrentPage < TotalPages;

        #endregion

        #region Constructor

        public ProductsViewModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
            Title = "Products";
        }

        #endregion

        #region Commands

        /// <summary>
        /// Load/refresh product list with current filters.
        /// </summary>
        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var criteria = new ProductSearchCriteria
                {
                    Keyword = SearchKeyword,
                    CategoryId = SelectedCategory?.Id,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    MinStock = MinStock,
                    MaxStock = MaxStock,
                    SortBy = SortBy,
                    Page = CurrentPage,
                    PageSize = PageSize
                };

                var result = await _productService.GetProductsAsync(criteria);

                Products.Clear();
                foreach (var product in result.Items)
                {
                    Products.Add(product);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages > 0 ? result.TotalPages : 1;

                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
            });
        }

        /// <summary>
        /// Load categories for the filter dropdown.
        /// </summary>
        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            });
        }

        /// <summary>
        /// Search with current keyword.
        /// </summary>
        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadProductsAsync();
        }

        /// <summary>
        /// Clear all filters and reload.
        /// </summary>
        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SearchKeyword = string.Empty;
            SelectedCategory = null;
            MinPrice = null;
            MaxPrice = null;
            MinStock = null;
            MaxStock = null;
            SortBy = "NameAsc";
            CurrentPage = 1;
            await LoadProductsAsync();
        }

        /// <summary>
        /// Navigate to previous page.
        /// </summary>
        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                await LoadProductsAsync();
            }
        }

        /// <summary>
        /// Navigate to next page.
        /// </summary>
        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                await LoadProductsAsync();
            }
        }

        /// <summary>
        /// Delete the selected product.
        /// </summary>
        [RelayCommand]
        private async Task DeleteProductAsync(Product? product)
        {
            if (product == null)
                return;

            await ExecuteAsync(async () =>
            {
                var success = await _productService.DeleteProductAsync(product.Id);
                if (success)
                {
                    Products.Remove(product);
                    TotalCount--;
                }
            });
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedToAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }

        partial void OnSelectedCategoryChanged(Category? value)
        {
            CurrentPage = 1;
            _ = LoadProductsAsync();
        }

        partial void OnSortByChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadProductsAsync();
        }

        #endregion
    }
}
