using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public class OrderSearchCriteria
    {
        public string? Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public OrderStatus? Status { get; set; }
        public int? MinAmount { get; set; }
        public int? MaxAmount { get; set; }
        public int? CustomerId { get; set; }
        public string SortBy { get; set; } = "DateDesc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public interface IOrderService
    {
        Task<PagedResult<Order>> GetOrdersAsync(OrderSearchCriteria criteria);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderWithItemsAsync(int id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> DeleteOrderAsync(int id);
        
        // Dashboard queries
        Task<List<Order>> GetRecentOrdersAsync(int count = 3);
        Task<int> GetTodayOrderCountAsync();
        Task<int> GetTodayRevenueAsync();
        Task<(int Created, int Paid, int Cancelled)> GetOrderCountByStatusAsync(DateTime? from = null, DateTime? to = null);
        
        // Reports
        Task<List<(DateTime Date, int Revenue, int Profit)>> GetRevenueByDateAsync(DateTime from, DateTime to);
        Task<List<(string ProductName, int Quantity)>> GetProductSalesByDateAsync(DateTime from, DateTime to);
    }
}
