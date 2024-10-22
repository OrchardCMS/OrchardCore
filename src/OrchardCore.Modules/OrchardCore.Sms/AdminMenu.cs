using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Sms.Controllers;

namespace OrchardCore.Sms;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["SMS Test"], S["SMS Test"].PrefixPosition(), sms => sms
                    .AddClass("smstest")
                    .Id("smstest")
                    .Action(nameof(AdminController.Test), typeof(AdminController).ControllerName(), "OrchardCore.Sms")
                    .Permission(SmsPermissions.ManageSmsSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
