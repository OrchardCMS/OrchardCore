using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Navigation;

namespace OrchardCore.AdminDashboard.Services;

/// <summary>
/// Adds the dashboard at the beginning of every admin breadcrumb trail.
/// </summary>
/// <remarks>
/// The provider is registered by the <c>OrchardCore.AdminDashboard</c> feature, and it reacts to every trail rather
/// than to one of them. The trails described by the other modules therefore gain the node when the feature is enabled,
/// and lose it when it is disabled, without any of them knowing that the dashboard exists.
/// </remarks>
public sealed class DashboardBreadcrumbProvider : AdminBreadcrumbProvider
{
    private readonly AdminOptions _adminOptions;

    internal readonly IStringLocalizer S;

    public DashboardBreadcrumbProvider(
        IOptions<AdminOptions> adminOptions,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<DashboardBreadcrumbProvider> stringLocalizer)
        : base(httpContextAccessor)
    {
        _adminOptions = adminOptions.Value;
        S = stringLocalizer;
    }

    // The dashboard is the root of the admin only. A trail rendered by a front end theme doesn't lead to it, which is
    // what inheriting from AdminBreadcrumbProvider takes care of.
    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        // 'start' always sorts first, so the node leads the trail whatever position its other nodes use.
        builder.Add(S["Dashboard"], "start", item => item
            .Id("Dashboard")
            .Url("~/" + _adminOptions.AdminUrlPrefix)
            .Permission(Permissions.AccessAdminDashboard));

        return ValueTask.CompletedTask;
    }
}
