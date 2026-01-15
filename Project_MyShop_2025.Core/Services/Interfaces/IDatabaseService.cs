using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public interface IDatabaseService
    {
        string GetDatabasePath();
        Task<bool> BackupAsync(string backupPath);
        Task<bool> RestoreAsync(string backupPath);
        Task<bool> EnsureDatabaseCreatedAsync();
    }
}
