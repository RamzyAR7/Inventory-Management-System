using Inventory_Management_System.BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentsController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        public async Task<IActionResult> Index()
        {
            var shipments = await _shipmentService.GetAllAsync();
            return View(shipments);
        }
    }
}
