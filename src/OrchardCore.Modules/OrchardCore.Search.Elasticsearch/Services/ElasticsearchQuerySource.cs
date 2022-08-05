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
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchQuerySource : IQuerySource
    {
        private readonly IElasticsearchQueryService _queryService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;
        private readonly JavaScriptEncoder _javaScriptEncoder;
        private readonly TemplateOptions _templateOptions;

        public ElasticsearchQuerySource(
            IElasticsearchQueryService queryService,
            ILiquidTemplateManager liquidTemplateManager,
            ISession session,
            JavaScriptEncoder javaScriptEncoder,
            IOptions<TemplateOptions> templateOptions)
        {
            _queryService = queryService;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
            _javaScriptEncoder = javaScriptEncoder;
            _templateOptions = templateOptions.Value;
        }

        public string Name => "Elasticsearch";

        public Query Create()
        {
            return new ElasticsearchQuery();
        }

        public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var elasticQuery = query as ElasticsearchQuery;
            var elasticQueryResults = new ElasticsearchQueryResults();

            var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(elasticQuery.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));
            var elasticSearchQueryContext = new ElasticsearchQueryContext(elasticQuery.Index);
            var docs = await _queryService.SearchAsync(elasticSearchQueryContext, tokenizedContent);
            elasticQueryResults.Count = docs.Count;

            if (elasticQuery.ReturnContentItems)
            {
                // We always return an empty collection if the bottom lines queries have no results.
                elasticQueryResults.Items = new List<ContentItem>();

                // Load corresponding content item versions
                var indexedContentItemVersionIds = docs.TopDocs.Select(x => x.GetValueOrDefault("ContentItemVersionId").ToString()).ToArray();
                var dbContentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(indexedContentItemVersionIds)).ListAsync();

                // Reorder the result to preserve the one from the Elasticsearch query
                if (dbContentItems.Any())
                {
                    var dbContentItemVersionIds = dbContentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                    var indexedAndInDB = indexedContentItemVersionIds.Where(dbContentItemVersionIds.ContainsKey);
                    elasticQueryResults.Items = indexedAndInDB.Select(x => dbContentItemVersionIds[x]).ToArray();
                }
            }
            else
            {
                var results = new List<JObject>();

                foreach (var document in docs.TopDocs)
                {
                    results.Add(new JObject(document.Select(x => new JProperty(x.Key, x.Value.ToString()))));
                }

                elasticQueryResults.Items = results;
            }

            return elasticQueryResults;
        }
    }
}
