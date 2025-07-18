using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace OrchardCore.OpenApi.Controllers;

public sealed class AdminController : Controller
{
    [HttpGet]
    [Admin("OpenApi", "OpenApi")]
    public IActionResult Index()
    {
        return View();
    }
}
