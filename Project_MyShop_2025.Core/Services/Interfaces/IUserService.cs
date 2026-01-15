using Project_MyShop_2025.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> CreateUserAsync(string username, string password, string? fullName = null, string? email = null, string role = "User");
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> UsernameExistsAsync(string username);
    }
}
