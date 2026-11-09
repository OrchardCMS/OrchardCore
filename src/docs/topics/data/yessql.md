# How YesSql works

Orchard Core does not use an ORM like Entity Framework. Most of its persistent application data — including content items, users, settings, and workflows — is stored with [YesSql](https://github.com/sebastienros/yessql), a .NET **document database interface over a relational database**. You get the flexibility of a document store (no schema to migrate when your objects change) while still running on SQL Server, SQLite, MySQL, or PostgreSQL.

## Documents

When an object is saved, YesSql serializes it to JSON and stores it as a row in a document table. The default collection uses the `Document` table; named collections use separate document tables. Each tenant can use a table prefix:

| Id | Type | Content | Version |
| --- | --- | --- | --- |
| 42 | `OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions` | `{ "ContentItemId": "4tavbc...", "DisplayText": "My blog post", ... }` | 3 |

This is why adding a field or a part to a content type in Orchard Core never requires a database migration: the shape of the JSON just changes.

The obvious limitation is that you cannot efficiently query inside a JSON blob with SQL. That is what indexes are for.

## Indexes

An index is a plain C# class holding the properties you want to query on. YesSql maintains **regular SQL tables for index data**. Map index rows have a `DocumentId` column pointing back to their document table, while reduce indexes use a separate table to associate documents with aggregated rows. When a document is created, updated, or deleted, its index rows are recomputed within the same transaction.

There are two kinds:

- **`MapIndex`**: each index row maps to one document. A mapping can emit one or more rows for a document. For example, `ContentItemIndex` maps every content item to its `ContentType`, `Published`, `Owner`, etc., and `AliasPartIndex` maps every content item that has an `AliasPart` to its alias.
- **`ReduceIndex`**: each index row aggregates a group of documents, like a SQL `GROUP BY`. It is used for counting or grouping scenarios.

A queryable property must be part of an index; everything else stays only in the JSON document.

### Defining an index

An `IndexProvider<T>` describes how to map a document type to index rows:

```csharp
using YesSql.Indexes;

public class ProductIndex : MapIndex
{
    public string Sku { get; set; }
    public decimal Price { get; set; }
}

public class ProductIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<ProductIndex>()
            .When(contentItem => contentItem.Has<ProductPart>())
            .Map(contentItem =>
            {
                var part = contentItem.As<ProductPart>();

                return new ProductIndex
                {
                    Sku = part.Sku,
                    Price = part.Price,
                };
            });
    }
}
```

In a module, register the provider in `Startup.ConfigureServices()`:

```csharp
services.AddIndexProvider<ProductIndexProvider>();
```

And create the index table in a [data migration](../../reference/modules/Migrations/README.md):

```csharp
public async Task<int> CreateAsync()
{
    await SchemaBuilder.CreateMapIndexTableAsync<ProductIndex>(table => table
        .Column<string>("Sku", column => column.WithLength(64))
        .Column<decimal>("Price")
    );

    return 1;
}
```

### Declaring the records of an index

A map index can have several records for a document, so a query joining it groups its matches by document to return each document once, and counts distinct documents. A page then reads every match before it can be sorted. An index provider can declare what its index holds, so that YesSql doesn't need to:

- `SingleRecordPerDocument()`: the map returns at most one record per document. A query that only joins indexes declaring it isn't grouped, so a page stops as soon as it's full when a database index serves its order, and its count isn't distinct. Saving a document throws if the map returns several records.
- `ExclusiveToDocumentType()`: only documents of the provider's type have records in the index, so a count reads the index tables without joining the document table.

```csharp
context.For<ProductIndex>()
    .ExclusiveToDocumentType()
    .SingleRecordPerDocument()
    .When(contentItem => contentItem.Has<ProductPart>())
    .Map(contentItem => ...);
```

YesSql can't verify the records that are already in the table, for example those written before the map returned a single record. A query is still grouped when it joins an index that doesn't declare a single record per document. A query that these declarations make slower, such as one where the database walks an index in the requested order looking for rare matches, can opt out with `NoIndexOptimizations()`. A query that joins an index with several records per document, but only matches one of them, can skip the grouping with `SkipDeduplication()`.

These indexes declare both: `ContentItemIndex`, `ContainedPartIndex`, `LocalizedContentItemIndex`, `UserIndex`, `WorkflowIndex`, `DataPipelineRunIndex`, `AuditTrailEventIndex` and `NotificationIndex`.

Real examples in the Orchard Core source: [`ContentItemIndex`](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.ContentManagement/Records/ContentItemIndex.cs), [`AliasPartIndex`](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Alias/Indexes/AliasPartIndex.cs) and its [migration](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Alias/Migrations.cs).

## The session

YesSql document reads and writes go through `ISession`, a scoped unit of work registered in the dependency injection container. `SaveAsync` and `Delete` are buffered in memory; the SQL commands run in a single transaction when `SaveChangesAsync` is called. Orchard Core automatically commits the session resolved from a tenant shell scope when that scope ends, normally at the end of a request.

```csharp
public sealed class MyController : Controller
{
    private readonly ISession _session;

    public MyController(ISession session)
    {
        _session = session;
    }

    public async Task<IActionResult> Cheap()
    {
        // Query documents through an index.
        var cheapProducts = await _session
            .Query<ContentItem, ProductIndex>(index => index.Price < 10)
            .ListAsync();

        return View(cheapProducts);
    }
}
```

The query filters on the index table with regular SQL, then loads the matching JSON documents and deserializes them. The session caches documents: loading the same document twice in a request returns the same instance.

For content items specifically, prefer the higher-level `IContentManager` (or `IOrchardHelper` extensions like `QueryContentItemsAsync`), which handle loading, versioning, and handlers; drop down to `ISession` when you need to query your own indexes.

## Configuration

The database provider, connection string, and table prefix are chosen per tenant at setup time. `YesSqlOptions` (command page size, isolation level, custom serializer...) and table naming presets are configurable: see [Data (`OrchardCore.Data`)](../../reference/modules/Data/README.md). For raw SQL against the same database, use `IDbConnectionAccessor` as described in that page, or the [SQL queries](../../reference/modules/Queries/README.md) module.

## Going further

- [YesSql repository and samples](https://github.com/sebastienros/yessql)
- [YesSql wiki](https://github.com/sebastienros/yessql/wiki)
