using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OrchardCore.SuperEdit.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
    public IActionResult List()
        {
            return View();
        }
    }
}
