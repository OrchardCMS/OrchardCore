using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Core;
using OrchardCore.Navigation;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", AzureAISearchDefaultSettingsDisplayDriver.GroupId},
    };

    private readonly AzureAISearchDefaultOptions _azureAISearchSettings;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        IOptions<AzureAISearchDefaultOptions> azureAISearchSettings,
        IStringLocalizer<AdminMenu> stringLocalizer)
    {
        _azureAISearchSettings = azureAISearchSettings.Value;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (_azureAISearchSettings.DisableUIConfiguration)
        {
            return ValueTask.CompletedTask;
        }

        if (NavigationHelper.UseLegacyFormat())
        {
            builder
               .Add(S["Configuration"], configuration => configuration
                   .Add(S["Settings"], settings => settings
                       .Add(S["Azure AI Search"], S["Azure AI Search"].PrefixPosition(), azureAISearch => azureAISearch
                       .AddClass("azure-ai-search")
                           .Id("azureaisearch")
                           .Action("Index", "Admin", _routeValues)
                           .Permission(AzureAISearchPermissions.ManageAzureAISearchISettings)
                           .LocalNav()
                       )
                   )
               );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Search"], S["Search"].PrefixPosition(), search => search
                    .Add(S["Azure AI Search"], S["Azure AI Search"].PrefixPosition(), azureAISearch => azureAISearch
                    .AddClass("azure-ai-search")
                        .Id("azureaisearch")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(IndexingPermissions.ManageIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
