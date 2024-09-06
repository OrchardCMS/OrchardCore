using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Settings;

namespace OrchardCore.Taxonomies;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TaxonomyContentsAdminListSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], "1", settings => settings
                    .Add(S["Taxonomy Filters"], S["Taxonomy Filters"].PrefixPosition(), filters => filters
                        .AddClass("taxonomyfilters")
                        .Id("taxonomyfilters")
                        .Permission(Permissions.ManageTaxonomies)
                        .Action("Index", "Admin", _routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
