using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class WarehousesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
