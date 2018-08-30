using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Content.Controllers;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentFields.Services
{
    public class DefaultContentPickerResultProvider : IContentPickerResultProvider
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public DefaultContentPickerResultProvider(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public async Task<IEnumerable<ContentPickerResult>> GetContentItems(string searchTerm, IEnumerable<string> contentTypes)
        {
            // Default provider only returns results if no search term is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                return new List<ContentPickerResult>();
            }

            var contentItems = await _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.ContentType.IsIn(contentTypes) && x.Published)
                .ListAsync();

            var results = new List<ContentPickerResult>();

            foreach (var contentItem in contentItems)
            {
                var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
                results.Add(new ContentPickerResult
                {
                    ContentItemId = contentItem.ContentItemId,
                    DisplayText = contentItemMetadata.DisplayText,
                    Score = 1 // TODO: figure out scoring
                });
            }

            return results;
        }
    }
}
