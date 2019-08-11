using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Razor
{
    /// <summary>
    /// Inject an instance of <see cref="ISite"/> in the HttpContext items such that
    /// a View can reuse it when it's executed.
    /// TODO: This should be removed once https://github.com/aspnet/Mvc/issues/8241 is implemented
    /// </summary>
    public class SiteViewResultFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!context.HttpContext.Items.ContainsKey(typeof(ISite)))
            {
                var siteService = context.HttpContext.RequestServices.GetService<ISiteService>();

                // siteService can be null during Setup
                if (siteService != null)
                {
                    context.HttpContext.Items.Add(typeof(ISite), await siteService.GetSiteSettingsAsync());
                }
            }

            await next();
        }
    }
}
