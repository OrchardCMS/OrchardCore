using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Admin.Controllers
{
    [Authorize]
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
