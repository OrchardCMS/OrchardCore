# Lucene (`OrchardCore.Search.Elasticsearch`)

The Elasticsearch module allows you to manage Elasticsearch indices.

## Recipe step

Elasticsearch indices can be created during recipe execution using the `elastic-index` step.  
Here is a sample step:

```json
{
  "name": "elastic-index",
  "Indices": [
    {
      "Search": {
        "AnalyzerName": "standardanalyzer",
        "IndexLatest": false,
        "IndexedContentTypes": [
          "Article",
          "BlogPost"
        ]
      }
    }
  ]
},
```

### Queries recipe step

Here is an example for creating a Elasticsearch query from a Queries recipe step:

```json
{
    "Source": "Elasticsearch",
    "Name": "RecentBlogPosts",
    "Index": "Search",
    "Template": "...", // json encoded query template
    "ReturnContentItems": true
}
```

## Web APIs

### `api/elasticsearch/content`

Executes a query with the specified name and returns the corresponding content items.

Verbs: `POST` and `GET`

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query. |
| `query` | `{ "query": { "match_all": {} } }` | A JSON object representing the query. |
| `parameters` | `{ size: 3}` | A JSON object representing the parameters of the query. |

### `api/elasticsearch/documents`

Executes a query with the specified name and returns the corresponding Elasticsearch documents.
Only the stored fields are returned.

Verbs: `POST` and `GET`

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query. |
| `query` | `{ "query": { "match_all": {} } }` | A JSON object representing the query. |
| `parameters` | `{ size: 3}` | A JSON object representing the parameters of the query. |

## Elasticsearch Queries

The Elasticsearch module provides a management UI and APIs for querying Elasticsearch data using ElasticSearch Queries.
See: https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html
