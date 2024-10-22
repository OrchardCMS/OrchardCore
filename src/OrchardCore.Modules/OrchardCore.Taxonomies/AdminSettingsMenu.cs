using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Settings;

namespace OrchardCore.Taxonomies;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{

    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], general => general
                .Add(S["Taxonomy Filters"], S["Taxonomy Filters"].PrefixPosition(), filters => filters
                    .AddClass("taxonomyfilters")
                    .Id("taxonomyfilters")
                    .Permission(Permissions.ManageTaxonomies)
                    .Action(GetRouteValues(TaxonomyContentsAdminListSettingsDisplayDriver.GroupId))
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
