# Indexing (`OrchardCore.Indexing`)

The `Indexing` module provides services to index content items. It does so by storing an append-only log of content item entries, and providing a service to query this list with a cursor-like interface. An entry can be either an `Update` or a `Deletion` task. This list of tasks can also be seen as an event store for content items.

Other modules can then store their own cursor location for this list, and check for updates and deletions of content items and do custom operations based on these changes.

As of version 3, the `Indexing` module also provide a user interface for all index providers modules like Azure AI Search, Elasticsearch, and Lucene. This user interface is available in the admin panel and allows users to manage their indexes, including creating, updating, and deleting indexes. The UI can be found in `Search` > `Indexes`.

#### Multi-Source Indexing  

You can extend the indexing feature by adding your own indexing source. This allows you to create a custom index provider that can be used to index content items from a specific source. The custom source can be anything, such as a database, an external API, or any other data source.

To register a new source, you can add the following code to your `Startup.cs` file:

```csharp
services.Configure<IndexingOptions>(options =>
{
    // Currently we support AzureAISearch, Elasticsearch, and Lucene providers.
    options.AddIndexingSource("ProviderName", "CustomSource", o =>
    {
        o.DisplayName = S["Custom Source in Provider"];
        o.Description = S["Create a Provider index based on custom source."];
    });
});
```

You'll also need to implement the `IIndexManager` and `IIndexDocumentManager` interfaces and register them as following

```csharp
services.AddKeyedScoped<IIndexManager, CustomSourceIndexManager>("ProviderName");
services.AddKeyedScoped<IIndexDocumentManager, CustomSourceDocumentManager>("ProviderName");
```

Should you need to add custom metadata to the index, you can do so by adding a display driver that is derived by `DisplayDriver<IndexEntity>`.

You may also implement `IIndexEntityHandler` to hook into multiple stages of the index entity lifecycle. This allows you to perform custom operations when the index entity is created, updated, or deleted.

## Recipes

The following recipes are available in the `Indexing` module:

  - `Indexing` allows you to create or update an index.
  - `ResetIndexing` allows you to reset an index.
  - `RebuildIndexing` allows you to rebuild an index.
  
  - 
## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/6jJH9ntqi_A" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/IYKEeYxeNck" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
