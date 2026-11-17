using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Themes.TheAdmin.Controllers;

namespace OrchardCore.Themes.TheAdmin.Navigation;

/// <summary>
/// Adds the schemes of the admin under Design, beside Themes.
/// </summary>
public sealed class AdminSchemesAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "TheAdmin" },
        { "controller", typeof(SchemesController).ControllerName() },
        { "action", nameof(SchemesController.Index) },
    };

    internal readonly IStringLocalizer S;

    public AdminSchemesAdminMenu(IStringLocalizer<AdminSchemesAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Design"], design => design
                .Add(S["Admin schemes"], S["Admin schemes"].PrefixPosition(), schemes => schemes
                    .AddClass("admin-schemes")
                    .Id("adminSchemes")
                    .Action(s_routeValues)
                    .Permission(AdminPermissions.ManageAdminSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
