using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Sms;

namespace OrchardCore.Admin;

public class SmsAdminMenu : INavigationProvider
{
    protected readonly IStringLocalizer S;

    public SmsAdminMenu(IStringLocalizer<SmsAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["SMS"], S["SMS"].PrefixPosition(), sms => sms
                        .AddClass("sms")
                        .Id("sms")
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmsConstants.SettingsGroupId })
                        .Permission(SmsPermissions.ManageSmsSettings)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
