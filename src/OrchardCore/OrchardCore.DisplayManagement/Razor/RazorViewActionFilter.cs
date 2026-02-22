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

        Task<ISite> getSiteSettingsTask = null;
        Task<Zones.IZoneHolding> layoutAccessorTask = null;
        Task<Environment.Extensions.IExtensionInfo> themeTask = null;

        if (razorViewFeature.Site is null)
        {
            var shellSettings = context.HttpContext.RequestServices.GetService<ShellSettings>();
            var siteService = context.HttpContext.RequestServices.GetService<ISiteService>();

            // 'ISiteService' may be null during a setup and can't be used if the tenant is 'Uninitialized'.
            if (siteService is not null && !shellSettings.IsUninitialized())
            {
                getSiteSettingsTask = siteService.GetSiteSettingsAsync();

                if (getSiteSettingsTask.IsCompletedSuccessfully)
                {
                    razorViewFeature.Site = getSiteSettingsTask.Result;
                    getSiteSettingsTask = null;
                }
            }
        }

        if (razorViewFeature.ThemeLayout is null)
        {
            var layoutAccessor = context.HttpContext.RequestServices.GetService<ILayoutAccessor>();

            if (layoutAccessor is not null)
            {
                layoutAccessorTask = layoutAccessor.GetLayoutAsync();

                if (layoutAccessorTask.IsCompletedSuccessfully)
                {
                    razorViewFeature.ThemeLayout = layoutAccessorTask.Result;
                    layoutAccessorTask = null;
                }
            }
        }

        if (razorViewFeature.Theme is null)
        {
            var themeManager = context.HttpContext.RequestServices.GetService<IThemeManager>();

            if (themeManager is not null)
            {
                themeTask = themeManager.GetThemeAsync();

                if (themeTask.IsCompletedSuccessfully)
                {
                    razorViewFeature.Theme = themeTask.Result;
                    themeTask = null;
                }
            }
        }

        if (getSiteSettingsTask is not null ||
            layoutAccessorTask is not null ||
            themeTask is not null)
        {
            return Awaited(razorViewFeature, getSiteSettingsTask, layoutAccessorTask, themeTask);
        }

        return Task.CompletedTask;

        static async Task Awaited(RazorViewFeature razorViewFeature, Task<ISite> getSiteSettingsTask, Task<Zones.IZoneHolding> layoutAccessorTask, Task<Environment.Extensions.IExtensionInfo> themeTask)
        {
            if (getSiteSettingsTask is not null)
            {
                razorViewFeature.Site = await getSiteSettingsTask;
            }

            if (layoutAccessorTask is not null)
            {
                razorViewFeature.ThemeLayout = await layoutAccessorTask;
            }

            if (themeTask is not null)
            {
                razorViewFeature.Theme = await themeTask;
            }
        }
    }
}
