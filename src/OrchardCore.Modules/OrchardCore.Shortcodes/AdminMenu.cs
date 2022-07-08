using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Shortcodes
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
                    .Add(S["Shortcodes"], S["Shortcodes"].PrefixPosition(), import => import
                        .Action("Index", "Admin", new { area = "OrchardCore.Shortcodes" })
                        .Permission(Permissions.ManageShortcodeTemplates)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
