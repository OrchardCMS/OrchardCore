# Lucene (`OrchardCore.Lucene`)

The Lucene module allows you to manage Lucene indices.

## Recipe step

Lucene indices can be created during recipe execution using the `lucene-index` step.  
Here is a sample step:

```json
{
  "name": "lucene-index",
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

### `api/lucene/content`

Executes a query with the specified name and returns the corresponding content items.

Verbs: `POST` and `GET`

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query. |
| `query` | `{ "query": { "match_all": {} } }` | A JSON object representing the query. |
| `parameters` | `{ size: 3}` | A JSON object representing the parameters of the query. |

### `api/lucene/documents`

Executes a query with the specified name and returns the corresponding Lucene documents.
Only the stored fields are returned.

Verbs: `POST` and `GET`

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query. |
| `query` | `{ "query": { "match_all": {} } }` | A JSON object representing the query. |
| `parameters` | `{ size: 3}` | A JSON object representing the parameters of the query. |

## Lucene Worker (`OrchardCore.Lucene.Worker`)

This feature creates a background task that will keep the local file system index synchronized with
other instances that could have their own local index.  
It is recommended to use it only if you are running the same tenant on multiple instances (farm) and are using a Lucene file system index.

If you are running on Azure App Services or if you are using Elasticsearch, then you don't need this feature.

## Lucene Queries

The Lucene module provides a management UI and APIs for querying Lucene data using ElasticSearch Queries.
See : https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html

### Query Filters

Query filters are used to retrieve records from Lucene without taking care of the boost values on them. So, it is retrieving records just like a SQL database would do. 

Here is an example of a filtered query : 

```json
{
  "query": {
    "bool": {
      "filter": [
        { "term": { "Content.ContentItem.Published" : "true" }},
        { "wildcard": { "Content.ContentItem.DisplayText" : "Main*" }}
      ]
    }
  }
}
```

With a must query in the bool Query. "finding specific content type(s)"

```json
{
  "query": {
    "bool": {
      "must" : {
          "term" : { "Content.ContentItem.ContentType" : "Menu" }
      },
      "filter": [
        { "term": { "Content.ContentItem.Published" : "true" }},
        { "wildcard": { "Content.ContentItem.DisplayText" : "Main*" }}
      ]
    }
  }
}
```

As you can see it allows to filter on multiple query types. All of the Query types that are available in Lucene or also filters.

So you can use : 

`fuzzy`  
`match`  
`match_phrase`  
`match_all`  
`prefix`  
`range`  
`term`  
`terms`  
`wildcard`
`geo_distance`  
`geo_bounding_box`  

See ElasticSearch documentation for more details : 
https://www.elastic.co/guide/en/elasticsearch/reference/current/query-filter-context.html