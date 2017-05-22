# Lucene (Orchard.Lucene)

The Lucene allows to manage Lucene indices.

## Recipe step

Lucene indices can be created during recipes using the `lucene-index` step.
Here is a sample step:

```json
{
    "name": "lucene-index",
    "Indices": "Indices": [ "Search" ]
}

```

### Queries recipe step

Here is an example for creating a Lucene query from a Queries recipe step:

```json
{
    "Source": "Lucene",
    "Name": "RecentBlogPosts",
    "Index": "Search",
    "Template": "...", // json encoded query template
    "ReturnContentItems": true
}
```


## Web APIs

### api/lucene/content

Executes a query with the specified name and returns the corresponding content items.

Verbs: **POST** and **GET**

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query |
| `query` | `{ "query": { "match_all": {} } }` | A Json object representing the query |
| `parameters` | `{ size: 3}` | A Json object representing the parameters of the query |

### api/lucene/documents

Executes a query with the specified name and returns the corresponding lucene documents. Only the stored
fields are returned.

Verbs: **POST** and **GET**

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query |
| `query` | `{ "query": { "match_all": {} } }` | A Json object representing the query |
| `parameters` | `{ size: 3}` | A Json object representing the parameters of the query |
