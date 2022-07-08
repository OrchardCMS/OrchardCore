using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Placements
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

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
                .Add(S["Design"], design => design
                    .Add(S["Placements"], S["Placements"].PrefixPosition(), import => import
                        .Action("Index", "Admin", new { area = "OrchardCore.Placements" })
                        .Permission(Permissions.ManagePlacements)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
