using IMS.Application.DTOs;

namespace IMS.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
