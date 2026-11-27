using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.AzureAI.Drivers;
using OrchardCore.AzureAI.Models;
using OrchardCore.Indexing.Core;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.AzureAI;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", AzureAISearchDefaultSettingsDisplayDriver.GroupId},
    };

    private readonly IOptionsMonitor<AzureAISearchDefaultOptions> _azureAISearchSettings;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        IOptionsMonitor<AzureAISearchDefaultOptions> azureAISearchSettings,
        IStringLocalizer<AdminMenu> stringLocalizer,
        IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        _azureAISearchSettings = azureAISearchSettings;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (_azureAISearchSettings.CurrentValue.DisableUIConfiguration)
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(_localizerFactory.Localize(CommonLocalizationSources.Search), _localizerFactory.Localize(CommonLocalizationSources.Search).PrefixPosition(), search => search
                    .Add(S["Azure AI Search"], S["Azure AI Search"].PrefixPosition(), azureAISearch => azureAISearch
                    .AddClass("azure-ai-search")
                        .Id("azureaisearch")
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(IndexingPermissions.ManageIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
