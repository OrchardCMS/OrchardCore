using System;
using System.Linq;
using System.Threading.Tasks;
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

namespace OrchardCore.Taxonomies.Settings
{
    public class TaxonomyContentsAdminListSettingsDisplayDriver : SectionDisplayDriver<ISite, TaxonomyContentsAdminListSettings>
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

        public override async Task<IDisplayResult> EditAsync(TaxonomyContentsAdminListSettings settings, BuildEditorContext context)
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
                IsChecked = settings.TaxonomyContentItemIds.Any(id => String.Equals(x.ContentItemId, id, StringComparison.OrdinalIgnoreCase))
            }).ToArray();

            return Initialize<TaxonomyContentsAdminListSettingsViewModel>("TaxonomyContentsAdminListSettings_Edit", model =>
            {
                model.TaxonomyEntries = entries;
            }).Location("Content:2").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(TaxonomyContentsAdminListSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new TaxonomyContentsAdminListSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    settings.TaxonomyContentItemIds = model.TaxonomyEntries.Where(e => e.IsChecked).Select(e => e.ContentItemId).ToArray();
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
