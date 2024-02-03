using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Email.Smtp;

public class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SmtpSettingsDisplayDriver.GroupId },
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
                        .Add(S["SMTP Settings"], S["SMTP Settings"].PrefixPosition(), entry => entry
                           .Action("Index", "Admin", _routeValues)
                           .Permission(Permissions.ManageSmtpEmailSettings)
                           .LocalNav()
                        )
                    )
                )
            );

        return Task.CompletedTask;
    }
}
