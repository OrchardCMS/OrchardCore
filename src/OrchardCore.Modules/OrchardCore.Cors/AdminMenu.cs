using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["CORS"], S["CORS"].PrefixPosition(), entry => entry
                        .AddClass("cors").Id("cors")
                            .Action("Index", "Admin", new { area = "OrchardCore.Cors" })
                            .Permission(Permissions.ManageCorsSettings)
                            .LocalNav()
                        ))
                );

            return Task.CompletedTask;
        }
    }
}
