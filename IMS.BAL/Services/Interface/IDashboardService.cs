using IMS.BAL.DTOs;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
