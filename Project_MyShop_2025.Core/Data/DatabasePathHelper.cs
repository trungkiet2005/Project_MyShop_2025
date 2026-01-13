using System;
using System.IO;

namespace Project_MyShop_2025.Core.Data
{
    public static class DatabasePathHelper
    {
        private static string? _customDatabasePath;

        /// <summary>
        /// Set đường dẫn database tùy chỉnh (từ UI project)
        /// </summary>
        public static void SetDatabasePath(string path)
        {
            _customDatabasePath = path;
        }

        /// <summary>
        /// Lấy đường dẫn tuyệt đối đến file database
        /// </summary>
        public static string GetDatabasePath()
        {
            // Nếu đã set custom path, dùng nó
            if (!string.IsNullOrEmpty(_customDatabasePath))
            {
                return _customDatabasePath;
            }

            // Mặc định: sử dụng thư mục hiện tại của app
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(currentDir, "myshop.db");
            return dbPath;
        }

        /// <summary>
        /// Lấy connection string cho SQLite
        /// </summary>
        public static string GetConnectionString()
        {
            var dbPath = GetDatabasePath();
            // Đảm bảo thư mục tồn tại
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return $"Data Source={dbPath}";
        }
    }
}

