using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Dashboards.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Dashboards;

public sealed class UserDashboardAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", DashboardConstants.Features.Dashboards },
        { "type", DashboardConstants.UserDashboardType },
    };

    internal readonly IStringLocalizer S;

    public UserDashboardAdminMenu(IStringLocalizer<UserDashboardAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["My Dashboard"], "0", myDashboard => myDashboard
                .Id("myDashboard")
                .AddClass("myDashboard")
                .Action(nameof(DashboardController.Index), typeof(DashboardController).ControllerName(), s_routeValues)
                .Permission(UserDashboardPermissions.ManageOwnDashboard)
                .LocalNav()
            );

        return ValueTask.CompletedTask;
    }
}
