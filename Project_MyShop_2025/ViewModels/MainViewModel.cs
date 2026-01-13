using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Project_MyShop_2025.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _greeting = "Welcome to Project MyShop 2025!";

        public MainViewModel()
        {
        }
    }
}
