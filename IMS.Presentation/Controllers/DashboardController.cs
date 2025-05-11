using IMS.Application.Hubs;
using IMS.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IMS.Presentation.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IHubContext<DashboardHub> _hubContext;

        public DashboardController(IDashboardService dashboardService, IHubContext<DashboardHub> hubContext)
        {
            _dashboardService = dashboardService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync();

            // Push data to SignalR clients
            await _hubContext.Clients.All.SendAsync("UpdateDashboard", dashboardData);

            return View(dashboardData);
        }
    }
}
