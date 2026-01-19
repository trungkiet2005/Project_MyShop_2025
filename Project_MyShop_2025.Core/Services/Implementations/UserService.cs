using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Helpers;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ShopDbContext _context;

        public UserService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            if (!PasswordHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> CreateUserAsync(string username, string password, string? fullName = null, string? email = null, string role = "User")
        {
            PasswordHelper.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
            
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = fullName,
                Email = email,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (!PasswordHelper.VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt))
                return false;

            PasswordHelper.CreatePasswordHash(newPassword, out string newHash, out string newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
