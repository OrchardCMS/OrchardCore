using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates
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
                    .Add(S["Templates"], S["Templates"].PrefixPosition(), import => import
                        .Action("Index", "Template", new { area = "OrchardCore.Templates" })
                        .Permission(Permissions.ManageTemplates)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
