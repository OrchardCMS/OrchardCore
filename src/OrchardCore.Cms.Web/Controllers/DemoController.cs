using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Controllers
{
    public class DemoController : Controller
    {
        [Route("Demo")]
        [Route("Demo/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}