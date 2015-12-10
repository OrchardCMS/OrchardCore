using Microsoft.AspNet.Mvc;
using Orchard.Themes.Services;
using System.Threading.Tasks;

namespace Orchard.Demo.Controllers
{
    public class ThemeController : Controller
    {
        private readonly ISiteThemeService _siteThemeService;

        public ThemeController(ISiteThemeService siteThemeService)
        {
            _siteThemeService = siteThemeService;
        }

        public async Task<ActionResult> Index()
        {
            var themeName = await _siteThemeService.GetCurrentThemeNameAsync();
            return View(model: themeName);
        }

        [HttpPost]
        public async Task<ActionResult> Index(string themeName)
        {
            await _siteThemeService.SetSiteThemeAsync(themeName);
            return RedirectToAction("Index");
        }
    }
}