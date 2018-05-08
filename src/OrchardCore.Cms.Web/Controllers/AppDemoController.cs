using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Cms.Web.Controllers
{
    public class AppDemoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("AppDemo/About")]
        public IActionResult About()
        {
            return Content("About content");
        }
        [Route("AppDemo/Contact")]
        public IActionResult Contact()
        {
            return Content("Contact content");
        }
    }
}