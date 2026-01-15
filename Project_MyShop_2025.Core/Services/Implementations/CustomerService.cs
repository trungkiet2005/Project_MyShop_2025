using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ShopDbContext _context;

        public CustomerService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Customer>> GetCustomersAsync(CustomerSearchCriteria criteria)
        {
            var query = _context.Customers.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(keyword) || 
                    (c.Phone != null && c.Phone.Contains(keyword)) ||
                    (c.Email != null && c.Email.ToLower().Contains(keyword)));
            }

            if (criteria.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == criteria.IsActive.Value);
            }

            // Apply sorting
            query = criteria.SortBy switch
            {
                "NameDesc" => query.OrderByDescending(c => c.Name),
                "CreatedAtDesc" => query.OrderByDescending(c => c.CreatedAt),
                "CreatedAtAsc" => query.OrderBy(c => c.CreatedAt),
                "PointsDesc" => query.OrderByDescending(c => c.LoyaltyPoints),
                "PointsAsc" => query.OrderBy(c => c.LoyaltyPoints),
                _ => query.OrderBy(c => c.Name) // NameAsc default
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToListAsync();

            return new PagedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedAt = System.DateTime.Now;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            // Soft delete - just mark as inactive
            customer.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddLoyaltyPointsAsync(int customerId, int points)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            customer.LoyaltyPoints += points;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeductLoyaltyPointsAsync(int customerId, int points)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            if (customer.LoyaltyPoints < points)
                return false; // Not enough points

            customer.LoyaltyPoints -= points;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalSpentAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Paid)
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<int> GetOrderCountAsync(int customerId)
        {
            return await _context.Orders
                .CountAsync(o => o.CustomerId == customerId);
        }
    }
}
