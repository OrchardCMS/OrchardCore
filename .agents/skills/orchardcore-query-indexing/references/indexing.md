# Indexing & query reference

## Two index concepts (keep separate)

| | Full-text index profile | YesSql MapIndex table |
|---|---|---|
| Module | `OrchardCore.Indexing` + provider | `OrchardCore.Data` (`SchemaBuilder`) |
| Storage | Lucene / Elasticsearch / Azure AI | SQL table |
| Built by | `IDocumentIndexHandler`, part/field handlers | `IndexProvider<T>.Describe` + migration |
| For | search / full-text | fast relational lookup |
| Skill | this one | `orchardcore-data-migration` |

## Query model

```csharp
public class Query : Entity
{
    public string Name { get; set; }
    public string Source { get; set; }   // "Sql", "Lucene", "Elasticsearch"
    public string Schema { get; set; }
    public bool ReturnContentItems { get; set; }
}
```

Source-specific data lives in the entity bag, e.g. `SqlQueryMetadata.Template`.

### IQueryManager

```csharp
Task<Query> NewAsync(string source, JsonNode data = null);
Task<Query> GetQueryAsync(string name);
Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
Task SaveAsync(params Query[] queries);
Task<IEnumerable<Query>> ListQueriesAsync(QueryContext context = null);
```

### Custom query source

```csharp
public interface IQuerySource
{
    string Name { get; }
    Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters);
}
```

Register with `services.AddScoped<IQuerySource, MyQuerySource>();`. SQL source flow: Liquid render template → `SqlParser.TryParse` (schema/dialect/table-prefix aware) → Dapper `QueryAsync` → optionally load `ContentItem`s by `DocumentId`.

## Index profile

```csharp
public sealed class IndexProfile : Entity
{
    public string Id { get; set; }
    public string ProviderName { get; set; } // Lucene / Elasticsearch / AzureAISearch
    public string Type { get; set; }         // Content
    public string Name { get; set; }         // unique profile name
    public string IndexName { get; set; }    // logical index name
    public string IndexFullName { get; set; }// provider-prefixed physical name
}
```

`IIndexProfileManager`: `NewAsync(provider, type, data)`, `CreateAsync`, `UpdateAsync`, `FindByNameAsync`, `ValidateAsync`, `SynchronizeAsync`, `ResetAsync`.

## How content gets indexed

1. A content item is published/updated → a `RecordIndexingTask` is queued (`CreateIndexingTaskContentHandler`).
2. `ContentIndexingService` reads the task, loads the content item (latest or published per `ContentIndexMetadata.IndexLatest`), and checks it's an indexed type.
3. It builds a `DocumentIndex`, invoking every `IContentPartIndexHandler` and `IContentFieldIndexHandler` for the item's parts/fields.
4. `IDocumentIndexManager.AddOrUpdateDocumentsAsync` hands the documents to the provider.

## Index handlers

Part handler:

```csharp
public class AliasPartIndexHandler : ContentPartIndexHandler<AliasPart>
{
    public override Task BuildIndexAsync(AliasPart part, BuildPartIndexContext context)
    {
        var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;
        foreach (var key in context.Keys)
            context.DocumentIndex.Set(key, part.Alias, options);
        return Task.CompletedTask;
    }
}
```

Field handler:

```csharp
public class BooleanFieldIndexHandler : ContentFieldIndexHandler<BooleanField>
{
    public override Task BuildIndexAsync(BooleanField field, BuildFieldIndexContext context)
    {
        var options = context.Settings.ToOptions();
        foreach (var key in context.Keys)
            context.DocumentIndex.Set(key, field.Value, options);
        return Task.CompletedTask;
    }
}
```

`context.Keys` — the index field name(s) to write (handles aliasing/prefixing). Always loop them.

### DocumentIndex.Set

Overloads for `string`, `IHtmlContent`, `DateTimeOffset?`, `int?`, `bool?`, `double?`, `decimal?`, `GeoPoint`, `float[]` (vector), and `object`. `DocumentIndex.Types`: `Integer, Text, DateTime, Boolean, Number, GeoPoint, Complex, Vector`.

### DocumentIndexOptions

```
None = 0, Store = 1, Sanitize = 2, Keyword = 4   // [Flags]
```

## Registering a provider (advanced)

```csharp
services.AddIndexingSource<TManager, TDocumentManager, TNamingProvider>(providerName, implementationType);
```

- `IIndexManager` — `CreateAsync/RebuildAsync/DeleteAsync/ExistsAsync` for the physical index.
- `IDocumentIndexManager` — bulk `AddOrUpdate/Delete` documents.
- `IIndexNameProvider` — maps logical → physical names.

The Lucene module wraps this as `AddLuceneIndexingSource(implementationType)` and registers a display name via `IndexingOptions`.

## Recipe steps

| Step | Purpose |
|------|---------|
| `queries` | create/update stored queries |
| `CreateOrUpdateIndexProfile` | create/update index profiles (any provider) |
| `ResetIndex` | reset sync position, re-index incrementally |
| `RebuildIndex` | delete and fully rebuild |

Deprecated: `lucene-index`, `ElasticIndexSettings` — migrate to `CreateOrUpdateIndexProfile`.

## Search

```csharp
public interface ISearchService
{
    string Name { get; }
    Task<SearchResult> SearchAsync(IndexProfile index, string term, int start, int size);
}
```

`ISearchHandler.SearchedAsync(SearchContext)` for post-search hooks. Provider cores: `OrchardCore.Search.Lucene.Core`, `.Elasticsearch.Core`, `.AzureAI.Core`.

## Startup wiring (content indexing)

```csharp
services.AddIndexingCore();
services.AddIndexProfileHandler<ContentIndexProfileHandler>();
// content feature:
services.AddScoped<IContentHandler, CreateIndexingTaskContentHandler>();
services.TryAddScoped<ContentIndexingService>();
```
