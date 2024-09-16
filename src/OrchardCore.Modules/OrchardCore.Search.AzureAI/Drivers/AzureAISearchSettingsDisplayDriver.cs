using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchSettingsDisplayDriver : SiteDisplayDriver<AzureAISearchSettings>
{
    private static readonly char[] _separator = [',', ' '];

    private readonly AzureAISearchIndexSettingsService _indexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => SearchConstants.SearchSettingsGroupId;

    public AzureAISearchSettingsDisplayDriver(
        AzureAISearchIndexSettingsService indexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<AzureAISearchSettingsDisplayDriver> stringLocalizer)
    {
        _indexSettingsService = indexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, AzureAISearchSettings settings, BuildEditorContext context)
        => Initialize<AzureAISearchSettingsViewModel>("AzureAISearchSettings_Edit", async model =>
        {
            model.SearchIndex = settings.SearchIndex;
            model.SearchFields = string.Join(", ", settings.DefaultSearchFields ?? []);
            model.SearchIndexes = (await _indexSettingsService.GetSettingsAsync())
            .Select(x => new SelectListItem(x.IndexName, x.IndexName))
            .ToList();
        }).Location("Content:2#Azure AI Search;5")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        .OnGroup(SettingsGroupId);

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureAISearchSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        var model = new AzureAISearchSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);


        if (string.IsNullOrEmpty(model.SearchIndex))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SearchIndex), S["Search Index is required."]);
        }
        else
        {
            var indexes = await _indexSettingsService.GetSettingsAsync();

            if (!indexes.Any(index => index.IndexName == model.SearchIndex))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.SearchIndex), S["Invalid Search Index value."]);
            }
        }

        var fields = model.SearchFields?.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

        if (settings.SearchIndex != model.SearchIndex || !AreTheSame(settings.DefaultSearchFields, fields))
        {
            settings.SearchIndex = model.SearchIndex;
            settings.DefaultSearchFields = fields;

            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }

    private static bool AreTheSame(string[] a, string[] b)
    {
        if (a == null && b == null)
        {
            return false;
        }

        if ((a is null && b is not null) || (a is not null && b is null))
        {
            return true;
        }

        var combine = a.Intersect(b).ToList();

        return combine.Count == a.Length && combine.Count == b.Length;
    }
}
