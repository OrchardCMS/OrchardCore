using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Cms.Web.Controllers
{
    public class AppliController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}