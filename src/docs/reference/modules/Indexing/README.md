# Indexing (`OrchardCore.Indexing`)

The `Indexing` module provides a flexible and extensible infrastructure for indexing any kind of data in Orchard Core. It supports multiple index providers—including Lucene, Elasticsearch, and Azure AI Search—and is designed to be fully agnostic of specific data types.

At its core, the module maintains an **append-only log of indexing tasks**, where each task represents either an `Update` or a `Deletion`. This log forms a chronological record of changes that can be queried using a **cursor-based interface**. This design allows consumers to track changes independently and implement custom behavior, such as syncing to external systems, rebuilding search indexes, or reacting to specific data events.

Although often used to index content items, the system is **not limited to content**—you can index any kind of document, such as user records, products, or data from external APIs.

!!! note
    Content item indexing is implemented as a consumer of the core `Indexing` infrastructure. It is optional and does not constrain the indexing module to Orchard Core content items.

## Indexing UI

Starting with Orchard Core version 3, the module provides a unified user interface under **Search** > **Indexes** in the admin dashboard. This UI supports the creation, configuration, and lifecycle management of indexes. You can:

* Create and configure index profiles
* Reset or rebuild existing indexes
* View provider-specific options
* Configure which data types to index (for example, content types, if applicable)

## Indexing Content Items

While the infrastructure is generic, the module includes built-in support for Orchard Core content items via the `Content` category.

When enabled, content item indexing uses the append-only task log to track changes. Each entry in the log indicates an update or deletion of a content item. Other modules can consume this log using their own cursor positions, enabling them to:

* Process changes at their own pace
* Implement custom search pipelines
* Integrate with analytics, auditing, or event-driven workflows

This event-log-style system encourages loose coupling between the indexing infrastructure and content consumers.

## Indexing Other Data (Custom Sources)

The indexing system supports **custom data sources** out of the box. You can index data from:

* External services (e.g., REST APIs)
* Relational or NoSQL databases
* In-memory structures or domain-specific objects

To define a custom source, implement the following interfaces:

* `IIndexManager`: Controls how indexing tasks are managed.
* `IIndexDocumentManager`: Converts entities into indexable documents.
* `IIndexNameProvider`: Provides names for index profiles.

Then, register your custom source in `Startup.cs`:

```csharp
services.AddIndexingSource<CustomSourceIndexManager, CustomSourceDocumentIndexManager, CustomSourceIndexNameProvider>(
    "ProviderName", // e.g., "Lucene", "Elasticsearch", "AzureAISearch"
    "CustomCategory", // Unique source category name
    o =>
    {
        o.DisplayName = S["Custom Source in Provider"];
        o.Description = S["Creates an index for a custom data source using the selected provider."];
    });
```

If you need UI integration, you can:

* Create a custom display driver by inheriting from `DisplayDriver<IndexProfile>` to provide configuration screens
* Implement `IIndexProfileHandler` to react to lifecycle events like index creation, update, or deletion

## Recipe steps

### Create Index Profile step

Index profile can be created during recipe execution using the `IndexProfile` step.

Here is a sample step:

```json
{
  "steps":[
    {
      "name":"CreateOrUpdateIndexProfile",
      "indexes": [
	    {
		    "Id": "The id",
		    "Name": "UniqueName",
            "IndexName": "blogposts",
		    "ProviderName": "ProviderName",
		    "Type": "Content",
		    "Properties": {
			    "ContentIndexMetadata": {
				    "IndexLatest": false,
				    "IndexedContentTypes": ["BlogPosts"],
				    "Culture": "any"
			    }
		    }
	    }
      ]
    }
  ]
}
```

!!! note
    To index Orchard Core content items, use the built-in `Content` category. This ensures full compatibility with the content indexing UI and configuration experience.

### Reset Index Profile Step

Restarts the indexing process from the beginning to update current content items.

Existing entries in the index are preserved; new or updated items are added as needed. The re-indexing operation runs asynchronously in the background, ensuring the index is populated without blocking other operations.

```json
{
  "steps":[
    {
      "name":"ResetIndexProfile",
      "IndexeIds":[
        "IndexName1", // Here you can specify the index id or name to reset.
        "IndexName2"
      ]
    }
  ]
}
```

To reset all indices:

```json
{
  "steps":[
    {
      "name":"ResetIndexProfile",
      "IncludeAll":true
    }
  ]
}
```


### rebuild Index Profile Step

Rebuilds the indexing process from the beginning to update current content items.

This operation deletes the existing index and rebuilds it from scratch. The re-indexing process runs asynchronously in the background, ensuring the index is repopulated without blocking other operations.

```json
{
  "steps":[
    {
      "name":"RebuildIndexProfile",
      "IndexeIds":[
        "IndexName1", // Here you can specify the index id or name to reset.
        "IndexName2"
      ]
    }
  ]
}
```

To reset all indices:

```json
{
  "steps":[
    {
      "name":"RebuildIndexProfile",
      "IncludeAll":true
    }
  ]
}
```
