using System.Threading.Tasks;
using Fluid;
using OrchardCore.Liquid;

namespace OrchardCore.Settings.Services
{
    public class SiteLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly ISiteService _siteService;

        public SiteLiquidTemplateEventHandler(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task RenderingAsync(TemplateContext context)
        {
            var site = await _siteService.GetSiteSettingsAsync();
            context.MemberAccessStrategy.Register(site.GetType());
            context.LocalScope.SetValue("Site", site);
        }
    }
}
