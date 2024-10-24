using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Liquid;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticQuerySource : IQuerySource
{
    public const string SourceName = "Elasticsearch";

    private readonly IElasticQueryService _queryService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ISession _session;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly TemplateOptions _templateOptions;

    public ElasticQuerySource(
        IElasticQueryService queryService,
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

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var metadata = query.As<ElasticsearchQueryMetadata>();
        var elasticQueryResults = new ElasticQueryResults();

        var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(metadata?.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));
        var docs = await _queryService.SearchAsync(metadata?.Index, tokenizedContent);
        elasticQueryResults.Count = docs.Count;

        // We always return an empty collection if the bottom lines queries have no results.
        elasticQueryResults.Items = [];

        if (elasticQueryResults.Count == 0 || docs.TopDocs == null || docs.TopDocs.Count == 0)
        {
            return elasticQueryResults;
        }

        if (query.ReturnContentItems)
        {
            // Load corresponding content item versions.
            var topDocs = docs.TopDocs.Where(x => x != null).ToList();

            if (topDocs.Count > 0)
            {
                var indexedContentItemVersionIds = topDocs.Select(x => x.GetValueOrDefault("ContentItemVersionId").ToString()).ToArray();
                var dbContentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(indexedContentItemVersionIds)).ListAsync();

                // Reorder the result to preserve the one from the Elasticsearch query.
                if (dbContentItems.Any())
                {
                    var dbContentItemVersionIds = dbContentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                    var indexedAndInDB = indexedContentItemVersionIds.Where(dbContentItemVersionIds.ContainsKey);
                    elasticQueryResults.Items = indexedAndInDB.Select(x => dbContentItemVersionIds[x]).ToArray();
                }
            }

            // TODO : get ContentItemVersionId from docs.Fields
        }
        else
        {
            var results = new List<JsonObject>();

            foreach (var document in docs.TopDocs)
            {
                results.Add(new JsonObject(document.Select(x =>
                    KeyValuePair.Create(x.Key, (JsonNode)JsonValue.Create(x.Value.ToString())))));
            }

            elasticQueryResults.Items = results;
        }

        return elasticQueryResults;
    }
}
