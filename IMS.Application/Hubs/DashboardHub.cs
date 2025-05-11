using IMS.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace IMS.Application.Hubs
{
    public class DashboardHub:Hub
    {
        public async Task UpdateDashboard(DashboardDto data)
        {
            await Clients.All.SendAsync("UpdateDashboard", data);
        }
    }
}
