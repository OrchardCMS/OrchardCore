using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.AzureAI;

public class AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer) : INavigationProvider
{
    protected readonly IStringLocalizer S = stringLocalizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("azure-ai-service")
                .Id("azureaiservice")
                .Add(S["Indexing"], S["Indexing"].PrefixPosition(), indexing => indexing
                    .Add(S["Azure AI Indices"], S["Azure AI Indices"].PrefixPosition(), indexes => indexes
                        .Action("Index", "Admin", new { area = "OrchardCore.Search.AzureAI" })
                        .Permission(AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes)
                        .LocalNav()
                        )
                    )
                );

        return Task.CompletedTask;
    }
}
