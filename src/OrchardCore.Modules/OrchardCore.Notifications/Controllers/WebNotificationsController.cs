using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Notifications.Controllers;

public class WebNotificationsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
