using Inventory_Management_System.Models.DTOs;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
