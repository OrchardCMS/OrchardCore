using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Drivers
{
    public class LuceneSettingsDisplayDriver(
        LuceneIndexSettingsService luceneIndexSettingsService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService
            ) : SectionDisplayDriver<ISite, LuceneSettings>
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService = luceneIndexSettingsService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IAuthorizationService _authorizationService = authorizationService;
        private static readonly char[] _separator = [',', ' '];

        public override async Task<IDisplayResult> EditAsync(LuceneSettings settings, BuildEditorContext context)
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
                .OnGroup(SearchConstants.SearchSettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneSettings section, BuildEditorContext context)
        {
            if (!SearchConstants.SearchSettingsGroupId.EqualsOrdinalIgnoreCase(context.GroupId))
            {
                return null;
            }

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

            return await EditAsync(section, context);
        }
    }
}
