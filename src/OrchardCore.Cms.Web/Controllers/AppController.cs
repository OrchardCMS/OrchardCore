using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Cms.Web.Controllers
{
    public class AppController : Controller
    {
        public IActionResult Test()
        {
            return View();
        }
    }
}