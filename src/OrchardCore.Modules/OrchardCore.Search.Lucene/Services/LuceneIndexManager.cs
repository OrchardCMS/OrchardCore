using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene.Core;
using OrchardCore.Modules;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Models;
using Spatial4n.Context;

namespace OrchardCore.Search.Lucene;

/// <summary>
/// Provides methods to manage physical Lucene indices.
/// This class is provided as a singleton to that the index searcher can be reused across requests.
/// </summary>
public sealed class LuceneIndexManager : IIndexManager, IDocumentIndexManager
{
    private readonly ILuceneIndexStore _indexStore;
    private readonly ILuceneIndexingState _indexingState;
    private readonly ILogger _logger;

    private readonly IEnumerable<IIndexEvents> _indexEvents;
    private readonly SpatialContext _ctx;
    private readonly GeohashPrefixTree _grid;

    public LuceneIndexManager(
        ILuceneIndexStore indexStore,
        ILuceneIndexingState indexingState,
        ILogger<LuceneIndexManager> logger,
        IEnumerable<IIndexEvents> indexEvents
        )
    {
        _indexStore = indexStore;
        _indexingState = indexingState;
        _logger = logger;

        _indexEvents = indexEvents;

        // Typical geospatial context.
        // These can also be constructed from SpatialContextFactory.
        _ctx = SpatialContext.Geo;

        var maxLevels = 11; // Results in sub-meter precision for geohash.

        // TODO demo lookup by detail distance.
        // This can also be constructed from SpatialPrefixTreeFactory.
        _grid = new GeohashPrefixTree(_ctx, maxLevels);
    }

    public async Task<bool> CreateAsync(IndexEntity index)
    {
        if (await ExistsAsync(index.IndexFullName))
        {
            return false;
        }

        var context = new IndexCreateContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

        try
        {
            await _indexStore.WriteAndClose(index);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), context, _logger);

