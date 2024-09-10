using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace OrchardCore.Apis.GraphQL.Controllers;

public sealed class AdminController : Controller
{
    [HttpGet]
    [Admin("GraphQL", "GraphQL")]
    public IActionResult Index()
    {
        return View();
    }
}
