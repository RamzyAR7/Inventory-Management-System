using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
