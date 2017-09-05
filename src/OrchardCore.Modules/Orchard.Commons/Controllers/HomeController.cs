using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Commons.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
