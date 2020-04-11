using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Diagnostics.Controllers
{
    public class DiagnosticsController : Controller
    {
        public IActionResult Error(int? status)
        {
            // Most commonly used error messages
            ViewData["StatusCode"] = status;
            Enum.TryParse((status ?? 500).ToString(), true, out HttpStatusCode httpStatusCode);


            switch (httpStatusCode)
            {
                case HttpStatusCode.InternalServerError:
                default:
                    return View("Error");
                case HttpStatusCode.Forbidden:
                    return View("Forbidden");
                case HttpStatusCode.NotFound:
                    return View("NotFound");
                case HttpStatusCode.BadRequest:
                    return View("BadRequest");
                case HttpStatusCode.Unauthorized:
                    return View("Unauthorized");

            }
        }
    }
}
