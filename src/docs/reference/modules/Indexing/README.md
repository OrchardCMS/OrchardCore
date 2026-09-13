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

Index profiles can be created or updated during recipe execution using the
`CreateOrUpdateIndexProfile` step.

For an existing profile, incoming content settings (`IndexLatest`, `Culture`, and
`IndexedContentTypes`) and Lucene settings (`AnalyzerName` and `StoreSourceData`)
use the same handlers as creation. Omitted values preserve the existing settings.
An empty content-type list fails shared validation and leaves the stored profile
unchanged. Updating a definition does not imply that existing documents have been
reindexed; run the appropriate indexing operation when changing indexed data.

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
      "name":"ResetIndex",
      "indexNames":[
        "IndexName1",
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
      "name":"ResetIndex",
      "IncludeAll":true
    }
  ]
}
```


### rebuild Index Step

Rebuilds the indexing process from the beginning to update current content items.

This operation deletes the existing index and rebuilds it from scratch. The re-indexing process runs asynchronously in the background, ensuring the index is repopulated without blocking other operations.

```json
{
  "steps":[
    {
      "name":"RebuildIndex",
      "indexNames":[
        "IndexName1",
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
      "name":"RebuildIndex",
      "IncludeAll":true
    }
  ]
}
```

## Remote index discovery

With `OrchardCore.Indexing` enabled, the tenant exposes the `indexes` remote-management
capability. Discovery requires API bearer authentication, `AccessRemoteManagement`
and `ManageIndexes`. The same permission checks apply to in-process MCP invocation.

```bash
pomi indexes providers list
pomi indexes list --page 1 --page-size 50
pomi indexes list --search Articles --page 1 --page-size 20
pomi indexes show <id>
```

`GET api/indexes` uses the existing index store's one-based paging contract. `page`
defaults to 1 and `pageSize` defaults to 50, with a maximum of 200. Negative/zero
values and offsets beyond a 32-bit integer are rejected. `search` filters profile
names using the configured store's comparison rules. Responses contain `page`,
`pageSize`, `totalCount` and `items`; names determine ordering.

`GET api/indexes/by-id?id=...` retrieves an index by its stable administrative ID,
returning `404` when it is absent. Each index response contains `id`, `name`,
`indexName`, `providerName`, `type` and `createdUtc`. It omits the properties bag,
physical backend name, author and owner fields. The profile name, logical backend
index name and administrative ID are distinct identifiers.

`GET api/indexes/providers` describes registered providers and their source types.
It returns provider names/display names and source types/display names/descriptions.
Registration does not promise that every provider supports the same remote mutation
or execution operations; inspect the live command catalog before invoking them.

With tenant MCP enabled, these operations are available as `indexes_list`,
`indexes_show` and `indexes_providers_list`. Reads use the existing profile manager
and registered indexing options; they do not serialize backend configuration objects.

## Coordinating profiles and provider resources

`IIndexProfileManagementService` coordinates creation and deletion for the admin UI
and recipe creation. Extensions that need both a local profile and a provider index
can use this service instead of repeating the coordination around `IIndexProfileManager`.

Creation validates the profile, saves it locally so creation handlers can populate
metadata, then asks the keyed `IIndexManager` to create the provider index. A rejected
provider creation removes the local profile. If that compensation fails, the result
is `LocalDeleteFailed`. Provider exceptions propagate and retain the local profile
because the provider outcome may be uncertain. Synchronization is scheduled only
after successful creation; it is not a completion signal for indexing.

Deletion checks whether the provider index exists, removes it if necessary, and then
removes the local profile. Provider rejection preserves the local profile. The admin's
force-delete option permits local removal when the provider is missing or rejects
deletion, but provider exceptions still propagate. Results distinguish success,
unavailable providers, provider rejection and failure to remove the local profile.

The default profile manager propagates failures from initialization, mutation and
validation handlers. It does not save after a creating/updating/validation handler
fails. Failed updates restore the tracked values from before the updating handlers,
including nested extension properties. A handler failure after persistence also
propagates, but does not undo stored data or provider operations; inspect the result
before retrying an uncertain operation. This behavior applies to admin, recipe and
remote-management callers using the default manager.

`IndexProfileIdentityValidator` supplies the required-name, length and uniqueness
checks shared by the index editor and default profile handler. The editor maps these
errors to its field prefixes; recipe and API callers receive the same domain checks.
Provider index-name uniqueness is enforced for all registered providers, not only Lucene.

### Indexing batch failures

The shared background indexer reads provider state and its cursor under the per-index
lock, and keeps an index's cursor before a failed batch. A
failed document handler, provider rejection, or cursor update stops that index for
the current run; other indexes may continue. A subsequent run retries from its last
saved cursor, so provider mutations in an incomplete batch may be repeated. A batch
read/preparation failure stops the run rather than skipping ahead.

Records successfully excluded by an index's selection still advance its cursor.
They do not require a provider write. These progress rules do not make a scheduled
synchronization request proof that indexing has completed.

### Coordinating lifecycle execution

`IIndexLifecycleService.ExecuteAsync` executes synchronization, reset or rebuild
through the registered indexing source. The worker acquires one per-index lock for
preparation and processing, subject to its 15-minute lease. Reset replays tasks without recreating the provider
index; rebuild recreates it before resetting and replaying. A provider rejection or
required reset-handler failure prevents subsequent processing.

This service executes work directly and returns an `IndexProcessingResult`. Callers
that need asynchronous HTTP operation tracking must schedule and track that work
separately. A completed result describes the queue observed during that run; later
content changes still require indexing.

After successful processing, the coordinator invokes the registered
`IIndexProfileHandler.SynchronizedAsync` callbacks with
`IndexProfileSynchronizedContext.IsIndexingCompleted` set to `true`. Extension
handlers still perform their synchronization work. The built-in content handler
uses this flag to avoid indexing the same queue again. Legacy synchronization
contexts default to `false` and retain the built-in processing behavior. Callback
exceptions prevent the tracked operation from being recorded as completed.

Custom sources without a keyed `NamedIndexingService` retain their legacy
synchronization-handler path for admin and recipe operations. Reset/rebuild prepare
the profile through the same coordinator, then release its preparation lock before
invoking handlers, which may manage their own locks or schedule further work.
These callbacks receive `IsIndexingCompleted = false`. Since legacy handlers return
no processing outcome, the coordinator returns `Unverified` and the operation is
recorded as `Uncertain`, never as completed. Register a keyed processor for the
source type to provide directly observed processing outcomes. This compatibility
path does not enable an unverified source/provider for remote lifecycle requests.

The lock API does not renew leases. The worker measures elapsed time with a
monotonic clock, checks the lease before starting further work or writes, and
reports `LockExpired` if the lease elapsed. The operation becomes `Uncertain`.
A provider call already in flight cannot be cancelled by this check and may have
changed the index; the server does not claim continued exclusive ownership or
successful completion. Inspect the provider before requesting new work. The last
reported cursor is the value confirmed before expiry, which may differ from a
provider write that finished after expiry.

### Persisting lifecycle state

`IndexOperationStore` stores lifecycle records in the tenant database with opaque
operation identifiers. Status writes use independent transactions so they can be
retained separately from indexing work. Expected-state transitions prevent a
terminal record from being restarted, and a completed state requires a completed
processing result for the same index. Records contain timestamps and confirmed
progress rather than provider exception details.

`IndexOperationRunner` records requests before scheduling post-request work. It
claims each pending operation once and records completion after the execution scope
returns. Exceptions are logged on the server and produce failed operation status.

After 30 minutes without a state transition, observed pending/running work becomes
`Uncertain`. This does not cancel work or prove it stopped. The original execution
can still record its eventual result, but uncertain operations are not automatically
restarted. Inspect the index before requesting new work after an uncertain outcome.

### Remote lifecycle requests

Remote lifecycle commands require API authentication, `AccessRemoteManagement` and
`ManageIndexes`. Discover the `lifecycleActions` on each source from
`pomi indexes providers list`. Lucene content indexes enable the current contract;
unverified provider/source pairs return HTTP 501.

```bash
pomi indexes synchronize INDEX_ID
pomi indexes reset INDEX_ID --force
pomi indexes rebuild INDEX_ID --force
pomi indexes operations show OPERATION_ID
```

Requests use `POST api/indexes/by-id:synchronize`, `:reset` or `:rebuild`, with the
index identifier in the `id` query parameter. HTTP 202 returns the operation record
and a Location header for `GET api/indexes/operations/by-id?id=OPERATION_ID`.
Acceptance is not completion. Observe `state` until `Completed`, `Failed` or
`Uncertain`, and inspect `outcome` for details such as provider contention. State and
action values are serialized enum names. Reset/rebuild confirmation is local to
Pomi and does not override server failures.

Existing admin single/bulk actions and reset/rebuild recipes queue the same
persisted operations. Their acknowledgement means work was queued, not completed.
