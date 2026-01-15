using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace Project_MyShop_2025.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels in the application.
    /// Provides common functionality including:
    /// - Property change notification via ObservableObject
    /// - Loading state management
    /// - Error handling
    /// - Async command support
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject
    {
        #region Properties

        /// <summary>
        /// Indicates whether the ViewModel is currently loading data.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        /// <summary>
        /// Inverse of IsBusy for convenient binding.
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        /// <summary>
        /// Title of the current view.
        /// </summary>
        [ObservableProperty]
        private string _title = string.Empty;

        /// <summary>
        /// Error message to display to the user.
        /// </summary>
        [ObservableProperty]
        private string? _errorMessage;

        /// <summary>
        /// Indicates whether there is an error.
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        #endregion

        #region Constructor

        protected BaseViewModel()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute an async operation with loading state and error handling.
        /// </summary>
        /// <param name="operation">The async operation to execute</param>
        /// <param name="onError">Optional error handler</param>
        protected async Task ExecuteAsync(Func<Task> operation, Action<Exception>? onError = null)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                onError?.Invoke(ex);
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Execute an async operation that returns a value.
        /// </summary>
        protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, Action<Exception>? onError = null)
        {
            if (IsBusy)
                return default;

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                return await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                onError?.Invoke(ex);
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error: {ex.Message}");
                return default;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Called when the view is navigated to. Override to load data.
        /// </summary>
        public virtual Task OnNavigatedToAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the view is navigated away from. Override to cleanup.
        /// </summary>
        public virtual Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clear any error message.
        /// </summary>
        protected void ClearError()
        {
            ErrorMessage = null;
        }

        #endregion
    }
}
