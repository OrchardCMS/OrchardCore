using Microsoft.AspNet.Mvc;
using Orchard.DisplayManagement.Admin;
using Orchard.Themes.Models;
using Orchard.Themes.Services;
using System.Threading.Tasks;

namespace Orchard.Demo.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly ISiteThemeService _siteThemeService;

        public AdminController(ISiteThemeService siteThemeService)
        {
            _siteThemeService = siteThemeService;
        }

        public async Task<ActionResult> Index()
        {
            var model = new SelectThemesViewModel
            {
                SiteThemeName = await _siteThemeService.GetCurrentThemeNameAsync(),
                AdminThemeName = await _siteThemeService.GetAdminThemeNameAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(SelectThemesViewModel model)
        {
            await _siteThemeService.SetSiteThemeAsync(model.SiteThemeName);
            await _siteThemeService.SetAdminThemeAsync(model.AdminThemeName);

            return RedirectToAction("Index");
        }
    }
}