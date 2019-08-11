using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.TheTheme.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
