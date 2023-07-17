using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Https
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
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["HTTPS"], S["HTTPS"].PrefixPosition(), entry => entry
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "Https" })
                            .Permission(Permissions.ManageHttps)
                            .LocalNav()
                        ))
                );

            return Task.CompletedTask;
        }
    }
}
