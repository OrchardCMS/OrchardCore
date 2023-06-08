using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Razor
{
    /// <summary>
    /// Inject commonly used data through an HttpContext feature <see cref="RazorViewFeature"/> such that
    /// e.g a <see cref="RazorPage"/> can reuse them when it's executed.
    /// </summary>
    public class RazorViewActionFilter : IAsyncViewActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            static async Task Awaited(Task task, ActionExecutionDelegate next)
            {
                await task;
                await next();
            }

            var task = OnActionExecutionAsync(context);
            return !task.IsCompletedSuccessfully
                ? Awaited(task, next)
                : next();
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            await OnActionExecutionAsync(context);
            await next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        // Used as a service when we create a fake 'ActionContext'.
        public async Task OnActionExecutionAsync(ActionContext context)
        {
            var razorViewFeature = context.HttpContext.Features.Get<RazorViewFeature>();

            if (razorViewFeature is null)
            {
                razorViewFeature = new RazorViewFeature();
                context.HttpContext.Features.Set(razorViewFeature);
            }

            if (razorViewFeature.Site is null)
            {
                var shellSettings = context.HttpContext.RequestServices.GetService<ShellSettings>();
                var siteService = context.HttpContext.RequestServices.GetService<ISiteService>();

                // 'ISiteService' may be null during a setup and can't be used if the tenant is 'Uninitialized'.
                if (siteService is not null && !shellSettings.IsUninitialized())
                {
                    razorViewFeature.Site = await siteService.GetSiteSettingsAsync();
                }
            }

            if (razorViewFeature.ThemeLayout is null)
            {
                var layoutAccessor = context.HttpContext.RequestServices.GetService<ILayoutAccessor>();

                if (layoutAccessor is not null)
                {
                    razorViewFeature.ThemeLayout = await layoutAccessor.GetLayoutAsync();
                }
            }

            if (razorViewFeature.Theme is null)
            {
                var themeManager = context.HttpContext.RequestServices.GetService<IThemeManager>();

                if (themeManager is not null)
                {
                    razorViewFeature.Theme = await themeManager.GetThemeAsync();
                }
            }
        }
    }
}
