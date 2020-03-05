using System.Collections.Generic;
using System.Linq;
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
            var query = _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.ContentType.IsIn(searchContext.ContentTypes) && x.Latest);

            if (!string.IsNullOrEmpty(searchContext.Query))
            {
                query.With<ContentItemIndex>(x => x.DisplayText.Contains(searchContext.Query) || x.ContentType.Contains(searchContext.Query));
            }

            var contentItems = await query.Take(50).ListAsync();

            var results = new List<ContentPickerResult>();

            foreach (var contentItem in contentItems)
            {
                results.Add(new ContentPickerResult
                {
                    ContentItemId = contentItem.ContentItemId,
                    DisplayText = contentItem.ToString(),
                    HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem)
                });
            }

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
