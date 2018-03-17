using OrchardCore.OpenIdConnect.Drivers;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using System;

namespace OrchardCore.OpenIdConnect
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Security"], "5", security => security
                        .Add(T["External Identity Providers"], "25", providers => providers
                            .Add(T["OpenIdConnect Client"], "5", installed => installed
                             .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = OpenIdConnectSettingsDisplayDriver.SettingsGroupId })
                             .Permission(Permissions.ConfigureOpenIdConnect)
                             .LocalNav()))));
        }
    }
}
