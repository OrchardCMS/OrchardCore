using Microsoft.AspNetCore.Mvc;

namespace Orchard.Commons.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
