using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Razor
{
    /// <summary>
    /// Inject commonly used data through an HttpContext feature <see cref="RazorViewFeature"/> such that
    /// e.g a <see cref="RazorPage"/> can reuse them when it's executed.
    /// </summary>
    public class RazorViewResultFilter : IAsyncViewResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await OnResultExecutionAsync(context);
            await next();
        }

        // Used as a service when we create a fake 'ActionContext'.
        public async Task OnResultExecutionAsync(ActionContext context)
        {
            var razorViewFeature = context.HttpContext.Features.Get<RazorViewFeature>();

            if (razorViewFeature == null)
            {
                razorViewFeature = new RazorViewFeature();
                context.HttpContext.Features.Set(razorViewFeature);
            }

            if (razorViewFeature.Site == null)
            {
                var siteService = context.HttpContext.RequestServices.GetService<ISiteService>();

                // siteService can be null during Setup
                if (siteService != null)
                {
                    razorViewFeature.Site = await siteService.GetSiteSettingsAsync();
                }
            }

            if (razorViewFeature.ThemeLayout == null)
            {
                var layoutAccessor = context.HttpContext.RequestServices.GetService<ILayoutAccessor>();

                if (layoutAccessor != null)
                {
                    razorViewFeature.ThemeLayout = await layoutAccessor.GetLayoutAsync();
                }
            }

            if (razorViewFeature.Theme == null)
            {
                var themeManager = context.HttpContext.RequestServices.GetService<IThemeManager>();

                if (themeManager != null)
                {
                    razorViewFeature.Theme = await themeManager.GetThemeAsync();
                }
            }
        }
    }
}
