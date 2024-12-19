using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Abstractions;
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
    private readonly IServiceProvider _serviceProvider;

    public SearchSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IServiceProvider serviceProvider
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;
    }

    protected override string SettingsGroupId
        => SearchConstants.SearchSettingsGroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, SearchSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSearchSettings))
        {
            return null;
        }

        return Initialize<SearchSettingsViewModel>("SearchSettings_Edit", model =>
        {
            var searchServices = _serviceProvider.GetServices<ISearchService>();

            model.SearchServices = searchServices.Select(service => new SelectListItem(service.Name, service.Name)).ToList();
            model.Placeholder = settings.Placeholder;
            model.PageTitle = settings.PageTitle;
            model.ProviderName = settings.ProviderName;
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SearchSettings section, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSearchSettings))
        {
            return null;
        }

        var model = new SearchSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        section.ProviderName = model.ProviderName;
        section.Placeholder = model.Placeholder;
        section.PageTitle = model.PageTitle;

        return await EditAsync(site, section, context);
    }
}
