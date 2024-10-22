using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.AzureAI;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), indexing => indexing
                    .Add(S["Azure AI Indices"], S["Azure AI Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", "OrchardCore.Search.AzureAI")
                        .AddClass("azureaiindices")
                        .Id("azureaiindices")
                        .Permission(AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
