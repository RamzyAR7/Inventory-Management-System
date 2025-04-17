using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class UIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Buttons() => View();
        public IActionResult Forms() => View();
        public IActionResult Typography() => View();
    }
}
