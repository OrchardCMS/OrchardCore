using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Https
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Security"], security => security
                    .Add(T["Settings"], settings => settings
                        .Add(T["HTTPS"], T["HTTPS"], entry => entry
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "Https" })
                            .Permission(Permissions.ManageHttps)
                            .LocalNav()
                        ))
                );

            return Task.CompletedTask;
        }
    }
}
