using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Sms.Controllers;

namespace OrchardCore.Sms;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SmsSettings.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["SMS"], S["SMS"].PrefixPosition(), sms => sms
                        .AddClass("sms")
                        .Id("sms")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(SmsPermissions.ManageSmsSettings)
                        .LocalNav()
                    )
                    .Add(S["SMS Test"], S["SMS Test"].PrefixPosition(), sms => sms
                        .AddClass("smstest")
                        .Id("smstest")
                        .Action(nameof(AdminController.Test), typeof(AdminController).ControllerName(), "OrchardCore.Sms")
                        .Permission(SmsPermissions.ManageSmsSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
