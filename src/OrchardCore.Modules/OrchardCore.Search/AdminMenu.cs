using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search
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
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                    .AddClass("search").Id("search")
                    .Add(S["Settings"], S["Settings"].PrefixPosition(), settings => settings
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SearchConstants.SearchSettingsGroupId })
                        .Permission(Permissions.ManageSearchSettings)
                        .LocalNav()
                        )
                    );

            return Task.CompletedTask;
        }
    }
}
