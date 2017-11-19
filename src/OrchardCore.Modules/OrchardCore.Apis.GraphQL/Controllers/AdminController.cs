using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Settings.Controllers
{
    [Route("admin/graphql")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}