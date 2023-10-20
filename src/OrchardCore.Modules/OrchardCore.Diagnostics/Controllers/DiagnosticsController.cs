using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Diagnostics.ViewModels;

namespace OrchardCore.Diagnostics.Controllers;

public class DiagnosticsController : Controller
{
    [IgnoreAntiforgeryToken]
    public IActionResult Error(int? status)
    {
        // Most commonly used error messages.
        ViewData["StatusCode"] = status;

        if (!status.HasValue || !Enum.TryParse<HttpStatusCode>(status.ToString(), out var httpStatusCode))
        {
            httpStatusCode = HttpStatusCode.InternalServerError;
        }

        return View(new HttpStatusCodeShapeViewModel(httpStatusCode));
    }
}
