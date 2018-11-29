using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Themes
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
                .Add(T["Themes"], "10", themes => themes
                    .AddClass("themes").Id("themes")
                    .Permission(Permissions.ApplyTheme)
                    .Add(T["Active Themes"], "0", installed => installed
                        .Action("Index", "Admin", new { area = "OrchardCore.Themes" })
                        .Permission(Permissions.ApplyTheme)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
