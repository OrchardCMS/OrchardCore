using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security;

namespace OrchardCore.Recipes
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

            builder.Add(T["Configuration"], configuration => configuration
                .AddClass("recipes").Id("recipes")
                .Add(T["Recipes"], "1", recipes => recipes
                    .Permission(StandardPermissions.SiteOwner)
                    .Action("Index", "Admin", new { area = "OrchardCore.Recipes" })
                    .LocalNav())
                );

            return Task.CompletedTask;
        }
    }
}
