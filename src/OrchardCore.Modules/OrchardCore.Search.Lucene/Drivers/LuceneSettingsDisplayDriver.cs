using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Drivers
{
    public class LuceneSettingsDisplayDriver : SectionDisplayDriver<ISite, LuceneSettings>
    {
        public const string GroupId = "lucene";
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public LuceneSettingsDisplayDriver(
            LuceneIndexSettingsService luceneIndexSettingsService,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService
            )
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

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
                    model.SearchFields = String.Join(", ", settings.DefaultSearchFields ?? Array.Empty<string>());
                    model.SearchIndexes = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName);
                    model.AllowLuceneQueriesInSearch = settings.AllowLuceneQueriesInSearch;
                }).Location("Content:2").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLuceneIndexes))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new LuceneSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.SearchIndex = model.SearchIndex;
                section.DefaultSearchFields = model.SearchFields?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                section.AllowLuceneQueriesInSearch = model.AllowLuceneQueriesInSearch;
            }

            return await EditAsync(section, context);
        }
    }
}
