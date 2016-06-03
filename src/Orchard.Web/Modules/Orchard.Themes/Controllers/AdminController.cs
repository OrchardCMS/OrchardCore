using Microsoft.AspNetCore.Mvc;
using Orchard.Admin;
using Orchard.Themes.Models;
using Orchard.Themes.Services;
using System.Threading.Tasks;

namespace Orchard.Themes.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdminThemeService _adminThemeService;

        public AdminController(
            ISiteThemeService siteThemeService,
            IAdminThemeService adminThemeService)
        {
            _siteThemeService = siteThemeService;
            _adminThemeService = adminThemeService;
        }

        public async Task<ActionResult> Index()
        {
            var model = new SelectThemesViewModel
            {
                SiteThemeName = await _siteThemeService.GetCurrentThemeNameAsync(),
                AdminThemeName = await _adminThemeService.GetAdminThemeNameAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(SelectThemesViewModel model)
        {
            await _siteThemeService.SetSiteThemeAsync(model.SiteThemeName);
            await _adminThemeService.SetAdminThemeAsync(model.AdminThemeName);

            return RedirectToAction("Index");
        }
    }
}