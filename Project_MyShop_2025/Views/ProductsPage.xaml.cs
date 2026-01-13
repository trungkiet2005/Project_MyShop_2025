using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ProductsPage : Page
    {
        private readonly ShopDbContext _context;
        private List<Product> _allProducts = new();
        private List<Product> _filteredProducts = new();
        private List<CategoryFilterItem> _categories = new();
        
        private int _currentPage = 1;
        private int _pageSize = 40;
        private int _totalPages = 1;
        private int? _selectedCategoryId = null;

        public ProductsPage()
        {
            this.InitializeComponent();
            
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            var connectionString = Project_MyShop_2025.Core.Data.DatabasePathHelper.GetConnectionString();
            optionsBuilder.UseSqlite(connectionString);
            _context = new ShopDbContext(optionsBuilder.Options);

            this.Loaded += ProductsPage_Loaded;
        }

        private async void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategories();
            await LoadProducts();
        }

        private async Task LoadCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            
            _categories = new List<CategoryFilterItem>
            {
                new CategoryFilterItem { Id = null, Name = "All Categories", IsSelected = true }
            };

            foreach (var category in categories)
            {
                _categories.Add(new CategoryFilterItem 
                { 
                    Id = category.Id, 
                    Name = category.Name,
                    IsSelected = false 
                });
            }

            // Update ComboBox
            CategoryComboBox.ItemsSource = _categories;
            CategoryComboBox.SelectedIndex = 0;

            // Update Category Pills
            UpdateCategoryPills();
        }

        private void UpdateCategoryPills()
        {
            CategoryPillsPanel.Children.Clear();
            
            foreach (var category in _categories)
            {
                var isSelected = category.Id == _selectedCategoryId || 
                                (category.Id == null && _selectedCategoryId == null);
                
                var pill = new Button
                {
                    Content = category.Name,
                    Background = isSelected 
                        ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246))
                        : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 243, 244, 246)),
                    Foreground = isSelected
                        ? new SolidColorBrush(Microsoft.UI.Colors.White)
                        : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 55, 65, 81)),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(14, 6, 14, 6),
                    BorderThickness = new Thickness(0),
                    Tag = category.Id,
                    FontSize = 12,
                    FontWeight = isSelected ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
                };
                
                pill.Click += CategoryPill_Click;
                CategoryPillsPanel.Children.Add(pill);
            }
        }

        private void CategoryPill_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _selectedCategoryId = button.Tag as int?;
                UpdateCategoryPills();
                ApplyFilters();
            }
        }

        private async Task LoadProducts()
        {
            _allProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredProducts = _allProducts.ToList();

            // Category filter
            if (_selectedCategoryId.HasValue)
            {
                _filteredProducts = _filteredProducts
                    .Where(p => p.CategoryId == _selectedCategoryId.Value)
                    .ToList();
            }

            // Search filter
            var searchText = SearchBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                _filteredProducts = _filteredProducts
                    .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Price range filter
            if (int.TryParse(MinPriceBox.Text, out int minPrice))
            {
                _filteredProducts = _filteredProducts.Where(p => p.Price >= minPrice).ToList();
            }

            if (int.TryParse(MaxPriceBox.Text, out int maxPrice))
            {
                _filteredProducts = _filteredProducts.Where(p => p.Price <= maxPrice).ToList();
            }

            // Apply sorting
            var selectedSort = (SortComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
            _filteredProducts = selectedSort switch
            {
                "NameDesc" => _filteredProducts.OrderByDescending(p => p.Name).ToList(),
                "PriceAsc" => _filteredProducts.OrderBy(p => p.Price).ToList(),
                "PriceDesc" => _filteredProducts.OrderByDescending(p => p.Price).ToList(),
                "StockAsc" => _filteredProducts.OrderBy(p => p.Quantity).ToList(),
                "StockDesc" => _filteredProducts.OrderByDescending(p => p.Quantity).ToList(),
                _ => _filteredProducts.OrderBy(p => p.Name).ToList()
            };

            // Update pagination
            _currentPage = 1;
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            var totalItems = _filteredProducts.Count;
            
            if (_pageSize == -1)
            {
                _totalPages = 1;
                DisplayProducts(_filteredProducts);
            }
            else
            {
                _totalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
                if (_totalPages == 0) _totalPages = 1;
                
                if (_currentPage > _totalPages)
                    _currentPage = _totalPages;

                var pagedProducts = _filteredProducts
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToList();

                DisplayProducts(pagedProducts);
            }

            // Update UI
            if (PageInfoText != null)
                PageInfoText.Text = $"Page {_currentPage} of {_totalPages}";
            
            if (PrevPageButton != null)
                PrevPageButton.IsEnabled = _currentPage > 1;
            
            if (NextPageButton != null)
                NextPageButton.IsEnabled = _currentPage < _totalPages;
            
            if (ResultsCountText != null)
                ResultsCountText.Text = $"{_filteredProducts.Count} item{(_filteredProducts.Count != 1 ? "s" : "")}";
        }

        private void DisplayProducts(List<Product> products)
        {
            var displayProducts = products.Select(p => 
            {
                var imagePath = p.ProductImages.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImagePath ?? p.Image ?? "/Assets/placeholder.png";
                
                // Determine stock status and colors
                string stockBadgeText;
                string stockBadgeBg;
                string stockBadgeFg;
                
                if (p.Quantity == 0)
                {
                    // Out of Stock - Red/Gray
                    stockBadgeText = "Out of stock";
                    stockBadgeBg = "#FEE2E2";
                    stockBadgeFg = "#DC2626";
                }
                else if (p.Quantity < 10)
                {
                    // Low Stock - Yellow/Orange
                    stockBadgeText = $"Low: {p.Quantity}";
                    stockBadgeBg = "#FEF3C7";
                    stockBadgeFg = "#D97706";
                }
                else
                {
                    // In Stock - Green
                    stockBadgeText = $"In stock: {p.Quantity}";
                    stockBadgeBg = "#DCFCE7";
                    stockBadgeFg = "#16A34A";
                }

                return new ProductDisplayModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU ?? "N/A",
                    Description = p.Description,
                    Price = p.Price,
                    PriceFormatted = $"₫{p.Price:N0}",
                    Quantity = p.Quantity,
                    Image = imagePath,
                    StockBadgeText = stockBadgeText,
                    StockBadgeBackground = new SolidColorBrush(GetColorFromHex(stockBadgeBg)),
                    StockBadgeForeground = new SolidColorBrush(GetColorFromHex(stockBadgeFg))
                };
            }).ToList();

            ProductsGridView.ItemsSource = displayProducts;
            
            // Load images async
            _ = LoadImagesForProductsAsync(displayProducts);
        }

        private static Windows.UI.Color GetColorFromHex(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        private async Task LoadImagesForProductsAsync(List<ProductDisplayModel> products)
        {
            foreach (var product in products)
            {
                try
                {
                    product.ImageSource = await GetImageSourceAsync(product.Image);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image for product {product.Name}: {ex.Message}");
                }
            }
        }

        private async Task<Microsoft.UI.Xaml.Media.ImageSource?> GetImageSourceAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return null;

                // URL (http/https)
                if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(imagePath));
                }

                // file:// URI
                if (imagePath.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = imagePath.Replace("file:///", "").Replace("file://", "");
                    filePath = filePath.Replace('/', '\\');
                    
                    if (System.IO.File.Exists(filePath))
                    {
                        var file = await StorageFile.GetFileFromPathAsync(filePath);
                        using (var stream = await file.OpenReadAsync())
                        {
                            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                            await bitmap.SetSourceAsync(stream);
                            return bitmap;
                        }
                    }
                }

                // Relative path starting with /
                if (imagePath.StartsWith("/", StringComparison.Ordinal))
                {
                    var msAppxPath = "ms-appx://" + imagePath;
                    return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(msAppxPath));
                }

                // Relative path without /
                if (!imagePath.Contains("://") && !System.IO.Path.IsPathRooted(imagePath))
                {
                    var msAppxPath = "ms-appx:///" + imagePath;
                    return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(msAppxPath));
                }

                // ms-appx:// path
                if (imagePath.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                {
                    return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(imagePath));
                }

                // Fallback
                return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(imagePath));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image from '{imagePath}': {ex.Message}");
                return null;
            }
        }

        // Event Handlers
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is CategoryFilterItem selected)
            {
                _selectedCategoryId = selected.Id;
                UpdateCategoryPills();
                ApplyFilters();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void PriceFilter_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allProducts.Any())
            {
                ApplyFilters();
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
            }
        }

        private void PageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeComboBox != null && PageSizeComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                _pageSize = int.Parse(item.Tag.ToString() ?? "40");
                UpdatePagination();
            }
        }

        private async void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                await ShowEditProductDialog(productId);
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                await DeleteProduct(productId);
            }
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowAddProductDialog();
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowAddCategoryDialog();
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowImportDialog();
        }

        private void ProductCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246));
                border.BorderThickness = new Thickness(2);
                border.Scale = new System.Numerics.Vector3(1.02f, 1.02f, 1f);
            }
        }

        private void ProductCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = new SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 229, 231, 235));
                border.BorderThickness = new Thickness(1);
                border.Scale = new System.Numerics.Vector3(1f, 1f, 1f);
            }
        }

        // Dialog Methods
        private async Task ShowEditProductDialog(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return;

            var dialog = new ContentDialog
            {
                Title = "Edit Product",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var nameBox = new TextBox { Header = "Product Name", Text = product.Name, Margin = new Thickness(0, 8, 0, 0) };
            var skuBox = new TextBox { Header = "SKU", Text = product.SKU, Margin = new Thickness(0, 8, 0, 0) };
            var descBox = new TextBox { Header = "Description", Text = product.Description, Margin = new Thickness(0, 8, 0, 0), AcceptsReturn = true };
            var priceBox = new TextBox { Header = "Price", Text = product.Price.ToString(), Margin = new Thickness(0, 8, 0, 0) };
            var importPriceBox = new TextBox { Header = "Import Price", Text = product.ImportPrice.ToString(), Margin = new Thickness(0, 8, 0, 0) };
            var qtyBox = new TextBox { Header = "Quantity", Text = product.Quantity.ToString(), Margin = new Thickness(0, 8, 0, 0) };

            var categoryCombo = new ComboBox { Header = "Category", Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            var categories = await _context.Categories.ToListAsync();
            categoryCombo.ItemsSource = categories;
            categoryCombo.DisplayMemberPath = "Name";
            categoryCombo.SelectedItem = categories.FirstOrDefault(c => c.Id == product.CategoryId);

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(nameBox);
            content.Children.Add(skuBox);
            content.Children.Add(categoryCombo);
            content.Children.Add(priceBox);
            content.Children.Add(importPriceBox);
            content.Children.Add(qtyBox);
            content.Children.Add(descBox);

            dialog.Content = new ScrollViewer { Content = content, MaxHeight = 500 };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                product.Name = nameBox.Text;
                product.SKU = skuBox.Text;
                product.Description = descBox.Text;
                product.Price = int.TryParse(priceBox.Text, out int price) ? price : 0;
                product.ImportPrice = int.TryParse(importPriceBox.Text, out int importPrice) ? importPrice : 0;
                product.Quantity = int.TryParse(qtyBox.Text, out int qty) ? qty : 0;
                product.CategoryId = (categoryCombo.SelectedItem as Category)?.Id ?? product.CategoryId;

                await _context.SaveChangesAsync();
                await LoadProducts();
            }
        }

        private async Task ShowAddProductDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Add New Product",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var nameBox = new TextBox { Header = "Product Name*", Margin = new Thickness(0, 8, 0, 0) };
            var skuBox = new TextBox { Header = "SKU", Margin = new Thickness(0, 8, 0, 0) };
            var descBox = new TextBox { Header = "Description", Margin = new Thickness(0, 8, 0, 0), AcceptsReturn = true };
            var priceBox = new TextBox { Header = "Price*", Text = "0", Margin = new Thickness(0, 8, 0, 0) };
            var importPriceBox = new TextBox { Header = "Import Price", Text = "0", Margin = new Thickness(0, 8, 0, 0) };
            var qtyBox = new TextBox { Header = "Quantity*", Text = "0", Margin = new Thickness(0, 8, 0, 0) };

            var categoryCombo = new ComboBox { Header = "Category*", Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
            var categories = await _context.Categories.ToListAsync();
            categoryCombo.ItemsSource = categories;
            categoryCombo.DisplayMemberPath = "Name";
            if (categories.Any()) categoryCombo.SelectedIndex = 0;

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(nameBox);
            content.Children.Add(skuBox);
            content.Children.Add(categoryCombo);
            content.Children.Add(priceBox);
            content.Children.Add(importPriceBox);
            content.Children.Add(qtyBox);
            content.Children.Add(descBox);

            dialog.Content = new ScrollViewer { Content = content, MaxHeight = 500 };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
            {
                var product = new Product
                {
                    Name = nameBox.Text,
                    SKU = skuBox.Text,
                    Description = descBox.Text,
                    Price = int.TryParse(priceBox.Text, out int price) ? price : 0,
                    ImportPrice = int.TryParse(importPriceBox.Text, out int importPrice) ? importPrice : 0,
                    Quantity = int.TryParse(qtyBox.Text, out int qty) ? qty : 0,
                    CategoryId = (categoryCombo.SelectedItem as Category)?.Id ?? 1
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                await LoadProducts();
            }
        }

        private async Task ShowAddCategoryDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Add New Category",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var nameBox = new TextBox { Header = "Category Name*", Margin = new Thickness(0, 8, 0, 0) };
            var descBox = new TextBox { Header = "Description", Margin = new Thickness(0, 8, 0, 0), AcceptsReturn = true };

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(nameBox);
            content.Children.Add(descBox);

            dialog.Content = content;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
            {
                var category = new Category
                {
                    Name = nameBox.Text,
                    Description = descBox.Text
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                await LoadCategories();
            }
        }

        private async Task DeleteProduct(int productId)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = "Are you sure you want to delete this product? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product != null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    await LoadProducts();
                }
            }
        }

        private async Task ShowImportDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Import Products",
                Content = "Choose file format to import:",
                PrimaryButtonText = "Excel",
                SecondaryButtonText = "Access",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ImportFromExcel();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                await ImportFromAccess();
            }
        }

        private async Task ImportFromExcel()
        {
            var picker = new FileOpenPicker();
            var window = (Application.Current as App)?.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var notImplementedDialog = new ContentDialog
                {
                    Title = "Import from Excel",
                    Content = $"Excel import functionality will process: {file.Name}\n\nThis feature requires implementation of Excel parsing logic.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await notImplementedDialog.ShowAsync();
            }
        }

        private async Task ImportFromAccess()
        {
            var picker = new FileOpenPicker();
            var window = (Application.Current as App)?.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".mdb");
            picker.FileTypeFilter.Add(".accdb");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var notImplementedDialog = new ContentDialog
                {
                    Title = "Import from Access",
                    Content = $"Access import functionality will process: {file.Name}\n\nThis feature requires implementation of Access database connection logic.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await notImplementedDialog.ShowAsync();
            }
        }
    }

    // Helper classes
    public class CategoryFilterItem
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class ProductDisplayModel : INotifyPropertyChanged
    {
        private Microsoft.UI.Xaml.Media.ImageSource? _imageSource;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Price { get; set; }
        public string PriceFormatted { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Image { get; set; } = string.Empty;
        
        public Microsoft.UI.Xaml.Media.ImageSource? ImageSource 
        { 
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }
        
        // Stock Badge Properties
        public string StockBadgeText { get; set; } = string.Empty;
        public SolidColorBrush StockBadgeBackground { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.Gray);
        public SolidColorBrush StockBadgeForeground { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.White);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
