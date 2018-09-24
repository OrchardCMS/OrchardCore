using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Features
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
                .Add(T["Configuration"], "10", configuration => configuration
                    .AddClass("menu-configuration").Id("configuration")
                    .Add(T["Modules"], "6", deployment => deployment
                        .Action("Features", "Admin", new { area = "OrchardCore.Features" })
                        .Permission(Permissions.ManageFeatures)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
