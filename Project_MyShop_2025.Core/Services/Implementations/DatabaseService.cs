using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ShopDbContext _context;

        public DatabaseService(ShopDbContext context)
        {
            _context = context;
        }

        public string GetDatabasePath()
        {
            return DatabasePathHelper.GetDatabasePath();
        }

        public async Task<bool> BackupAsync(string backupPath)
        {
            try
            {
                var dbPath = GetDatabasePath();
                
                if (!File.Exists(dbPath))
                {
                    return false;
                }

                // Ensure backup directory exists
                var backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Close connection before copying
                await _context.Database.CloseConnectionAsync();

                // Copy database file
                await Task.Run(() => File.Copy(dbPath, backupPath, overwrite: true));

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backup failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RestoreAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    return false;
                }

                var dbPath = GetDatabasePath();

                // Close connection before restoring
                await _context.Database.CloseConnectionAsync();

                // Create backup of current database before restoring
                var currentBackup = dbPath + ".bak";
                if (File.Exists(dbPath))
                {
                    await Task.Run(() => File.Copy(dbPath, currentBackup, overwrite: true));
                }

                try
                {
                    // Restore from backup
                    await Task.Run(() => File.Copy(backupPath, dbPath, overwrite: true));
                    
                    // Delete temporary backup
                    if (File.Exists(currentBackup))
                    {
                        File.Delete(currentBackup);
                    }

                    return true;
                }
                catch
                {
                    // Restore from temporary backup if restore failed
                    if (File.Exists(currentBackup))
                    {
                        await Task.Run(() => File.Copy(currentBackup, dbPath, overwrite: true));
                        File.Delete(currentBackup);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Restore failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnsureDatabaseCreatedAsync()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureCreated failed: {ex.Message}");
                return false;
            }
        }
    }
}
