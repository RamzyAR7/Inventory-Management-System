using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult Blank()
        {
            return View();
        }
    }
}
