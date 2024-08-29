using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Controllers;
using OrchardCore.Email.Core;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Email;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", EmailSettings.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                   .Add(S["Email"], S["Email"].PrefixPosition(), entry => entry
                      .AddClass("email")
                      .Id("email")
                      .Action("Index", "Admin", _routeValues)
                      .Permission(Permissions.ManageEmailSettings)
                      .LocalNav()
                    )
                   .Add(S["Email Test"], S["Email Test"].PrefixPosition(), entry => entry
                      .AddClass("emailtest")
                      .Id("emailtest")
                      .Action(nameof(AdminController.Test), typeof(AdminController).ControllerName(), "OrchardCore.Email")
                      .Permission(Permissions.ManageEmailSettings)
                      .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
