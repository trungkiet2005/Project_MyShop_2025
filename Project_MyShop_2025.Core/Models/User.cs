using System;
using System.ComponentModel.DataAnnotations;

namespace Project_MyShop_2025.Core.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        public string? FullName { get; set; }
        
        public string? Email { get; set; }
        
        public string? Phone { get; set; }
        
        public string Role { get; set; } = "User"; // Admin, User
    }
}
