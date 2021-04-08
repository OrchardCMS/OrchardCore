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
using OrchardCore.Search.Elastic.Model;
using OrchardCore.Search.Elastic.Services;
using OrchardCore.Queries;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Elastic
{
    public class ElasticQuerySource : IQuerySource
    {
        private readonly ElasticIndexManager _elasticIndexProvider;
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
        private readonly ElasticAnalyzerManager _elasticAnalyzerManager;
        private readonly IElasticQueryService _queryService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;
        private readonly JavaScriptEncoder _javaScriptEncoder;
        private readonly TemplateOptions _templateOptions;

        public ElasticQuerySource(
            ElasticIndexManager elasticIndexProvider,
            ElasticIndexSettingsService elasticIndexSettingsService,
            ElasticAnalyzerManager elasticAnalyzerManager,
            IElasticQueryService queryService,
            ILiquidTemplateManager liquidTemplateManager,
            ISession session,
            JavaScriptEncoder javaScriptEncoder,
            IOptions<TemplateOptions> templateOptions)
        {
            _elasticIndexProvider = elasticIndexProvider;
            _elasticIndexSettingsService = elasticIndexSettingsService;
            _elasticAnalyzerManager = elasticAnalyzerManager;
            _queryService = queryService;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
            _javaScriptEncoder = javaScriptEncoder;
            _templateOptions = templateOptions.Value;
        }

        public string Name => "Elastic";

        public Query Create()
        {
            return new ElasticQuery();
        }

        public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var elasticQuery = query as ElasticQuery;

            //Should be renamed at OrchardCore.Queries to SearchQueryResults

            var luceneQueryResults = new LuceneQueryResults();

            await _elasticIndexProvider.SearchAsync(elasticQuery.Index, async searcher =>
            {
                var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(elasticQuery.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

                var parameterizedQuery = JObject.Parse(tokenizedContent);

                var analyzer = _elasticAnalyzerManager.CreateAnalyzer(await _elasticIndexSettingsService.GetIndexAnalyzerAsync(elasticQuery.Index));
                var context = new ElasticQueryContext(searcher, ElasticSettings.DefaultVersion, analyzer);
                var docs = await _queryService.SearchAsync(context, parameterizedQuery);
                luceneQueryResults.Count = docs.Count;

                if (elasticQuery.ReturnContentItems)
                {
                    // We always return an empty collection if the bottom lines queries have no results.
                    luceneQueryResults.Items = new List<ContentItem>();

                    // Load corresponding content item versions
                    var indexedContentItemVersionIds = docs.TopDocs.ScoreDocs.Select(x => searcher.Doc(x.Doc).Get("Content.ContentItem.ContentItemVersionId")).ToArray();
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
