using System.Collections.Generic;
using System.Threading.Tasks;
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

        public string Name => "Default";

        public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            var contentItems = await _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.ContentType.IsIn(searchContext.ContentTypes) && x.Published)
                .ListAsync();

            var results = new List<ContentPickerResult>();

            foreach (var contentItem in contentItems)
            {
                var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
                results.Add(new ContentPickerResult
                {
                    ContentItemId = contentItem.ContentItemId,
                    DisplayText = contentItemMetadata.DisplayText
                });
            }

            return results;
        }
    }
}
