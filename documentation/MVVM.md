# MVVM Architecture

## Overview

The application follows the **Model-View-ViewModel (MVVM)** architectural pattern using **CommunityToolkit.Mvvm** for:
- `ObservableObject` - Property change notifications
- `[ObservableProperty]` - Auto-generated properties with notifications
- `[RelayCommand]` - Auto-generated ICommand implementations

## Base ViewModel

All ViewModels inherit from `BaseViewModel` which provides:

```csharp
public abstract partial class BaseViewModel : ObservableObject
{
    // Loading state
    [ObservableProperty]
    private bool _isBusy;
    
    // Error handling
    [ObservableProperty]
    private string? _errorMessage;
    
    // Async execution with error handling
    protected async Task ExecuteAsync(Func<Task> operation);
    
    // Navigation hooks
    public virtual Task OnNavigatedToAsync();
    public virtual Task OnNavigatedFromAsync();
}
```

## ViewModels

| ViewModel | Purpose | Key Features |
|-----------|---------|--------------|
| `DashboardViewModel` | Dashboard metrics | Product/order stats, best sellers, low stock alerts |
| `ProductsViewModel` | Product listing | Search, filter, pagination, CRUD commands |
| `OrdersViewModel` | Order management | Filters, promotion integration, status updates |

## Dependency Injection

All ViewModels are registered in `App.xaml.cs`:

```csharp
services.AddTransient<DashboardViewModel>();
services.AddTransient<ProductsViewModel>();
services.AddTransient<OrdersViewModel>();
```

## Usage in Views

Views can access ViewModels via DI:

```csharp
public partial class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel;
    
    public DashboardPage()
    {
        InitializeComponent();
        var app = (App)Application.Current;
        _viewModel = app.Services.GetRequiredService<DashboardViewModel>();
        DataContext = _viewModel;
    }
}
```

## Commands

Use `[RelayCommand]` attribute to generate ICommand properties:

```csharp
[RelayCommand]
private async Task LoadProductsAsync()
{
    // Implementation
}
// Generates: public IAsyncRelayCommand LoadProductsCommand { get; }
```

## File Structure

```
ViewModels/
├── BaseViewModel.cs      # Base class for all ViewModels
├── DashboardViewModel.cs # Dashboard page logic
├── ProductsViewModel.cs  # Products page logic
├── OrdersViewModel.cs    # Orders page logic
└── MainViewModel.cs      # Main window (existing)
```
