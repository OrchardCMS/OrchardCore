using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Sms.Azure.Controllers;

namespace OrchardCore.Sms.Azure;

public class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SmsSettings.GroupId },
    };

    protected readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
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

        return Task.CompletedTask;
    }
}
