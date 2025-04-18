using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
