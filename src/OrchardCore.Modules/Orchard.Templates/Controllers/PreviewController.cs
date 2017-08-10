using Microsoft.AspNetCore.Mvc;
using Orchard.Admin;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Templates.Controllers
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
