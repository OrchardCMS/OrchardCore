using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Templates.Controllers
{
    [Admin]
    public class PreviewController : Controller, IUpdateModel
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
