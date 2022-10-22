using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Search.Drivers;

namespace OrchardCore.Search
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
                .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                    .AddClass("search").Id("search")
                    .Add(S["Settings"], settings => settings
                        .Add(S["Search"], S["Search"].PrefixPosition(), entry => entry
                             .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SearchSettingsDisplayDriver.GroupId })
                             .Permission(Permissions.ManageSearchSettings)
                             .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
