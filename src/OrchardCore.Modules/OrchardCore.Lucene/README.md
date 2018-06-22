# Lucene (OrchardCore.Lucene)

The Lucene module allows to manage Lucene indices.

## Recipe step

Lucene indices can be created during recipe execution using the `lucene-index` step.
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

Executes a query with the specified name and returns the corresponding Lucene documents.
Only the stored fields are returned.

Verbs: **POST** and **GET**

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query |
| `query` | `{ "query": { "match_all": {} } }` | A Json object representing the query |
| `parameters` | `{ size: 3}` | A Json object representing the parameters of the query |

## Lucene Worker (OrchardCore.Lucene.Worker)

This feature creates a background task that will keep the local file system index synchronized with
other instances that could have their own local index. It is recommended to use it only if you are 
running the same tenant on multiple instances (farm) and are using a Lucene file system index.

If you are running on Azure App Services or if you are using Elasticsearch, then you don't need this 
feature.

## CREDITS

### Lucene.net

<http://lucenenet.apache.org/index.html>
Copyright 2013 The Apache Software Foundation  
Licensed under the Apache License, Version 2.0.
