using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security.Drivers;

namespace OrchardCore.Security
{
    public class AdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                    .AddClass("security").Id("security")
                        .Add(S["Settings"], settings => settings
                            .Add(S["Security Headers"], S["Security Headers"].PrefixPosition(), headers => headers
                                .Permission(SecurityPermissions.ManageSecurityHeadersSettings)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SecuritySettingsDisplayDriver.SettingsGroupId })
                                .LocalNav())));
            }

            return Task.CompletedTask;
        }
    }
}
