using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Queries;
using OrchardCore.OpenSearch.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.OpenSearch.Core.Services;

public sealed class OpenSearchQuerySource : IQuerySource
{
    public const string SourceName = OpenSearchConstants.ProviderName;

    private readonly OpenSearchQueryService _queryService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly ISession _session;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly TemplateOptions _templateOptions;

    public OpenSearchQuerySource(
        OpenSearchQueryService queryService,
        ILiquidTemplateManager liquidTemplateManager,
        IIndexProfileStore indexProfileStore,
        ISession session,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<TemplateOptions> templateOptions)
    {
        _queryService = queryService;
        _liquidTemplateManager = liquidTemplateManager;
        _indexProfileStore = indexProfileStore;
        _session = session;
        _javaScriptEncoder = javaScriptEncoder;
        _templateOptions = templateOptions.Value;
    }

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var metadata = query.As<OpenSearchQueryMetadata>();
        var openSearchQueryResults = new OpenSearchQueryResults()
        {
            Items = [],
        };

        var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(metadata?.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

        if (string.IsNullOrEmpty(metadata.Index))
        {
            return openSearchQueryResults;
        }

        var index = await _indexProfileStore.FindByNameAsync(metadata.Index);

        if (index is null || index.ProviderName != OpenSearchConstants.ProviderName)
        {
            return openSearchQueryResults;
        }

        var docs = await _queryService.SearchAsync(index, tokenizedContent);
        openSearchQueryResults.Count = docs.Count;

        if (openSearchQueryResults.Count == 0 || docs.TopDocs == null || docs.TopDocs.Count == 0)
        {
            return openSearchQueryResults;
        }

        if (query.ReturnContentItems)
        {
            var topDocs = docs.TopDocs.Where(x => x != null).ToList();

            if (topDocs.Count > 0)
            {
                var indexedContentItemVersionIds = new List<string>();

                foreach (var topDoc in topDocs)
                {
                    if (!topDoc.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemVersionId), out var versionId))
                    {
                        continue;
                    }

                    indexedContentItemVersionIds.Add(versionId.GetValue<string>());
                }

                var dbContentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(indexedContentItemVersionIds)).ListAsync();

                if (dbContentItems.Any())
                {
                    var dbContentItemVersionIds = dbContentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                    var indexedAndInDB = indexedContentItemVersionIds.Where(dbContentItemVersionIds.ContainsKey);
                    openSearchQueryResults.Items = indexedAndInDB.Select(x => dbContentItemVersionIds[x]).ToArray();
                }
            }
        }
        else
        {
            var results = new List<JsonObject>();

            foreach (var document in docs.TopDocs)
            {
                results.Add(new JsonObject(document.Value.Select(x =>
                    KeyValuePair.Create(x.Key, (JsonNode)JsonValue.Create(x.Value)))));
            }

            openSearchQueryResults.Items = results;
        }

        return openSearchQueryResults;
    }
}
