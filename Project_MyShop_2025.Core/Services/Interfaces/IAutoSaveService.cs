using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public interface IAutoSaveService
    {
        Task SaveDraftAsync<T>(string key, T data);
        Task<T?> LoadDraftAsync<T>(string key);
        Task ClearDraftAsync(string key);
    }
}
