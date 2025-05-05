using IMS.BAL.DTOs;

namespace IMS.BAL.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
