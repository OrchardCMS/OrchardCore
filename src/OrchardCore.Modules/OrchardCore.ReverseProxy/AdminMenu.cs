using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.ReverseProxy
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder
                    .Add(T["Configuration"], configuration => configuration
                        .Add(T["Settings"], settings => settings
                            .Add(T["Reverse Proxy"], T["Reverse Proxy"], entry => entry
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "ReverseProxy" })
                                .Permission(Permissions.ReverseProxySettings)
                                .LocalNav()
                            )
                        )
                    );
            }

            return Task.CompletedTask;
        }
    }
}
