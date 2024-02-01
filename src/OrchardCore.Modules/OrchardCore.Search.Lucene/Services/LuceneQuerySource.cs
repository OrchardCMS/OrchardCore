using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Liquid;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Lucene
{
    public class LuceneQuerySource : IQuerySource
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly ILuceneQueryService _queryService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;
        private readonly JavaScriptEncoder _javaScriptEncoder;
        private readonly TemplateOptions _templateOptions;

        public LuceneQuerySource(
            LuceneIndexManager luceneIndexManager,
            LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            ILuceneQueryService queryService,
            ILiquidTemplateManager liquidTemplateManager,
            ISession session,
            JavaScriptEncoder javaScriptEncoder,
            IOptions<TemplateOptions> templateOptions)
        {
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _queryService = queryService;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
            _javaScriptEncoder = javaScriptEncoder;
            _templateOptions = templateOptions.Value;
        }

        public string Name => "Lucene";

        public Query Create()
        {
            return new LuceneQuery();
        }

        public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var luceneQuery = query as LuceneQuery;
            var luceneQueryResults = new LuceneQueryResults();

            await _luceneIndexManager.SearchAsync(luceneQuery.Index, async searcher =>
            {
                var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(luceneQuery.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

                var parameterizedQuery = JObject.Parse(tokenizedContent);

                var analyzer = _luceneAnalyzerManager.CreateAnalyzer(await _luceneIndexSettingsService.GetIndexAnalyzerAsync(luceneQuery.Index));
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);
                var docs = await _queryService.SearchAsync(context, parameterizedQuery);
                luceneQueryResults.Count = docs.Count;

                if (luceneQuery.ReturnContentItems)
                {
                    // We always return an empty collection if the bottom lines queries have no results.
                    luceneQueryResults.Items = new List<ContentItem>();

                    // Load corresponding content item versions
                    var indexedContentItemVersionIds = docs.TopDocs.ScoreDocs.Select(x => searcher.Doc(x.Doc).Get("ContentItemVersionId")).ToArray();
                    var dbContentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(indexedContentItemVersionIds)).ListAsync();

                    // Reorder the result to preserve the one from the lucene query
                    if (dbContentItems.Any())
                    {
                        var dbContentItemVersionIds = dbContentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                        var indexedAndInDB = indexedContentItemVersionIds.Where(dbContentItemVersionIds.ContainsKey);
                        luceneQueryResults.Items = indexedAndInDB.Select(x => dbContentItemVersionIds[x]).ToArray();
                    }
                }
                else
                {
                    var results = new List<JObject>();
                    foreach (var document in docs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)))
                    {
                        results.Add(new JObject(document.Select(x => new JProperty(x.Name, x.GetStringValue()))));
                    }

                    luceneQueryResults.Items = results;
                }
            });

            return luceneQueryResults;
        }
    }
}
