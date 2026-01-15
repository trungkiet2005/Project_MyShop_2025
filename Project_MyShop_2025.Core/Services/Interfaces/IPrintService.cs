using Project_MyShop_2025.Core.Models;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public interface IPrintService
    {
        Task PrintOrderAsync(Order order);
        bool IsPrintingSupported { get; }
    }
}
