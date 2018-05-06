using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Controllers
{
    public class ApplicationController : Controller
    {
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("About")]
        public IActionResult About()
        {
            return Content("Application About content");
        }
        [Route("Contact")]
        public IActionResult Contact()
        {
            return Content("Application Contact content");
        }
    }
}