using Microsoft.AspNetCore.Mvc;
using Orchard.DisplayManagement.Admin;

namespace Orchard.Core.Dashboard.Controllers
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
