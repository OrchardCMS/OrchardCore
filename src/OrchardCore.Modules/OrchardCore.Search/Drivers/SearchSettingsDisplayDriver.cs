using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Search.Models;
using OrchardCore.Search.Services;
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

    internal readonly IStringLocalizer S;

    /// <summary>Creates the tenant search settings editor with index lookup and localized validation.</summary>
    public SearchSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IIndexProfileStore indexProfileStore,
        IStringLocalizer<SearchSettingsDisplayDriver> localizer
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _indexProfileStore = indexProfileStore;
        S = localizer;
    }

    protected override string SettingsGroupId
        => SearchConstants.SearchSettingsGroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, SearchSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SearchPermissions.ManageSearchSettings))
        {
            return null;
        }

        return Initialize<SearchSettingsViewModel>("SearchSettings_Edit", async model =>
        {
            model.DefaultIndexProfileName = settings.DefaultIndexProfileName;
            model.Placeholder = settings.Placeholder;
            model.PageTitle = settings.PageTitle;
            model.Indexes = (await _indexProfileStore.GetAllAsync())
                .Select(index => new SelectListItem(index.Name, index.Name))
                .ToArray();
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SearchSettings section, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SearchPermissions.ManageSearchSettings))
        {
            return null;
        }

        var model = new SearchSettingsViewModel
        {
            DefaultIndexProfileName = section.DefaultIndexProfileName,
            Placeholder = section.Placeholder,
            PageTitle = section.PageTitle,
        };

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var proposed = new SearchSettings
        {
            DefaultIndexProfileName = model.DefaultIndexProfileName,
            Placeholder = model.Placeholder,
            PageTitle = model.PageTitle,
        };
        if (await SearchSettingsEditor.ValidateAsync(_indexProfileStore, section, proposed))
        {
            SearchSettingsEditor.Apply(section, proposed);
        }
        else
        {
            context.Updater.ModelState.AddModelError(Prefix + "." + nameof(model.DefaultIndexProfileName), S["Choose an existing index profile."]);
        }

        return await EditAsync(site, section, context);
    }
}
