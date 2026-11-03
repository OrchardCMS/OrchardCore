# Indexes management API

The **Indexing** feature (`OrchardCore.Indexing`) lists the tenant's index profiles and providers, and queues tracked lifecycle operations. It advertises the `indexes` remote-management capability. With **Lucene** (`OrchardCore.Lucene`) and content support enabled, the `indexes-lucene` capability also manages Lucene content index definitions.

These operations contribute the `pomi indexes` commands and MCP tools. For the indexing pipeline and the lifecycle semantics, see the [Indexing](../../modules/Indexing/README.md) and [Lucene](../../modules/Lucene/README.md) modules.

## Authentication and permissions

Every operation requires an `Api` scheme bearer token, **Access remote management API** (`AccessRemoteManagement`) and **Manage Indexes** (`ManageIndexes`). An absent or invalid token returns `401`, a missing permission returns `403`.

## Base route

`/api/indexes`

## Endpoints

| Method | Endpoint | Operation ID | Pomi command | Capability |
| --- | --- | --- | --- | --- |
| `GET` | `/api/indexes` | `ApiListIndexes` | `indexes list` | `indexes` |
| `GET` | `/api/indexes/by-id?id={id}` | `ApiGetIndex` | `indexes show <id>` | `indexes` |
| `GET` | `/api/indexes/providers` | `ApiListIndexProviders` | `indexes providers list` | `indexes` |
| `POST` | `/api/indexes/by-id:synchronize?id={id}` | `ApiSynchronizeIndexOperation` | `indexes synchronize <id>` | `indexes` |
| `POST` | `/api/indexes/by-id:reset?id={id}` | `ApiResetIndexOperation` | `indexes reset <id> --force` | `indexes` |
| `POST` | `/api/indexes/by-id:rebuild?id={id}` | `ApiRebuildIndexOperation` | `indexes rebuild <id> --force` | `indexes` |
| `GET` | `/api/indexes/operations/by-id?id={id}` | `ApiGetIndexOperation` | `indexes operations show <id>` | `indexes` |
| `GET` | `/api/indexes/lucene/analyzers` | `ApiListLuceneIndexAnalyzers` | `indexes lucene analyzers` | `indexes-lucene` |
| `GET` | `/api/indexes/lucene/by-id?id={id}` | `ApiGetLuceneIndexDefinition` | `indexes lucene show <id>` | `indexes-lucene` |
| `POST` | `/api/indexes/lucene` | `ApiCreateLuceneIndexDefinition` | `indexes lucene create` | `indexes-lucene` |
| `PUT` | `/api/indexes/lucene/by-id?id={id}` | `ApiUpdateLuceneIndexDefinition` | `indexes lucene update <id>` | `indexes-lucene` |
| `DELETE` | `/api/indexes/lucene/by-id?id={id}` | `ApiDeleteLuceneIndexDefinition` | `indexes lucene delete <id> --force` | `indexes-lucene` |

