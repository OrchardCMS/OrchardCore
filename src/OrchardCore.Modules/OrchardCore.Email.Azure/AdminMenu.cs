using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Email.Azure;

public class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Email.Azure" }
    };

    protected readonly IStringLocalizer S;

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
                        .AddClass("email").Id("email")
                        .Add(S["Azure Email Settings"], S["Azure Email Settings"].PrefixPosition(), entry => entry
                            .Action("Options", "Admin", _routeValues)
                            .Permission(Permissions.ViewAzureEmailOptions)
                            .LocalNav()
                        )
                    )
                )
            );

        return Task.CompletedTask;
    }
}
