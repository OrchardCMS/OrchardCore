using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Menu
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

            var rvd = new RouteValueDictionary
            {
                { "contentTypeId", "Menu" },
                { "Area", "OrchardCore.Contents" },
                { "Options.SelectedContentType", "Menu" },
                { "Options.CanCreateSelectedContentType", true }
            };

            builder.Add(S["Content"], design => design
                    .Add(S["Menus"], S["Menus"].PrefixPosition(), menus => menus
                        .Permission(Permissions.ManageMenu)
                        .Action("List", "Admin", rvd)
                        .LocalNav()
                        ));

            return Task.CompletedTask;
        }
    }
}
