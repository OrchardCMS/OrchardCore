using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Search;
using OrchardCore.ContentManagement;
using OrchardCore.Search.Lucene.Settings;

namespace OrchardCore.Search.Lucene.Services
{
    public class LuceneContentPickerResultProvider : IContentPickerResultProvider
    {
        private readonly LuceneIndexManager _luceneIndexManager;

        public LuceneContentPickerResultProvider(LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
        }

        public string Name => "Lucene";

        public async Task<IEnumerable<ContentPickerResult>> Search(ContentPickerSearchContext searchContext)
        {
            var indexName = "Search";

            var fieldSettings = searchContext.PartFieldDefinition?.GetSettings<ContentPickerFieldLuceneEditorSettings>();

            if (!String.IsNullOrWhiteSpace(fieldSettings?.Index))
            {
                indexName = fieldSettings.Index;
            }

            if (!_luceneIndexManager.Exists(indexName))
            {
                return new List<ContentPickerResult>();
            }

            var results = new List<ContentPickerResult>();

            await _luceneIndexManager.SearchAsync(indexName, searcher =>
            {
                Query query = null;

                if (String.IsNullOrWhiteSpace(searchContext.Query))
                {
                    query = new MatchAllDocsQuery();
                }
                else
                {
                    query = new WildcardQuery(new Term("Content.ContentItem.DisplayText.Normalized", searchContext.Query.ToLowerInvariant() + "*"));
                }

                var filter = new FieldCacheTermsFilter("Content.ContentItem.ContentType", searchContext.ContentTypes.ToArray());

                var docs = searcher.Search(query, filter, 50, Sort.RELEVANCE);

                foreach (var hit in docs.ScoreDocs)
                {
                    var doc = searcher.Doc(hit.Doc);

                    results.Add(new ContentPickerResult
                    {
                        ContentItemId = doc.GetField("ContentItemId").GetStringValue(),
                        DisplayText = doc.GetField("Content.ContentItem.DisplayText.keyword").GetStringValue(),
                        HasPublished = doc.GetField("Content.ContentItem.Published").GetStringValue().ToLower() == "true"
                    });
                }

                return Task.CompletedTask;
            });

            return results.OrderBy(x => x.DisplayText);
        }
    }
}
