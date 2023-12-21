using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Azure.CognitiveSearch;

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
                .AddClass("azurecognitiveservice")
                .Id("azurecognitiveservice")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), indexing => indexing
                    .Add(S["Azure Cognitive Indices"], S["Azure Cognitive Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", new { area = "OrchardCore.Search.Azure.CognitiveSearch" })
                        .Permission(AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes)
                        .LocalNav()
                        )
                    )
                );

        return Task.CompletedTask;
    }
}
