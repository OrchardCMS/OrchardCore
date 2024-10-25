using Microsoft.AspNetCore.Mvc;
using OrchardCore.Diagnostics.ViewModels;

namespace OrchardCore.Diagnostics.Controllers;

public sealed class DiagnosticsController : Controller
{
    [IgnoreAntiforgeryToken]
    public IActionResult Error(int? status)
    {
        // Most commonly used error messages.
        ViewData["StatusCode"] = status;

        return View(new HttpErrorShapeViewModel(status));
    }
}