        return true;
    }

    public async Task<bool> RebuildAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var context = new IndexRebuildContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

        if (await ExistsAsync(index.IndexFullName))
        {
            await _indexStore.RemoveAsync(index.IndexFullName);
        }

        try
        {
            await _indexStore.WriteAndClose(index);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

        return true;
    }

    public async Task<bool> DeleteAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var context = new IndexRemoveContext(indexFullName);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);

        try
        {
            await _indexStore.RemoveAsync(indexFullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing index: {IndexFullName}", indexFullName);

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

        return true;
    }

    public Task<bool> ExistsAsync(string indexFullName)
        => _indexStore.ExistsAsync(indexFullName);

    public async Task<bool> DeleteDocumentsAsync(IndexEntity index, IEnumerable<string> documentIds)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(documentIds);

        if (!documentIds.Any())
        {
            return false;
        }

        await _indexStore.WriteAsync(index, writer =>
        {
            var metadata = index.As<LuceneIndexMetadata>();

            writer.DeleteDocuments(documentIds.Select(id => new Term(metadata.IndexMappings.KeyFieldName, id)).ToArray());

            writer.Commit();
        });

        return true;
    }

    public async Task<bool> MergeOrUploadDocumentsAsync(IndexEntity index, IEnumerable<DocumentIndexBase> documents)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(documents);

        if (!documents.Any())
        {
            return false;
        }

        var contentMetadata = index.As<LuceneContentIndexMetadata>();

        try
        {
            await _indexStore.WriteAsync(index, writer =>
            {
                foreach (var indexDocument in documents)
                {
                    writer.AddDocument(CreateLuceneDocument(indexDocument, contentMetadata.StoreSourceData));
                }

                writer.Commit();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging or uploading documents to index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAllDocumentsAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        try
        {
            await _indexStore.WriteAsync(index, writer =>
            {
                writer.DeleteAll();
                writer.Commit();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all documents from index: {IndexFullName}", index.IndexFullName);

            return false;
        }

        return true;
    }

    public Task<long> GetLastTaskIdAsync(IndexEntity index)
        => _indexingState.GetLastTaskIdAsync(index.IndexFullName);

    public Task SetLastTaskIdAsync(IndexEntity index, long lastTaskId)
        => _indexingState.SetLastTaskIdAsync(index.IndexFullName, lastTaskId);

    public IContentIndexSettings GetContentIndexSettings()
         => new LuceneContentIndexSettings();

    public Task SearchAsync(IndexEntity index, Func<IndexSearcher, Task> searcher)
        => _indexStore.SearchAsync(index, searcher);

    private Document CreateLuceneDocument(DocumentIndexBase document, bool storeSourceData)
    {
        var doc = new Document();

        if (document is DocumentIndex documentIndex)
        {
            // Always store the content item id and version id.
            // These fields need to be indexed as a StringField because it needs to be searchable for the writer.DeleteDocuments method.
            // Else it won't be able to prune oldest draft from the indexes.
            // Maybe eventually find a way to remove a document from a StoredDocument.
            doc.Add(new StringField(ContentIndexingConstants.ContentItemIdKey, documentIndex.ContentItemId.ToString(), Field.Store.YES));
            doc.Add(new StringField(ContentIndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId?.ToString(), Field.Store.YES));

            if (storeSourceData)
            {
                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + ContentIndexingConstants.ContentItemIdKey, documentIndex.ContentItemId.ToString()));
                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + ContentIndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId.ToString()));
            }
        }

        foreach (var entry in document.Entries)
        {
            var store = entry.Options.HasFlag(DocumentIndexOptions.Store)
                        ? Field.Store.YES
                        : Field.Store.NO;

            switch (entry.Type)
            {
                case DocumentIndexBase.Types.Boolean:
                    // Store "true"/"false" for boolean.
                    doc.Add(new StringField(entry.Name, Convert.ToString(entry.Value).ToLowerInvariant(), store));

                    if (storeSourceData)
                    {
                        doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, Convert.ToString(entry.Value).ToLowerInvariant()));
                    }
                    break;

                case DocumentIndexBase.Types.DateTime:
                    if (entry.Value != null)
                    {
                        if (entry.Value is DateTimeOffset)
                        {
                            doc.Add(new StringField(entry.Name, DateTools.DateToString(((DateTimeOffset)entry.Value).UtcDateTime, DateResolution.SECOND), store));
                        }
                        else
                        {
                            doc.Add(new StringField(entry.Name, DateTools.DateToString(((DateTime)entry.Value).ToUniversalTime(), DateResolution.SECOND), store));
                        }

                        if (storeSourceData)
                        {
                            if (entry.Value is DateTimeOffset)
                            {
                                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, DateTools.DateToString(((DateTimeOffset)entry.Value).UtcDateTime, DateResolution.SECOND)));
                            }
                            else
                            {
                                doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, DateTools.DateToString(((DateTime)entry.Value).ToUniversalTime(), DateResolution.SECOND)));
                            }
                        }
                    }
                    else
                    {
                        doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                    }
                    break;

                case DocumentIndexBase.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        doc.Add(new Int64Field(entry.Name, value, store));

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, value));
                        }
                    }
                    else
                    {
                        doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                    }

                    break;

                case DocumentIndexBase.Types.Number:
                    if (entry.Value != null)
                    {
                        doc.Add(new DoubleField(entry.Name, Convert.ToDouble(entry.Value), store));

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, Convert.ToDouble(entry.Value)));
                        }
                    }
                    else
                    {
                        doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                    }
                    break;

                case DocumentIndexBase.Types.Text:
                    if (entry.Value != null && !string.IsNullOrEmpty(Convert.ToString(entry.Value)))
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (entry.Options.HasFlag(DocumentIndexOptions.Keyword))
                        {
                            doc.Add(new StringField(entry.Name, stringValue, store));
                        }
                        else
                        {
                            doc.Add(new TextField(entry.Name, stringValue, store));
                        }

                        // This is for Elasticsearch Queries compatibility since a keyword field is always indexed
                        // by default when indexing without explicit mapping in Elasticsearch.
                        // Keyword ignore above 256 chars by default.
                        if (store == Field.Store.NO && !entry.Options.HasFlag(DocumentIndexOptions.Keyword) && stringValue.Length <= 256)
                        {
                            doc.Add(new StringField($"{entry.Name}.keyword", stringValue, Field.Store.NO));
                        }

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + entry.Name, stringValue));
                        }
                    }
                    else
                    {
                        if (entry.Options.HasFlag(DocumentIndexOptions.Keyword))
                        {
                            doc.Add(new StringField(entry.Name, ContentIndexingConstants.NullValue, store));
                        }
                        else
                        {
                            doc.Add(new TextField(entry.Name, ContentIndexingConstants.NullValue, store));
                        }
                    }
                    break;

                case DocumentIndexBase.Types.GeoPoint:
                    var strategy = new RecursivePrefixTreeStrategy(_grid, entry.Name);

                    if (entry.Value != null && entry.Value is DocumentIndexBase.GeoPoint point)
                    {
                        var geoPoint = _ctx.MakePoint((double)point.Longitude, (double)point.Latitude);
                        foreach (var field in strategy.CreateIndexableFields(geoPoint))
                        {
                            doc.Add(field);
                        }

                        doc.Add(new StoredField(strategy.FieldName, $"{point.Latitude},{point.Longitude}"));

                        if (storeSourceData)
                        {
                            doc.Add(new StoredField(ContentIndexingConstants.SourceKey + strategy.FieldName, $"{point.Latitude},{point.Longitude}"));
                        }
                    }
                    else
                    {
                        doc.Add(new StoredField(strategy.FieldName, ContentIndexingConstants.NullValue));
                    }
                    break;
            }
        }

        return doc;
    }
}
