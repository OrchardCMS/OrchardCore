using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Controllers;

public sealed class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
