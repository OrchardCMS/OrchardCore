using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Navigation;
using OrchardCore.Search.AzureAI.Drivers;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    private readonly AzureAISearchDefaultOptions _azureAISearchSettings;

    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(
        IOptions<AzureAISearchDefaultOptions> azureAISearchSettings,
        IStringLocalizer<AdminSettingsMenu> stringLocalizer)
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

        builder
            .Add(S["General"], general => general
                .Add(S["Azure AI Search"], S["Azure AI Search"].PrefixPosition(), azureAISearch => azureAISearch
                    .AddClass("azure-ai-search")
                    .Id("azureaisearch")
                    .Action(GetRouteValues(AzureAISearchDefaultSettingsDisplayDriver.GroupId))
                    .Permission(AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
