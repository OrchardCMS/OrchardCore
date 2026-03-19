using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.OpenApi.Drivers;

namespace OrchardCore.OpenApi;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", OpenApiSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Settings"], settings => settings
                .Add(S["OpenApi"], S["OpenApi"].PrefixPosition(), openApi => openApi
                    .Permission(OpenApiPermissions.ApiManage)
                    .Action("Index", "Admin", _routeValues)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
