using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Cms.Web.Controllers
{
    public class ApplicationController : Controller
    {
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}