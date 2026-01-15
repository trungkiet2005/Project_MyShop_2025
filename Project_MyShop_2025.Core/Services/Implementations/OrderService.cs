using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ShopDbContext _context;

        public OrderService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(OrderSearchCriteria criteria)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword.ToLower();
                query = query.Where(o => 
                    o.Id.ToString().Contains(keyword) ||
                    (o.CustomerName != null && o.CustomerName.ToLower().Contains(keyword)) ||
                    (o.CustomerPhone != null && o.CustomerPhone.Contains(keyword)));
            }

            if (criteria.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= criteria.FromDate.Value);
            }

            if (criteria.ToDate.HasValue)
            {
                var endOfDay = criteria.ToDate.Value.Date.AddDays(1);
                query = query.Where(o => o.CreatedAt < endOfDay);
            }

            if (criteria.Status.HasValue)
            {
                query = query.Where(o => o.Status == criteria.Status.Value);
            }

            if (criteria.MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalPrice >= criteria.MinAmount.Value);
            }

            if (criteria.MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalPrice <= criteria.MaxAmount.Value);
            }

            if (criteria.CustomerId.HasValue)
            {
                // Future: query = query.Where(o => o.CustomerId == criteria.CustomerId.Value);
            }

            // Apply sorting
            query = criteria.SortBy switch
            {
                "DateAsc" => query.OrderBy(o => o.CreatedAt),
                "AmountAsc" => query.OrderBy(o => o.TotalPrice),
                "AmountDesc" => query.OrderByDescending(o => o.TotalPrice),
                _ => query.OrderByDescending(o => o.CreatedAt) // DateDesc default
            };

            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Calculate total price
            order.TotalPrice = order.Items.Sum(i => i.TotalPrice);
            order.CreatedAt = DateTime.Now;
            
            _context.Orders.Add(order);
            
            // Update product quantities
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                }
            }
            
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            // Validate status transition
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Paid)
            {
                // Cannot change status of completed orders (except maybe Paid -> Cancelled for refunds)
                if (order.Status == OrderStatus.Paid && status != OrderStatus.Cancelled)
                    return false;
            }

            order.Status = status;
            
            // If cancelling, restore product quantities
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                var orderWithItems = await GetOrderWithItemsAsync(orderId);
                if (orderWithItems != null)
                {
                    foreach (var item in orderWithItems.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Quantity += item.Quantity;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
                return false;

            // Restore product quantities if order was not cancelled
            if (order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                    }
                }
            }

            _context.OrderItems.RemoveRange(order.Items);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 3)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTodayOrderCountAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _context.Orders
                .CountAsync(o => o.CreatedAt >= today && o.CreatedAt < tomorrow);
        }

        public async Task<int> GetTodayRevenueAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _context.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status == OrderStatus.Paid)
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<(int Created, int Paid, int Cancelled)> GetOrderCountByStatusAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Orders.AsQueryable();

            if (from.HasValue)
                query = query.Where(o => o.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(o => o.CreatedAt <= to.Value);

            var counts = await query
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return (
                Created: counts.FirstOrDefault(c => c.Status == OrderStatus.Created)?.Count ?? 0,
                Paid: counts.FirstOrDefault(c => c.Status == OrderStatus.Paid)?.Count ?? 0,
                Cancelled: counts.FirstOrDefault(c => c.Status == OrderStatus.Cancelled)?.Count ?? 0
            );
        }

        public async Task<List<(DateTime Date, int Revenue, int Profit)>> GetRevenueByDateAsync(DateTime from, DateTime to)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.Status == OrderStatus.Paid)
                .ToListAsync();

            var result = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => (
                    Date: g.Key,
                    Revenue: g.Sum(o => o.TotalPrice),
                    Profit: g.Sum(o => o.Items.Sum(i => (i.Price - (i.Product?.ImportPrice ?? 0)) * i.Quantity))
                ))
                .OrderBy(x => x.Date)
                .ToList();

            return result;
        }

        public async Task<List<(string ProductName, int Quantity)>> GetProductSalesByDateAsync(DateTime from, DateTime to)
        {
            var result = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order != null && 
                             oi.Order.CreatedAt >= from && 
                             oi.Order.CreatedAt <= to &&
                             oi.Order.Status == OrderStatus.Paid)
                .GroupBy(oi => oi.Product!.Name)
                .Select(g => new { ProductName = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync();

            return result.Select(x => (x.ProductName, x.Quantity)).ToList();
        }
    }
}
