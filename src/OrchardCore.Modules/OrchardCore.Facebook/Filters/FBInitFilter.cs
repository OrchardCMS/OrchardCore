using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Filters
{
    public class FBInitFilter : IAsyncResultFilter
    {
        private IResourceManager _resourceManager;
        private ISiteService _siteService;
        
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                // Resolve scoped services lazy if we got this far.
                _siteService ??= context.HttpContext.RequestServices.GetRequiredService<ISiteService>();

                var site = (await _siteService.GetSiteSettingsAsync());
                var settings = site.As<FacebookSettings>();
                if (!string.IsNullOrWhiteSpace(settings?.AppId))
                {
                    if (settings.FBInit)
                    {
                        // Resolve scoped services lazy if we got this far.
                        _resourceManager ??= context.HttpContext.RequestServices.GetRequiredService<IResourceManager>();

                        var setting = _resourceManager.RegisterResource("script", "fb");
                        setting.AtLocation(ResourceLocation.Foot);
                    }
                }
            }

            await next.Invoke();
        }
    }
}
