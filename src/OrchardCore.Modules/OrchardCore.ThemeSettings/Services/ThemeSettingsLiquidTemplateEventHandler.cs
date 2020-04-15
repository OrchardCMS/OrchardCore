using System.Threading.Tasks;
using Fluid;
using OrchardCore.Entities;
using OrchardCore.Liquid;
using OrchardCore.ThemeSettings.Models;

namespace OrchardCore.Settings.Services
{
    public class ThemeSettingsLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly ISiteService _siteService;

        public ThemeSettingsLiquidTemplateEventHandler(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task RenderingAsync(TemplateContext context)
        {
            var themeSettings = (await _siteService.GetSiteSettingsAsync()).As<CustomThemeSettings>();
            context.MemberAccessStrategy.Register(themeSettings.GetType());
            context.SetValue("ThemeSettings", themeSettings);
        }
    }
}
