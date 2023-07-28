using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Themes
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
                .Add(S["Design"], NavigationConstants.AdminMenuDesignPosition, design => design
                    .AddClass("themes").Id("themes")
                    .Add(S["Themes"], S["Themes"].PrefixPosition(), installed => installed
                        .Action("Index", "Admin", new { area = "OrchardCore.Themes" })
                        .Permission(Permissions.ApplyTheme)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
