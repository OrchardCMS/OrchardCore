using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.Azure.CognitiveSearch.Models;
using OrchardCore.Search.Azure.CognitiveSearch.Services;
using OrchardCore.Search.Azure.CognitiveSearch.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Azure.CognitiveSearch.Drivers;

public class AzureCognitiveSearchSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureCognitiveSearchSettings>
{
    private static readonly char[] _separator = [',', ' '];
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };
    private readonly AzureCognitiveSearchIndexSettingsService _cognitiveSearchIndexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    protected readonly IStringLocalizer S;

    public AzureCognitiveSearchSettingsDisplayDriver(
        AzureCognitiveSearchIndexSettingsService cognitiveSearchIndexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<AzureCognitiveSearchSettingsDisplayDriver> stringLocalizer
        )
    {
        _cognitiveSearchIndexSettingsService = cognitiveSearchIndexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AzureCognitiveSearchSettings settings)
        => Initialize<AzureCognitiveSearchSettingsViewModel>("AzureCognitiveSearchSettings_Edit", async model =>
        {
            model.SearchIndex = settings.SearchIndex;
            model.SearchFields = string.Join(", ", settings.DefaultSearchFields ?? []);
            model.SearchIndexes = (await _cognitiveSearchIndexSettingsService.GetSettingsAsync())
            .Select(x => new SelectListItem(x.IndexName, x.IndexName))
            .ToList();
        }).Location("Content:2#Azure Cognitive Search;5")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        .Prefix(Prefix)
        .OnGroup(SearchConstants.SearchSettingsGroupId);

    public override async Task<IDisplayResult> UpdateAsync(AzureCognitiveSearchSettings section, BuildEditorContext context)
    {
        if (!SearchConstants.SearchSettingsGroupId.EqualsOrdinalIgnoreCase(context.GroupId))
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        {
            return null;
        }

        var model = new AzureCognitiveSearchSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (string.IsNullOrEmpty(model.SearchIndex))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SearchIndex), S["Search Index is required."]);
        }
        else
        {
            var indexes = await _cognitiveSearchIndexSettingsService.GetSettingsAsync();

            if (!indexes.Any(index => index.IndexName == model.SearchIndex))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.SearchIndex), S["Invalid Search Index value."]);
            }
        }

        section.SearchIndex = model.SearchIndex;
        section.DefaultSearchFields = model.SearchFields?.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

        return await EditAsync(section, context);
    }

    protected override void BuildPrefix(ISite model, string htmlFieldPrefix)
    {
        Prefix = typeof(AzureCognitiveSearchSettings).Name;

        if (!string.IsNullOrEmpty(htmlFieldPrefix))
        {
            Prefix = htmlFieldPrefix + "." + Prefix;
        }
    }
}
