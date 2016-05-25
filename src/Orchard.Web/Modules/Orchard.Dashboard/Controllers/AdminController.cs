using Microsoft.AspNetCore.Mvc;
using Orchard.DisplayManagement.Admin;

namespace Orchard.Dashboard.Controllers
{
    [Admin]
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
