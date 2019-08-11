using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.GraphQL.Controllers
{
    [Route("admin/graphql")]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}