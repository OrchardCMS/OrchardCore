using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Razor;

/// <summary>
/// Inject commonly used data through an HttpContext feature <see cref="RazorViewFeature"/> such that
/// e.g a <see cref="RazorPage"/> can reuse them when it's executed.
/// </summary>
public sealed class RazorViewActionFilter : IAsyncViewActionFilter
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

    public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        static async Task Awaited(Task task, PageHandlerExecutionDelegate next)
        {
            await task;
            await next();
        }

        var task = OnActionExecutionAsync(context);
        return !task.IsCompletedSuccessfully
            ? Awaited(task, next)
            : next();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    // Used as a service when we create a fake 'ActionContext'.
    public Task OnActionExecutionAsync(ActionContext context)
    {
        var razorViewFeature = context.HttpContext.Features.Get<RazorViewFeature>();

        if (razorViewFeature is null)
        {
            razorViewFeature = new RazorViewFeature();
            context.HttpContext.Features.Set(razorViewFeature);
        }

        var services = context.HttpContext.RequestServices;

        if (razorViewFeature.Site is null)
        {
            var shellSettings = services.GetService<ShellSettings>();
            var siteService = services.GetService<ISiteService>();

            // 'ISiteService' may be null during a setup and can't be used if the tenant is 'Uninitialized'.
            if (siteService is not null && !shellSettings.IsUninitialized())
            {
                var getSiteSettingsTask = siteService.GetSiteSettingsAsync();

                if (!getSiteSettingsTask.IsCompletedSuccessfully)
                {
                    // Must go async - execute remaining operations sequentially
                    return AwaitedSiteSettings(razorViewFeature, services, getSiteSettingsTask);
                }

                razorViewFeature.Site = getSiteSettingsTask.Result;
            }
        }

        if (razorViewFeature.ThemeLayout is null)
        {
            var layoutAccessor = services.GetService<ILayoutAccessor>();

            if (layoutAccessor is not null)
            {
                var layoutAccessorTask = layoutAccessor.GetLayoutAsync();

                if (!layoutAccessorTask.IsCompletedSuccessfully)
                {
                    // Must go async - execute remaining operations sequentially
                    return AwaitedLayout(razorViewFeature, services, layoutAccessorTask);
                }

                razorViewFeature.ThemeLayout = layoutAccessorTask.Result;
            }
        }

        // Step 3: Theme
        if (razorViewFeature.Theme is null)
        {
            var themeManager = services.GetService<IThemeManager>();

            if (themeManager is not null)
            {
                var themeTask = themeManager.GetThemeAsync();

                if (!themeTask.IsCompletedSuccessfully)
                {
                    // Must go async
                    return AwaitedTheme(razorViewFeature, themeTask);
                }

                razorViewFeature.Theme = themeTask.Result;
            }
        }

        return Task.CompletedTask;

        static async Task AwaitedSiteSettings(
            RazorViewFeature razorViewFeature,
            IServiceProvider services,
            Task<ISite> getSiteSettingsTask)
        {
            razorViewFeature.Site = await getSiteSettingsTask;

            // Continue with layout
            if (razorViewFeature.ThemeLayout is null)
            {
                var layoutAccessor = services.GetService<ILayoutAccessor>();

                if (layoutAccessor is not null)
                {
                    razorViewFeature.ThemeLayout = await layoutAccessor.GetLayoutAsync();
                }
            }

            // Continue with theme
            if (razorViewFeature.Theme is null)
            {
                var themeManager = services.GetService<IThemeManager>();

                if (themeManager is not null)
                {
                    razorViewFeature.Theme = await themeManager.GetThemeAsync();
                }
            }
        }

        static async Task AwaitedLayout(
            RazorViewFeature razorViewFeature,
            IServiceProvider services,
            Task<Zones.IZoneHolding> layoutAccessorTask)
        {
            razorViewFeature.ThemeLayout = await layoutAccessorTask;

            // Continue with theme
            if (razorViewFeature.Theme is null)
            {
                var themeManager = services.GetService<IThemeManager>();

                if (themeManager is not null)
                {
                    razorViewFeature.Theme = await themeManager.GetThemeAsync();
                }
            }
        }

        static async Task AwaitedTheme(
            RazorViewFeature razorViewFeature,
            Task<Environment.Extensions.IExtensionInfo> themeTask)
        {
            razorViewFeature.Theme = await themeTask;
        }
    }
}
