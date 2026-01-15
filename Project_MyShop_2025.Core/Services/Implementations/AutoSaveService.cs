using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Project_MyShop_2025.Core.Services.Interfaces;

namespace Project_MyShop_2025.Core.Services.Implementations
{
    public class AutoSaveService : IAutoSaveService
    {
        private readonly string _basePath;

        public AutoSaveService(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task SaveDraftAsync<T>(string key, T data)
        {
            try
            {
                var filePath = Path.Combine(_basePath, $"{key}.json");
                var json = JsonSerializer.Serialize(data);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoSave failed: {ex.Message}");
            }
        }

        public async Task<T?> LoadDraftAsync<T>(string key)
        {
            try
            {
                var filePath = Path.Combine(_basePath, $"{key}.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDraft failed: {ex.Message}");
            }
            return default;
        }

        public Task ClearDraftAsync(string key)
        {
            try
            {
                var filePath = Path.Combine(_basePath, $"{key}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearDraft failed: {ex.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
