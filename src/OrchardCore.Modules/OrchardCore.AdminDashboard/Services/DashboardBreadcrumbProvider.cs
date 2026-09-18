using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Navigation;

namespace OrchardCore.AdminDashboard.Services;

/// <summary>
/// Adds Dashboard at the beginning of admin breadcrumb trails when the Admin Dashboard feature is enabled.
/// </summary>
public sealed class DashboardBreadcrumbProvider : IBreadcrumbProvider
{
    private readonly AdminOptions _adminOptions;
    private readonly IStringLocalizer S;

    public DashboardBreadcrumbProvider(
        IOptions<AdminOptions> adminOptions,
        IStringLocalizer<DashboardBreadcrumbProvider> localizer)
    {
        _adminOptions = adminOptions.Value;
        S = localizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbContext context)
    {
        if (!AdminAttribute.IsApplied(context.ViewContext.HttpContext)
            || context.Name == "Dashboard"
            || context.Items.Any(item => item.Id == "Dashboard"))
        {
            return ValueTask.CompletedTask;
        }

        context.Items.Insert(0, new BreadcrumbItem
        {
            Id = "Dashboard",
            Text = S["Dashboard"],
            Position = "start",
            Url = "~/" + _adminOptions.AdminUrlPrefix,
            Permissions = { Permissions.AccessAdminDashboard },
        });

        return ValueTask.CompletedTask;
    }
}
