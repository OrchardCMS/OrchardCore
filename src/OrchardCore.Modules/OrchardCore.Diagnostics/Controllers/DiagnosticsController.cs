using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Diagnostics.Controllers
{
    public class DiagnosticsController : Controller
    {
        public IActionResult Error(int? status)
        {
            ViewData["StatusCode"] = status;

            if (status == (int)HttpStatusCode.NotFound)
            {
                return View("NotFound");
            }
            else if (status == (int)HttpStatusCode.Unauthorized)
            {
                return View("Unauthorized");
            }

            return View("Error");
        }
    }
}
