using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Controllers
{
    public class DemoController : Controller
    {
        [Route("Demo")]
        [Route("Demo/Index")]
        public IActionResult Index()
        {
          var ceshi=2;
            return Content("Index content");
        }
        [Route("Demo/About")]
        public IActionResult About()
        {
            return Content("About content");
        }
        [Route("Demo/Contact")]
        public IActionResult Contact()
        {
            return Content("Contact content");
        }
    }
}
