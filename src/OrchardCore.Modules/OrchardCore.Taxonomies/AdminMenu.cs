using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Settings;

namespace OrchardCore.Taxonomies
{
    public class AdminMenu : INavigationProvider
    {
#pragma warning disable IDE1006 // Naming Styles
        private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

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
                       .Add(S["Settings"], "1", settings => settings
                            .Add(S["Taxonomy Filters"], S["Taxonomy Filters"].PrefixPosition(), admt => admt
                            .AddClass("taxonomyfilters").Id("taxonomyfilters")
                                .Permission(Permissions.ManageTaxonomies)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TaxonomyContentsAdminListSettingsDisplayDriver.GroupId })
                                .LocalNav()
                    )));

            return Task.CompletedTask;
        }
    }
}
