using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Drivers;

public sealed class LuceneSettingsDisplayDriver : SiteDisplayDriver<LuceneSettings>
{
    private static readonly char[] _separator = [',', ' '];

    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public LuceneSettingsDisplayDriver(
        LuceneIndexSettingsService luceneIndexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => SearchConstants.SearchSettingsGroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, LuceneSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLuceneIndexes))
        {
            return null;
        }

        return Initialize<LuceneSettingsViewModel>("LuceneSettings_Edit", async model =>
        {
            model.SearchIndex = settings.SearchIndex;
            model.SearchFields = string.Join(", ", settings.DefaultSearchFields ?? []);
            model.SearchIndexes = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName);
            model.AllowLuceneQueriesInSearch = settings.AllowLuceneQueriesInSearch;
        }).Location("Content:2#Lucene;15")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, LuceneSettings section, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLuceneIndexes))
        {
            return null;
        }

        var model = new LuceneSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        section.SearchIndex = model.SearchIndex;
        section.DefaultSearchFields = model.SearchFields?.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        section.AllowLuceneQueriesInSearch = model.AllowLuceneQueriesInSearch;

        return await EditAsync(site, section, context);
    }
}
