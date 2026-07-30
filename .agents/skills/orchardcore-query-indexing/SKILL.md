---
name: orchardcore-query-indexing
description: Works with OrchardCore queries and indexing — SQL/Lucene/Elasticsearch queries, full-text index profiles, content part/field index handlers, and search. Use when the user needs to define a query, index content for search, write an index handler, register an indexing source, or add query/index recipe steps.
---

# OrchardCore Query & Indexing

This skill guides you through OrchardCore's query and indexing systems following project conventions.

Two distinct systems, often confused:

- **Queries** (`OrchardCore.Queries`) — named, stored queries executed on demand (`SQL`, `Lucene`, `Elasticsearch` sources). Return raw rows or content items.
- **Indexing** (`OrchardCore.Indexing` + providers) — build full-text **index profiles** that mirror content into Lucene/Elasticsearch/Azure AI Search for searching. Driven by **index handlers**.

Separate again from **YesSql SQL index tables** (`MapIndex` + `SchemaBuilder`) — those are relational indexes for fast DB queries, covered by the `orchardcore-data-migration` skill, not here.

## Decide what you need

| Goal | System |
|------|--------|
| Run a stored SQL/Lucene query, expose via GraphQL/API | Queries |
| Make content searchable (full-text) | Indexing (index profile + provider) |
| Index a custom part/field's data into search | `IContentPartIndexHandler` / `IContentFieldIndexHandler` |
| Fast relational lookup by a column | YesSql `MapIndex` → see `orchardcore-data-migration` |
| Run a search box over an index | Search (`ISearchService`) |

## Workflow A: define and run a query

### Step 1: Pick a source

`Query` has `Name`, `Source` (`Sql`/`Lucene`/`Elasticsearch`), `ReturnContentItems`, and source-specific metadata (e.g. a SQL `Template`).

### Step 2: Create via recipe or manager

Recipe step:

```json
{
  "name": "queries",
  "Queries": [
    {
      "Source": "Sql",
      "Name": "RecentPosts",
      "Template": "select DocumentId from ContentItemIndex where ContentType = 'BlogPost'",
      "ReturnContentItems": true
    }
  ]
}
```

Programmatically:

```csharp
var query = await _queryManager.NewAsync("Sql", data);
await _queryManager.SaveAsync(query);
```

### Step 3: Execute

```csharp
var query = await _queryManager.GetQueryAsync("RecentPosts");
var results = await _queryManager.ExecuteQueryAsync(query, new Dictionary<string, object>
{
    ["maxResults"] = 10,
});
foreach (var item in results.Items) { /* JsonObject or ContentItem */ }
```

SQL templates are Liquid-rendered (parameters become Liquid vars), parsed by `SqlParser`, then run via Dapper. `ReturnContentItems = true` loads `ContentItem`s by `DocumentId`; otherwise rows are returned as `JsonObject`.

## Workflow B: index content for search

### Step 1: Create an index profile

A profile binds a provider (`Lucene`/`Elasticsearch`/`AzureAISearch`) + a type (`Content`) + which content types to index.

```json
{
  "name": "CreateOrUpdateIndexProfile",
  "indexes": [
    {
      "Name": "BlogPostsLucene",
      "IndexName": "blogposts",
      "ProviderName": "Lucene",
      "Type": "Content",
      "Properties": {
        "ContentIndexMetadata": {
          "IndexLatest": false,
          "IndexedContentTypes": [ "BlogPost" ],
          "Culture": "any"
        },
        "LuceneIndexMetadata": { "AnalyzerName": "standard", "StoreSourceData": true }
      }
    }
  ]
}
```

Content changes raise indexing tasks; `ContentIndexingService` resolves the content item and calls every part/field index handler to build a `DocumentIndex`, which the provider stores.

### Step 2: Index custom part/field data

For a custom part, implement `ContentPartIndexHandler<TPart>`:

