using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
