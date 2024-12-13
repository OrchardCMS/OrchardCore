using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
