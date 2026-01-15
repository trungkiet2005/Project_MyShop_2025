using Microsoft.UI.Xaml.Controls;
using Project_MyShop_2025.Core.Models;
using System.Linq;

namespace Project_MyShop_2025.Views.PrintTemplates
{
    public sealed partial class OrderInvoicePage : Page
    {
        public OrderInvoicePage(Order order)
        {
            this.InitializeComponent();
            LoadOrder(order);
        }

        private void LoadOrder(Order order)
        {
            OrderIdText.Text = $"Order #{order.Id}";
            OrderDateText.Text = order.CreatedAt.ToString("MMMM dd, yyyy");

            CustomerNameText.Text = order.CustomerName ?? "Guest";
            CustomerAddressText.Text = order.CustomerAddress ?? "No address provided";
            CustomerPhoneText.Text = order.CustomerPhone ?? "";

            // Format items
            // We need to ensure OrderItem has these formatted properties or we use a view model.
            // But OrderItem is a model. Let's make sure it defaults gracefully.
            // For simplicity, we assume OrderItem is populated.
            
            // To make x:Bind work with simple formatting in DataTemplate, we should check OrderItem extension or helper.
            // Actually x:Bind directly to model properties. Model properties like FormattedTotal might be needed.
            // Let's create a wrapper or just trust the model extensions if present.
            // Review OrderItem model: it has Product, Quantity, Price, TotalPrice.
            // I used "PriceFormatted" and "TotalPriceFormatted" in XAML, let's verify if they exist in OrderItem or I need to add them.
            // I haven't added them to the model. I should probably add them to the Model or use a ViewModel.
            // Since this is a template, I'll use a dynamic object or update the model.
            // Or better, I'll update the ItemSource to use an anonymous type or wrapper.
            // But x:DataType="models:OrderItem" expects OrderItem.
            // So I should UPDATE OrderItem model to have these helper properties (NotMapped).
            // OR change DataTemplate to use x:Bind Function.
            
            // Let's stick with updating the Model (easiest for now) or use a helper class.
            // I'll update OrderItem in Core to have [NotMapped] properties for formatting if not already present.
            // Let me check OrderItem content again via 'view_file' if needed. 
            // I remember modifying Order.cs but not OrderItem.cs much.
            
            ItemsList.ItemsSource = order.Items;

            SubTotalText.Text = $"₫{order.Items.Sum(i => i.TotalPrice):N0}";
            DiscountText.Text = order.DiscountAmount > 0 ? $"-₫{order.DiscountAmount:N0}" : "₫0";
            TotalText.Text = $"₫{order.TotalPrice:N0}";
        }
    }
}
