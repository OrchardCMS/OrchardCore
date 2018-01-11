using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
          private readonly IShapeFactory _shapeFactory;
          
        public AdminController(IShapeFactory shapeFactory)
        {
            _shapeFactory = shapeFactory;
        }

         public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
