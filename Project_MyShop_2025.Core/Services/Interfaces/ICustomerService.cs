using Project_MyShop_2025.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public class CustomerSearchCriteria
    {
        public string? Keyword { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "NameAsc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public interface ICustomerService
    {
        Task<PagedResult<Customer>> GetCustomersAsync(CustomerSearchCriteria criteria);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByPhoneAsync(string phone);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        
        // Loyalty operations
        Task<bool> AddLoyaltyPointsAsync(int customerId, int points);
        Task<bool> DeductLoyaltyPointsAsync(int customerId, int points);
        
        // Statistics
        Task<int> GetTotalSpentAsync(int customerId);
        Task<int> GetOrderCountAsync(int customerId);
    }
}
