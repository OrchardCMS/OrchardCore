using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Liquid;
using OrchardCore.Lucene.Services;
using OrchardCore.Queries;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Lucene
{
    public class LuceneQuerySource : IQuerySource
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly LuceneAnalyzerSettingsService _luceneAnalyzerSettingsService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly ILuceneQueryService _queryService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;

        public LuceneQuerySource(
            LuceneIndexManager luceneIndexManager,
            LuceneAnalyzerSettingsService luceneAnalyzerSettingsService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            ILuceneQueryService queryService,
            ILiquidTemplateManager liquidTemplateManager,
            ISession session)
        {
            _luceneIndexManager = luceneIndexManager;
            _luceneAnalyzerSettingsService = luceneAnalyzerSettingsService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _queryService = queryService;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
        }

        public string Name => "Lucene";

        public Query Create()
        {
            return new LuceneQuery();
        }

        public async Task<object> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var luceneQuery = query as LuceneQuery;
            object result = null;

            await _luceneIndexManager.SearchAsync (luceneQuery.Index, async searcher =>
            {
                var templateContext = new TemplateContext();

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        templateContext.SetValue(parameter.Key, parameter.Value);
                    }
                }

                var tokenizedContent = await _liquidTemplateManager.RenderAsync(luceneQuery.Template, templateContext);
                var parameterizedQuery = JObject.Parse(tokenizedContent);
                var luceneSettings = await _luceneAnalyzerSettingsService.GetLuceneAnalyzerSettingsAsync();
                var analyzer = _luceneAnalyzerManager.CreateAnalyzer(luceneSettings.Analyzer);
                var context = new LuceneQueryContext(searcher, luceneSettings.Version, analyzer);
                var docs = await _queryService.SearchAsync(context, parameterizedQuery);

                if (luceneQuery.ReturnContentItems)
                {
                    // Load corresponding content item versions
                    var contentItemVersionIds = docs.ScoreDocs.Select(x => searcher.Doc(x.Doc).Get("Content.ContentItem.ContentItemVersionId")).ToArray();
                    var contentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(contentItemVersionIds)).ListAsync();

                    // Reorder the result to preserve the one from the lucene query
                    var indexed = contentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                    result = contentItemVersionIds.Select(x => indexed[x]).ToArray();
                }
                else
                {
                    var results = new List<JObject>();
                    foreach (var document in docs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)))
                    {
                        results.Add(new JObject(document.Select(x => new JProperty(x.Name, x.GetStringValue()))));
                    }

                    result = results;
                }
            });

            return result;
        }
    }
}
