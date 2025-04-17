using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
