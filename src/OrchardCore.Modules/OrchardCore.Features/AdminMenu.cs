using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Features
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
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Configuration"], "100", configuration => configuration
                    .AddClass("menu-configuration").Id("configuration")
                    .Add(T["Features"], "1.2", deployment => deployment
                        .Action("Features", "Admin", new { area = "OrchardCore.Features" })
                        .Permission(Permissions.ManageFeatures)
                        .LocalNav()
                    ), priority: 1
                );

            return Task.CompletedTask;
        }
    }
}