```csharp
public class AliasPartIndexHandler : ContentPartIndexHandler<AliasPart>
{
    public override Task BuildIndexAsync(AliasPart part, BuildPartIndexContext context)
    {
        var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;
        foreach (var key in context.Keys)
        {
            context.DocumentIndex.Set(key, part.Alias, options);
        }
        return Task.CompletedTask;
    }
}
```

Fields use `ContentFieldIndexHandler<TField>` with `BuildFieldIndexContext`. `context.Keys` are the index field names to populate; `DocumentIndexOptions` flags control storage:

| Flag | Effect |
|------|--------|
| `Store` | keep the value retrievable from the index |
| `Keyword` | index as a non-analyzed keyword (exact match) |
| `Sanitize` | strip HTML before indexing |
| `None` | analyzed full-text (default text) |

### Step 3: Register the handler

```csharp
services.AddScoped<IContentPartIndexHandler, AliasPartIndexHandler>();
// field:
services.AddScoped<IContentFieldIndexHandler, MyFieldIndexHandler>();
```

### Step 4: Reset / rebuild

Recipe steps `ResetIndex` (re-sync from last position) and `RebuildIndex` (drop + rebuild) manage existing indexes. The admin Search/Indexing UI exposes the same.

## Quick Reference

### Query APIs

| Type | Member |
|------|--------|
| `IQueryManager` | `NewAsync`, `SaveAsync`, `GetQueryAsync`, `ExecuteQueryAsync`, `ListQueriesAsync` |
| `IQuerySource` | `Name`, `ExecuteQueryAsync(query, parameters)` — implement to add a source |
| `IQueryResults` | `IEnumerable<object> Items` |

### Indexing APIs

| Type | Member |
|------|--------|
| `IIndexProfileManager` | `NewAsync`, `CreateAsync`, `UpdateAsync`, `FindByNameAsync`, `SynchronizeAsync`, `ResetAsync` |
| `IDocumentIndexHandler` | `BuildIndexAsync`, `DocumentsAddedOrUpdatedAsync`, `DocumentsDeletedAsync` |
| `IContentPartIndexHandler` / `IContentFieldIndexHandler` | `BuildIndexAsync(...)` to populate `DocumentIndex` |
| `IIndexManager` (per provider) | `CreateAsync`, `RebuildAsync`, `DeleteAsync`, `ExistsAsync` |
| `IDocumentIndexManager` | `AddOrUpdateDocumentsAsync`, `DeleteDocumentsAsync` |
| `DocumentIndex` | `Set(name, value, options)` overloads; `Types` enum |

### Register an indexing source

```csharp
services.AddIndexingSource<TIndexManager, TDocumentIndexManager, TIndexNameProvider>(
    providerName, implementationType);
// e.g. the Lucene module: services.AddLuceneIndexingSource(implementationType);
```

### Search

`ISearchService.SearchAsync(IndexProfile index, string term, int start, int size)` → `SearchResult`. Providers: `OrchardCore.Search.Lucene`, `.Elasticsearch`, `.AzureAI`.

## Gotchas

- "Index" is overloaded: full-text **index profiles** (this skill) vs YesSql **MapIndex tables** (`orchardcore-data-migration`). Don't mix them.
- Query recipe step is `queries`; index profile step is `CreateOrUpdateIndexProfile`. Older `lucene-index` / `ElasticIndexSettings` steps are deprecated — prefer `CreateOrUpdateIndexProfile`.
- `IndexLatest: true` indexes draft (latest) versions; `false` indexes published. Match to your search UI.
- SQL query `Template` is Liquid first, then SQL — parameters are Liquid variables, not ADO placeholders.

## References

- `references/indexing.md` — index profiles, handlers, DocumentIndex, providers, recipe steps
- `src/docs/reference/modules/Queries/README.md` (repo)
- `src/docs/reference/modules/Indexing/README.md` (repo)
- `src/docs/reference/modules/{Lucene,Elasticsearch,SQLIndexing}/README.md` (repo)
- `AGENTS.md` (repo root) — build commands
