using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security;

namespace OrchardCore.Recipes
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

            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Recipes"], S["Recipes"].PrefixPosition(), recipes => recipes
                    .AddClass("recipes").Id("recipes")
                    .Permission(StandardPermissions.SiteOwner)
                    .Action("Index", "Admin", new { area = "OrchardCore.Recipes" })
                    .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
