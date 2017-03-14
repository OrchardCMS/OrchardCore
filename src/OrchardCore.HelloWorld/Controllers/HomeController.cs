using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.HelloWorld.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}