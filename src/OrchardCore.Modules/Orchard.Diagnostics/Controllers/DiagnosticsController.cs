using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Diagnostics.Controllers
{
    public class DiagnosticsController : Controller
    {
        public IActionResult Error(int? status)
        {
            ViewData["StatusCode"] = status;

            if (status == 404)
            {
                return View("NotFound");
            }

            return View("Error");
        }
    }
}
