using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
