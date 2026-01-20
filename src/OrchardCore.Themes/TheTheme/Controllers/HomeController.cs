using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.TheTheme.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
