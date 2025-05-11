using IMS.Application.Hubs;
using IMS.Application.Services.Interface;
using IMS.Application.SharedServices.Interface;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.SharedServices.Impelimentation
{
    public class DashboardUpdateNotifier : IDashboardUpdateNotifier
    {
        private readonly IDashboardService _dashboardService;
        private readonly IHubContext<DashboardHub> _hubContext;

        public DashboardUpdateNotifier(IDashboardService dashboardService, IHubContext<DashboardHub> hubContext)
        {
            _dashboardService = dashboardService;
            _hubContext = hubContext;
        }

        public async Task NotifyDashboardUpdateAsync()
        {
            var updatedDashboardData = await _dashboardService.GetDashboardDataAsync();
            await _hubContext.Clients.All.SendAsync("UpdateDashboard", updatedDashboardData);
        }

        public async Task NotifyOrderCreatedAsync() => await NotifyDashboardUpdateAsync();
        public async Task NotifyStockChangedAsync() => await NotifyDashboardUpdateAsync();
        public async Task NotifyTransferCompletedAsync() => await NotifyDashboardUpdateAsync();
    }
}
