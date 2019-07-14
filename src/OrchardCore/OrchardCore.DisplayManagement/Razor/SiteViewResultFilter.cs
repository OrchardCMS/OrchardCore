using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Razor
{
    /// <summary>
    /// Inject an instance of <see cref="ISite"/> in the HttpContext items such that
    /// a View can reuse it when it's executed.
    /// </summary>
    public class SiteViewResultFilter : IAsyncViewResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await OnResultExecutionAsync(context);
            await next();
        }

        // Used as a service when we create a fake 'ActionContext'.
        public async Task OnResultExecutionAsync(ActionContext context)
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
        }
    }
}
