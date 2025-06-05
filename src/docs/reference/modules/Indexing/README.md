# Indexing (`OrchardCore.Indexing`)

The `Indexing` module provides a flexible and extensible infrastructure for indexing data in Orchard Core. It is designed to support various types of data and multiple index providers such as Azure AI Search, Elasticsearch, and Lucene.

It exposes a provider-agnostic model that allows developers to define custom indexing sources and integrate them into a unified UI. This UI, introduced in version 3, is available in the admin dashboard under **Search** > **Indexes**, and allows users to create, update, reset, rebuild, and delete indexes.

The module is agnostic of specific data types—it does not assume that the indexed content is based on Orchard Core content items.

## Indexing Content Items

Although the `Indexing` module is generic, it includes built-in support for indexing Orchard Core content items.

Content indexing works by recording an **append-only log of content item tasks**, where each task represents either an `Update` or a `Deletion`. This log acts as a change history and can be queried using a **cursor-based interface**. This enables other modules to store their own cursor positions and react to content item changes—effectively using the log as an event stream.

This approach decouples indexing consumers from the content system, allowing modules to process updates at their own pace and apply custom logic for search, analytics, or other purposes.

note !!!
     Content item indexing is implemented as a consumer of the core `Indexing` infrastructure, not a requirement of it.

## Multi-Source Indexing

The `Indexing` module can be extended to index data from arbitrary sources, such as external APIs, relational databases, or custom data stores.

To define a custom index source, implement the following interfaces:

- `IIndexManager`
- `IIndexDocumentManager`
- `IIndexNameProvider`

Then register your custom source in your module's `Startup.cs`:

```csharp
// Supported providers include 'AzureAISearch', 'Elasticsearch', and 'Lucene'.
services.AddIndexingSource<CustomSourceIndexManager, CustomSourceDocumentManager, CustomSourceIndexNameProvider>(
    "ProviderName", // e.g., "AzureAISearch", "Lucene", "Elasticsearch"
    "CustomSource", // Unique source name
    o =>
    {
        o.DisplayName = S["Custom Source in Provider"];
        o.Description = S["Creates a provider index based on a custom data source."];
    });
```

To provide additional configuration or metadata through the admin UI, implement a display driver by deriving from `DisplayDriver<IndexProfile>`.

You can also implement `IIndexProfileHandler` to respond to lifecycle events during indexing—such as when an entity is created, updated, or deleted.

note !!!
     If you are implementing a custom provider, use the `Content` type to index content items. This ensures compatibility with the built-in content indexing infrastructure, enabling automatic indexing of content items and providing the necessary UI to configure content types and related settings.

## Recipes

The following deployment recipes are available with the `Indexing` module:

- `IndexProfile`: Creates or updates an index.
- `ResetIndexProfile`: Resets an index to its initial state.
- `RebuildIndexProfile`: Rebuilds an index from scratch.