All these operations are MCP tools, except `reset`, `rebuild` and Lucene `delete`, for which Pomi asks a confirmation: they need `AllowConfirmationRequiredTools`. See [MCP server](../../modules/RemoteManagement/README.md#mcp-server).

## Index profiles

### List indexes

`GET /api/indexes`

| Location | Name | Type | Required | Description |
| --- | --- | --- | --- | --- |
| Query | `search` | string | No | Filters the profile names, with the comparison rules of the configured store. |
| Query | `skip` | integer | No | Zero-based offset. Default `0`, minimum `0`. |
| Query | `take` | integer | No | Page size. Default `50`, range `1`–`200`. |

Profiles are ordered by name.

```json
{
  "skip": 0,
  "take": 50,
  "totalCount": 1,
  "items": [
    {
      "id": "4x7qk2m9w",
      "name": "Articles",
      "indexName": "articles",
      "providerName": "Lucene",
      "type": "Content",
      "createdUtc": "2026-01-15T10:00:00Z"
    }
  ]
}
```

The response leaves out the properties of the profile, the physical name of the index, and the author and owner. The `id`, `name` and `indexName` are distinct identifiers.

Paging values out of range, or an offset that doesn't fit a 32-bit integer, return `400`. The store reads whole pages, so a `skip` that isn't a multiple of `take` costs two queries, and rows changing between them can shift the results.

### Show an index

`GET /api/indexes/by-id?id=4x7qk2m9w`

`200` returns one profile as shown above. A missing `id` returns `400`, and an absent profile `404`.

### List providers

`GET /api/indexes/providers`

Returns the registered providers and their source types. `lifecycleActions` lists the operations verified for a provider and source pair: it is empty unless the source is the content one, and the provider declares support.

```json
[
  {
    "name": "Lucene",
    "displayName": "Lucene",
    "sources": [
      {
        "lifecycleActions": ["synchronize", "reset", "rebuild"],
        "type": "Content",
        "displayName": "Content in Lucene",
        "description": "Create an Lucene index based on site contents."
      }
    ]
  }
]
```

## Lifecycle operations

`POST /api/indexes/by-id:synchronize?id={id}`, `POST /api/indexes/by-id:reset?id={id}` and `POST /api/indexes/by-id:rebuild?id={id}`

| Action | Effect |
| --- | --- |
| `synchronize` | Processes the tasks after the provider's current cursor. |
| `reset` | Resets the cursor and processes the tasks again, without recreating the provider index. |
| `rebuild` | Recreates the provider index, resets the cursor and processes the tasks again. |

The request has no body. Acceptance isn't completion: the work starts after the response.

| Status | Meaning |
| --- | --- |
| `202` | The operation representation, with a `Location` header for its status. |
| `400` | A missing `id`. |
| `404` | The profile doesn't exist. |
| `501` | The provider and source pair doesn't support these operations. |

Every accepted request creates a new operation with a new identifier: the request isn't idempotent, but `synchronize` converges, as it only processes what remains. Before repeating a `reset` or a `rebuild`, read the status of the first one.

### Show an operation

`GET /api/indexes/operations/by-id?id={operationId}`

```json
{
  "id": "9b2f6c1d8e",
  "indexId": "4x7qk2m9w",
  "action": "Rebuild",
  "state": "Completed",
  "createdUtc": "2026-01-15T10:00:00Z",
  "updatedUtc": "2026-01-15T10:00:42Z",
  "outcome": "Completed",
  "lastTaskId": 512,
  "skippedTasks": 0
}
```

`action`, `state` and `outcome` are serialized enum names.

| Property | Values |
| --- | --- |
| `action` | `Synchronize`, `Reset`, `Rebuild` |
| `state` | `Pending`, `Running`, `Completed`, `Failed`, `Uncertain` |
| `outcome` | `Failed`, `Completed`, `Busy`, `ProviderUnavailable`, `ProviderMissing`, `ProviderRejected`, `NotFound`, `Unsupported`, `Unverified`, `LockExpired`, or null while unknown |

`lastTaskId` is the last confirmed cursor. `skippedTasks` counts the tasks skipped because their document kept failing to build; the logs name these records, see [Indexing batch failures](../../modules/Indexing/README.md#indexing-batch-failures).

After 30 minutes without a transition, pending or running work reads as `Uncertain`. This doesn't cancel the work or prove it stopped, so inspect the index before requesting more. A missing `id` returns `400`, and an unknown operation `404`.

## Lucene content indexes

These operations only apply to profiles of the Lucene provider and the content source. Another profile returns `404`.

### Definition

| Property | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | string | Yes | The unique administrative name, at most 255 characters, without surrounding whitespace. Queries and search settings refer to it. |
| `indexName` | string | Yes | The provider index name, at most 255 characters. A single file name without path separators or reserved characters. It can't change after creation. |
| `indexedContentTypes` | array of strings | Yes | 1 to 200 unique, existing content types. |
| `indexLatest` | boolean | No | Whether to index the latest version, including drafts. Default `false`. |
| `culture` | string | No | The content culture, or `any`, which is the default. At most 100 characters. |
| `analyzerName` | string | No | A registered analyzer. Default `standardanalyzer`. |
| `storeSourceData` | boolean | No | Whether to store the source document. Default `false`. |
| `queryAnalyzerName` | string | No | A registered analyzer for the default query. Default `standardanalyzer`. |
| `allowLuceneQueries` | boolean | No | Whether the default query accepts the Lucene syntax. Default `false`. |
| `defaultVersion` | string | No | The Lucene compatibility version, as an enum name. Default `LUCENE_48`. |
| `defaultSearchFields` | array of strings | No | At most 200 fields. Default `["Content.ContentItem.FullText"]`. |

Unknown properties return `400`.

```json
{
  "name": "Articles",
  "indexName": "articles",
  "indexedContentTypes": ["Article"],
  "indexLatest": false,
  "culture": "any",
  "analyzerName": "standardanalyzer",
  "storeSourceData": false,
  "queryAnalyzerName": "standardanalyzer",
  "allowLuceneQueries": false,
  "defaultVersion": "LUCENE_48",
  "defaultSearchFields": ["Content.ContentItem.FullText"]
}
```

Responses contain the identifier and the definition, without other profile properties:

```json
{
  "id": "4x7qk2m9w",
  "definition": { "name": "Articles", "indexName": "articles" }
}
```

The definition is abbreviated in this example; responses contain every property.

### List analyzers

`GET /api/indexes/lucene/analyzers`

`200` returns an array of registered analyzer names.

### Show a definition

`GET /api/indexes/lucene/by-id?id=4x7qk2m9w`

`200` returns the identifier and the definition. A missing `id` returns `400`.

### Create a definition

`POST /api/indexes/lucene`

Creates the profile and the provider index, and schedules a synchronization without waiting for it. Enable `OrchardCore.Indexing.Worker` for ongoing updates.

| Status | Meaning |
| --- | --- |
| `201` | Created, with a `Location` header. |
| `200` | An equivalent definition already exists under this name, and is returned unchanged. |
| `400` | An invalid definition, with a validation problem keyed by property. |
| `409` | The name holds a different definition, or the provider index name is taken. |
| `503` | The provider is unavailable or rejected the operation. Inspect the outcome before retrying. |

`name` is the stable identifier that makes creation retry-safe.

### Update a definition

`PUT /api/indexes/lucene/by-id?id=4x7qk2m9w`

Replaces the editable values with the complete definition, while keeping unknown extension data. Omitted optional properties get their default. Updating doesn't reindex: queue the appropriate lifecycle operation after changing what affects the indexed data.

An equivalent definition doesn't write anything. A different `indexName` or an invalid definition returns `400`, and an absent profile `404`. Renaming doesn't update the queries and settings that refer to `name`.

### Delete a definition

`DELETE /api/indexes/lucene/by-id?id=4x7qk2m9w`

Removes the provider index and the profile, and returns `204`. An absent profile also returns `204`, so a repeated delete converges. `503` means the provider rejected the operation or the profile couldn't be removed: inspect the index before retrying.

## Errors

Failures use Problem Details, except invalid definitions, which use a validation problem keyed by property.

## Sources

- [Discovery endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Indexing/Endpoints/Management/IndexDiscoveryEndpoints.cs)
- [Lifecycle endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Indexing/Endpoints/Management/IndexLifecycleEndpoints.cs)
- [Lucene endpoints](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore.Modules/OrchardCore.Lucene/Endpoints/Management/LuceneIndexDefinitionEndpoints.cs)
- [Indexing module](../../modules/Indexing/README.md)
- [Lucene module](../../modules/Lucene/README.md)
