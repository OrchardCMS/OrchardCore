using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
