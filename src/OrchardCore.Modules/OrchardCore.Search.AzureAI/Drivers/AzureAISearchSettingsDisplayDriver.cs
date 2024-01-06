using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Drivers;

public class AzureAISearchSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureAISearchSettings>
{
    private static readonly char[] _separator = [',', ' '];

    private readonly AzureAISearchIndexSettingsService _indexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly AzureAISearchDefaultOptions _azureAIOptions;

    protected readonly IStringLocalizer S;

    public AzureAISearchSettingsDisplayDriver(
        AzureAISearchIndexSettingsService indexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<AzureAISearchDefaultOptions> azureAIOptions,
        IStringLocalizer<AzureAISearchSettingsDisplayDriver> stringLocalizer
        )
    {
        _indexSettingsService = indexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _azureAIOptions = azureAIOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AzureAISearchSettings settings)
        => Initialize<AzureAISearchSettingsViewModel>("AzureAISearchSettings_Edit", async model =>
        {
            model.SearchIndex = settings.SearchIndex;
            model.SearchFields = string.Join(", ", settings.DefaultSearchFields ?? []);
            model.SearchIndexes = (await _indexSettingsService.GetSettingsAsync())
            .Select(x => new SelectListItem(x.IndexName, x.IndexName))
            .ToList();
        }).Location("Content:5#Azure AI Search;5")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        .Prefix(Prefix)
        .OnGroup(SearchConstants.SearchSettingsGroupId);

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchSettings section, BuildEditorContext context)
    {
        if (!SearchConstants.SearchSettingsGroupId.EqualsOrdinalIgnoreCase(context.GroupId))
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        var model = new AzureAISearchSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            var hasSearchIndex = !string.IsNullOrEmpty(model.SearchIndex);

            if (_azureAIOptions.IsConfigurationExists() && !hasSearchIndex)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.SearchIndex), S["Search Index is required."]);
            }

            if (context.Updater.ModelState.IsValid)
            {
                var indexes = await _indexSettingsService.GetSettingsAsync();

                if (hasSearchIndex && !indexes.Any(index => index.IndexName == model.SearchIndex))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.SearchIndex), S["Invalid Search Index value."]);
                }
            }

            section.SearchIndex = model.SearchIndex;
            section.DefaultSearchFields = model.SearchFields?.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        }

        return Edit(section);
    }

    protected override void BuildPrefix(ISite model, string htmlFieldPrefix)
    {
        Prefix = typeof(AzureAISearchSettings).Name;

        if (!string.IsNullOrEmpty(htmlFieldPrefix))
        {
            Prefix = htmlFieldPrefix + "." + Prefix;
        }
    }
}
