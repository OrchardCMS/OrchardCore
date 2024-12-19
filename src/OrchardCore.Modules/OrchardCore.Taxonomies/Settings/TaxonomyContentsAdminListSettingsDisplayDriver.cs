using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Settings;

public sealed class TaxonomyContentsAdminListSettingsDisplayDriver : SiteDisplayDriver<TaxonomyContentsAdminListSettings>
{
    public const string GroupId = "taxonomyContentsAdminList";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly YesSql.ISession _session;

    public TaxonomyContentsAdminListSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        YesSql.ISession session)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _session = session;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, TaxonomyContentsAdminListSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTaxonomies))
        {
            return null;
        }

        var taxonomies = await _session.Query<ContentItem, ContentItemIndex>(q => q.ContentType == "Taxonomy" && q.Published).ListAsync();

        var entries = taxonomies.Select(x => new TaxonomyEntry
        {
            DisplayText = x.DisplayText,
            ContentItemId = x.ContentItemId,
            IsChecked = settings.TaxonomyContentItemIds.Any(id => string.Equals(x.ContentItemId, id, StringComparison.OrdinalIgnoreCase))
        }).ToArray();

        return Initialize<TaxonomyContentsAdminListSettingsViewModel>("TaxonomyContentsAdminListSettings_Edit", model =>
        {
            model.TaxonomyEntries = entries;
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, TaxonomyContentsAdminListSettings settings, UpdateEditorContext context)
    {
        var model = new TaxonomyContentsAdminListSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.TaxonomyContentItemIds = model.TaxonomyEntries.Where(e => e.IsChecked).Select(e => e.ContentItemId).ToArray();

        return await EditAsync(site, settings, context);
    }
}
