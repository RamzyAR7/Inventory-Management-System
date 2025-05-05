using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
