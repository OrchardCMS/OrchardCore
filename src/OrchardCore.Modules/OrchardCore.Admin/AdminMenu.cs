using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Admin;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", AdminSiteSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Admin"], S["Admin"].PrefixPosition(), admin => admin
                        .AddClass("admin")
                        .Id("admin")
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(AdminPermissions.ManageAdminSettings)
                        .LocalNav()
                    )
                )
            );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Admin"], S["Admin"].PrefixPosition(), admin => admin
                    .AddClass("admin").Id("admin")
                    .Action("Index", "Admin", s_routeValues)
                    .Permission(AdminPermissions.ManageAdminSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
