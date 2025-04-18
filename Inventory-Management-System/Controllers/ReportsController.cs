using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
