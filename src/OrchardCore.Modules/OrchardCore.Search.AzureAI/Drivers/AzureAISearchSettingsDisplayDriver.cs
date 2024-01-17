using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
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
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    protected readonly IStringLocalizer S;

    public AzureAISearchSettingsDisplayDriver(
        AzureAISearchIndexSettingsService indexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IStringLocalizer<AzureAISearchSettingsDisplayDriver> stringLocalizer
        )
    {
        _indexSettingsService = indexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
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
        }).Location("Content:2#Azure AI Search;5")
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

            if (section.SearchIndex != model.SearchIndex || !AreTheSame(section.DefaultSearchFields, fields))
            {
                section.SearchIndex = model.SearchIndex;
                section.DefaultSearchFields = fields;

                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
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
