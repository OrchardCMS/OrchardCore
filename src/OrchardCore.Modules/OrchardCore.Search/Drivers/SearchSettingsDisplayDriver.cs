using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Search.Models;
using OrchardCore.Search.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Drivers;

public sealed class SearchSettingsDisplayDriver : SiteDisplayDriver<SearchSettings>
{
    [Obsolete("This property should not be used. Instead use  SearchConstants.SearchSettingsGroupId.")]
    public const string GroupId = SearchConstants.SearchSettingsGroupId;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IIndexProfileStore _indexProfileStore;

    public SearchSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IIndexProfileStore indexProfileStore
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _indexProfileStore = indexProfileStore;
    }

    protected override string SettingsGroupId
        => SearchConstants.SearchSettingsGroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, SearchSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SearchPermissions.ManageSearchSettings).ConfigureAwait(false))
        {
            return null;
        }

        return Initialize<SearchSettingsViewModel>("SearchSettings_Edit", async model =>
        {
            model.DefaultIndexProfileName = settings.DefaultIndexProfileName;
            model.Placeholder = settings.Placeholder;
            model.PageTitle = settings.PageTitle;
            model.Indexes = (await _indexProfileStore.GetAllAsync().ConfigureAwait(false))
                .Select(index => new SelectListItem(index.Name, index.Id))
                .ToArray();
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SearchSettings section, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SearchPermissions.ManageSearchSettings).ConfigureAwait(false))
        {
            return null;
        }

        var model = new SearchSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix).ConfigureAwait(false);

        section.DefaultIndexProfileName = model.DefaultIndexProfileName;
        section.Placeholder = model.Placeholder;
        section.PageTitle = model.PageTitle;

        return await EditAsync(site, section, context).ConfigureAwait(false);
    }
}
