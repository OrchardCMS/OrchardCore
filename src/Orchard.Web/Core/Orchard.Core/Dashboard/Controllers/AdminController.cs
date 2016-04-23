using Microsoft.AspNetCore.Mvc;
using Orchard.DisplayManagement.Admin;

namespace Orchard.Core.Dashboard
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
