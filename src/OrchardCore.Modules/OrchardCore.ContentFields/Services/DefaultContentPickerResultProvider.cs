using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentFields.Services
{
    public class DefaultContentPickerResultProvider : IContentPickerResultProvider<ContentItem>
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public DefaultContentPickerResultProvider(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public string Name => "Default";

        public virtual ContentPickerResult BuildResult(ContentItem contentItem) => new ContentPickerResult
        {
            ContentItemId = contentItem.ContentItemId,
            DisplayText = $"{contentItem.ContentType}{(string.IsNullOrWhiteSpace(contentItem.DisplayText) ? string.Empty : ": " + contentItem.DisplayText)}"
        };

        public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.ContentType.IsIn(searchContext.ContentTypes) && x.Latest);

            if (!string.IsNullOrEmpty(searchContext.Query))
            {
                query.With<ContentItemIndex>(x => x.DisplayText.Contains(searchContext.Query));
            }

            var contentItems = await query.Take(20).ListAsync();

            var results = new List<ContentPickerResult>();

            foreach (var contentItem in contentItems)
            {
                results.Add(BuildResult(contentItem));
            }

            return results;
        }
    }
}
