using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return Content("Demo Admin Index Page.");
        }
    }
}