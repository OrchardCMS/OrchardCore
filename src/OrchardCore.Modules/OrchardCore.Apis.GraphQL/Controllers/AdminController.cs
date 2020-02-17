using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.GraphQL.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
