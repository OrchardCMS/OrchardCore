using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Liquid;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Lucene;

public sealed class LuceneQuerySource : IQuerySource
{
    public const string SourceName = "Lucene";

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

    public string Name
        => SourceName;

    public async Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        var luceneQueryResults = new LuceneQueryResults();
        var metadata = query.As<LuceneQueryMetadata>();

        await _luceneIndexManager.SearchAsync(metadata.Index, async searcher =>
        {
            var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(metadata.Template, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

            var parameterizedQuery = JsonNode.Parse(tokenizedContent, JOptions.Node, JOptions.Document).AsObject();

            var analyzer = _luceneAnalyzerManager.CreateAnalyzer(await _luceneIndexSettingsService.GetIndexAnalyzerAsync(metadata.Index));
            var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);
            var docs = await _queryService.SearchAsync(context, parameterizedQuery);
            luceneQueryResults.Count = docs.Count;

            if (query.ReturnContentItems)
            {
                // We always return an empty collection if the bottom lines queries have no results.
                luceneQueryResults.Items = [];

                // Load corresponding content item versions.
                var indexedContentItemVersionIds = docs.TopDocs.ScoreDocs.Select(x => searcher.Doc(x.Doc).Get("ContentItemVersionId")).ToArray();
                var dbContentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(indexedContentItemVersionIds)).ListAsync();

                // Reorder the result to preserve the one from the lucene query.
                if (dbContentItems.Any())
                {
                    var dbContentItemVersionIds = dbContentItems.ToDictionary(x => x.ContentItemVersionId, x => x);
                    var indexedAndInDB = indexedContentItemVersionIds.Where(dbContentItemVersionIds.ContainsKey);
                    luceneQueryResults.Items = indexedAndInDB.Select(x => dbContentItemVersionIds[x]).ToArray();
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

                luceneQueryResults.Items = results;
            }
        });

        return luceneQueryResults;
    }
}
