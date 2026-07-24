# How YesSql works

Orchard Core does not use an ORM like Entity Framework. All its data — content items, users, settings, workflows... — is stored with [YesSql](https://github.com/sebastienros/yessql), a .NET **document database interface over a relational database**. You get the flexibility of a document store (no schema to migrate when your objects change) while still running on SQL Server, SQLite, MySQL or PostgreSQL.

## Documents

When an object is saved, YesSql serializes it to JSON and stores it as a row in a single `Document` table (per tenant, with an optional table prefix):

| Id | Type | Content | Version |
| --- | --- | --- | --- |
| 42 | `OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions` | `{ "ContentItemId": "4tavbc...", "DisplayText": "My blog post", ... }` | 3 |

This is why adding a field or a part to a content type in Orchard Core never requires a database migration: the shape of the JSON just changes.

The obvious limitation is that you cannot efficiently query inside a JSON blob with SQL. That is what indexes are for.

## Indexes

An index is a plain C# class holding the properties you want to query on. YesSql maintains one **regular SQL table per index type**, with a `DocumentId` column pointing back to the `Document` table. When a document is created, updated, or deleted, its index rows are recomputed within the same transaction.

There are two kinds:

- **`MapIndex`**: one (or several) index rows per document. For example `ContentItemIndex` maps every content item to its `ContentType`, `Published`, `Owner`, etc., and `AliasPartIndex` maps every content item that has an `AliasPart` to its alias.
- **`ReduceIndex`**: one row per group of documents, aggregating values (like a `GROUP BY`). Used for counting or grouping scenarios.

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

Real examples in the Orchard Core source: [`ContentItemIndex`](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.ContentManagement/Records/ContentItemIndex.cs), [`AliasPartIndex`](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Alias/Indexes/AliasPartIndex.cs) and its [migration](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Alias/Migrations.cs).

## The session

All reads and writes go through `ISession`, a unit of work registered in the dependency injection container (one per request). `Save` and `Delete` are buffered in memory; the SQL commands run in a single transaction that is committed at the end of the request, or when `SaveChangesAsync` is called explicitly.

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

The query filters on the index table with regular SQL, then loads the matching JSON documents and deserializes them. Documents are cached by the session: loading the same document twice in a request returns the same instance.

For content items specifically, prefer the higher-level `IContentManager` (or `IOrchardHelper` extensions like `QueryContentItemsAsync`) which take care of loading, versioning, and handlers; drop down to `ISession` when you need to query on your own indexes.

## Configuration

The database provider, connection string, and table prefix are chosen per tenant at setup time. `YesSqlOptions` (command page size, isolation level, custom serializer...) and table naming presets are configurable: see [Data (`OrchardCore.Data`)](../../reference/modules/Data/README.md). For raw SQL against the same database, use `IDbConnectionAccessor` as described in that page, or the [SQL queries](../../reference/modules/Queries/README.md) module.

## Going further

- [YesSql repository and samples](https://github.com/sebastienros/yessql)
- [YesSql wiki](https://github.com/sebastienros/yessql/wiki)
