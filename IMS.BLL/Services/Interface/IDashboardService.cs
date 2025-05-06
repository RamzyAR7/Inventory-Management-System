using IMS.BLL.DTOs;

namespace IMS.BLL.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
