using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class ChartsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ChartJs() => View();
    }
}
