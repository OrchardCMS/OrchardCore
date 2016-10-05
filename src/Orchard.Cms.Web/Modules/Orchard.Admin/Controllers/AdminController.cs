using Microsoft.AspNetCore.Mvc;

namespace Orchard.Admin.Controllers
{
    public class AdminController : Controller
    {
        public AdminController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
