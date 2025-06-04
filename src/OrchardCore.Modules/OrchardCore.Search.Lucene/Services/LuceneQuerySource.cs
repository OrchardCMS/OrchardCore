using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Lucene.Core;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Lucene;

public sealed class LuceneQuerySource : IQuerySource
{
    public const string SourceName = LuceneConstants.ProviderName;

    private readonly ILuceneIndexStore _luceneIndexStore;
    private readonly IIndexEntityStore _indexStore;
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly ILuceneQueryService _queryService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ISession _session;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly TemplateOptions _templateOptions;

    public LuceneQuerySource(
        ILuceneIndexStore luceneIndexStore,
        IIndexEntityStore indexStore,
        LuceneAnalyzerManager luceneAnalyzerManager,
        ILuceneQueryService queryService,
        ILiquidTemplateManager liquidTemplateManager,
        ISession session,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<TemplateOptions> templateOptions)
    {
        _luceneIndexStore = luceneIndexStore;
        _indexStore = indexStore;
        _luceneAnalyzerManager = luceneAnalyzerManager;
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
        var result = new LuceneQueryResults()
        {
            Items = [],
        };

        var metadata = query.As<LuceneQueryMetadata>();

        var index = await _indexStore.FindByIndexNameAndProviderAsync(metadata.Index, LuceneConstants.ProviderName);

        if (index is null)
        {
            return result;
        }

        await _luceneIndexStore.SearchAsync(index, async searcher =>
        {
            var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(metadata.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

            var parameterizedQuery = JsonNode.Parse(tokenizedContent, JOptions.Node, JOptions.Document).AsObject();

            var analyzer = _luceneAnalyzerManager.CreateAnalyzer(index.As<LuceneIndexMetadata>().AnalyzerName);

            var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();

            var context = new LuceneQueryContext(searcher, queryMetadata.DefaultVersion, analyzer);
            var docs = await _queryService.SearchAsync(context, parameterizedQuery);
            result.Count = docs.Count;

            if (query.ReturnContentItems)
            {
                // We always return an empty collection if the bottom lines queries have no results.
                result.Items = [];

                // Load corresponding content item versions.
                var indexedContentItemVersionIds = docs.TopDocs.ScoreDocs.Select(x => searcher.Doc(x.Doc).Get("ContentItemVersionId")).ToArray();
                var dbContentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(indexedContentItemVersionIds)).ListAsync();

                // Reorder the result to preserve the one from the Lucene query.
                if (dbContentItems.Any())
                {
                    var dbContentItemVersionIds = dbContentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                    var indexedAndInDB = indexedContentItemVersionIds.Where(dbContentItemVersionIds.ContainsKey);
                    result.Items = indexedAndInDB.Select(x => dbContentItemVersionIds[x]).ToArray();
                }
            }
            else
            {
                var results = new List<JsonObject>();

                foreach (var document in docs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)))
                {
                    results.Add(new JsonObject(document.Select(x =>
                        KeyValuePair.Create(x.Name, (JsonNode)JsonValue.Create(x.GetStringValue())))));
                }

                result.Items = results;
            }
        });

        return result;
    }
}
