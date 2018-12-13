using Microsoft.AspNetCore.Mvc;
using OrchardCore.Modules;

namespace HelloWorld.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    [Feature("OrchardCore.Mvc.HelloWorld.All")]
    public class AllController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    [Feature("OrchardCore.Mvc.HelloWorld.Contoso")]
    public class HomeContosoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    [Feature("OrchardCore.Mvc.HelloWorld.Acme")]
    public class HomeAcmeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
