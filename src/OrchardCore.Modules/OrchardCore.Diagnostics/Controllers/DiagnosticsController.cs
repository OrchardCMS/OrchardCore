using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Diagnostics.Controllers
{
    public class DiagnosticsController : Controller
    {
        [IgnoreAntiforgeryToken]
        public IActionResult Error(int? status)
        {
            // Most commonly used error messages.
            ViewData["StatusCode"] = status;
            Enum.TryParse((status ?? 500).ToString(), true, out HttpStatusCode httpStatusCode);


            return httpStatusCode switch
            {
                HttpStatusCode.Forbidden => View("Forbidden"),
                HttpStatusCode.NotFound => View("NotFound"),
                HttpStatusCode.BadRequest => View("BadRequest"),
                HttpStatusCode.Unauthorized => View("Unauthorized"),
                _ => View("Error"),
            };
        }
    }
}
