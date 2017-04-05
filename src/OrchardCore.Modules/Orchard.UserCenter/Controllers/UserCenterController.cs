using Microsoft.AspNetCore.Mvc;

namespace Orchard.UserCenter.Controllers
{
    public class UserCenterController : Controller
    {
        public UserCenterController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
