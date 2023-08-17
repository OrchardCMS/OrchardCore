# Elasticsearch (`OrchardCore.Search.Elasticsearch`)

The Elasticsearch module allows you to manage Elasticsearch indices.

## How to use

You can use an Elasticsearch cloud service like offered on https://www.elastic.co or install it on-premises. For development and testing purposes, it is also available to be deployed with Docker.

### Install Elasticsearch 7.x with Docker compose

Elasticsearch uses a mmapfs directory by default to store its indices. The default operating system limits on mmap counts is likely to be too low, which may result in out of memory exceptions.

https://www.elastic.co/guide/en/elasticsearch/reference/current/vm-max-map-count.html

For Docker with WSL2, you will need to persist this setting by using a .wslconfig file.

In your Windows `%userprofile%` directory (typically `C:\Users\<username>`) create or edit the file `.wslconfig` with the following:

```
[wsl2]
kernelCommandLine = "sysctl.vm.max_map_count=262144"
```

Then exit any WSL instance, `wsl --shutdown`, and restart.

```cmd
> sysctl vm.max_map_count
vm.max_map_count = 262144
```

Elasticsearch v7.17.5 Docker Compose file : 
[docker-compose.yml](docker-compose.yml)

- Copy this file in a folder named Elasticsearch somewhere safe.
- Open up a Terminal or Command Shell in this folder.
- Execute `docker-compose up` to deploy Elasticsearch containers.

Advice: don't remove this file from its folder if you want to remove all their containers at once later on in Docker desktop.

You should get this result in Docker Desktop app: 

![Elasticsearch docker containers](images/elasticsearch-docker.png)

### Set up Elasticsearch in Orchard Core

- Add Elastic Connection in the shell configuration (OrchardCore.Cms.Web appsettings.json file). [See Elasticsearch Configurations](#elasticsearch-configuration).

- Start an Orchard Core instance with VS Code debugger
- Go to Orchard Core features, Enable Elasticsearch.

## Recipe step

Elasticsearch indices can be created during recipe execution using the `ElasticIndexSettings` step.  
Here is a sample step:

```json
{
  "steps":[
    {
      "name":"ElasticIndexSettings",
      "Indices":[
        {
          "Search":{
            "AnalyzerName":"standardanalyzer",
            "IndexLatest":false,
            "IndexedContentTypes":[
              "Article",
              "BlogPost"
            ]
          }
        }
      ]
    }
  ]
}
```

## Elasticsearch settings recipe step

Here is an example for setting default search settings:

```json
{
  "steps":[
    {
      // Create the search settings.
      "name":"Settings",
      "ElasticSettings":{
        "SearchIndex":"search",
        "DefaultSearchFields":[
          "Content.ContentItem.FullText"
        ],
        "AllowElasticQueryStringQueryInSearch":false,
        "SyncWithLucene":true // Allows to sync content index settings.
      }
    }
  ]
}
```

### Reset Elasticsearch Index Step

This Reset Index Step resets an Elasticsearch index.
Restarts the indexing process from the beginning in order to update current content items.
It doesn't delete existing entries from the index.

```json
{
  "steps":[
    {
      "name":"lucene-index-reset",
      "Indices":[
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
      "name":"lucene-index-reset",
      "IncludeAll":true
    }
  ]
}
```

### Rebuild Elasticsearch Index Step

This Rebuild Index Step rebuilds an Elasticsearch index.
Deletes and recreates the full index content.

```json
{
  "steps":[
    {
      "name":"lucene-index-rebuild",
      "Indices":[
        "IndexName1",
        "IndexName2"
      ]
    }
  ]
}
```

To rebuild all indices:   

```json
{
  "steps":[
    {
      "name":"lucene-index-rebuild",
      "IncludeAll":true
    }
  ]
}
```

## Queries recipe step

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

Executes a query with the specified name and returns the corresponding Elasticsearch documents. Only the stored fields are returned.

Verbs: `POST` and `GET`

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `indexName` | `search` | The name of the index to query. |
| `query` | `{ "query": { "match_all": {} } }` | A JSON object representing the query. |
| `parameters` | `{ size: 3}` | A JSON object representing the parameters of the query. |

## Elasticsearch Queries

The Elasticsearch module provides a management UI and APIs for querying Elasticsearch data using ElasticSearch Queries.
See: https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html

## Elasticsearch configuration

The Elasticsearch module connection configuration can be set globally in the appsettings.json file or per tenant.

```json
"OrchardCore_Elasticsearch": {
  "ConnectionType": "SingleNodeConnectionPool",
  "Url": "http://localhost",
  "Ports": [ 9200 ],
  "CloudId": "Orchard_Core_deployment:ZWFzdHVzMi5henVyZS5lbGFzdGljLWNsb3VkLmNvbTo0NDMkNmMxZGQ4YzBrQ2Y2NDI5ZDkyNzc1MTUxN2IyYjZkYTgkMTJmMjA1MzBlOTU0NDgyNDlkZWVmZWYzNmZlY2Q5Yjc=",
  "Username": "admin",
  "Password": "admin",
  "CertificateFingerprint": "75:21:E7:92:8F:D5:7A:27:06:38:8E:A4:35:FE:F5:17:D7:37:F4:DF:F0:9A:D2:C0:C4:B6:FF:EE:D1:EA:2B:A7",
  "EnableApiVersioningHeader": false,
  "IndexPrefix": "",
  "Analyzers": {
    "standard": {
      "type": "standard"
    }
  }
}
```

!!! note
    When `CloudConnectionPool` connection type is used, `CertificateFingerprint` is not needed.

The connection types documentation and examples can be found at this url: 

https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/connection-pooling.html

## Elasticsearch Analyzers

As of version 1.6, [built-in](https://www.elastic.co/guide/en/elasticsearch/reference/current/analysis-analyzers.html) and custom analyzers are supported. By default, only `standard` analyzer is available. You may update the Elasticsearch configurations to enable any of the built-in and any custom analyzers. For example, to enable the built in `stop` and `standard` analyzers, you may add the following to the [appsettings.json](../../core/Configuration/README.md) file

```json
"OrchardCore_Elasticsearch": {
  "Analyzers": {
    "standard": {
      "type": "standard"
    },
    "stop": {
      "type": "stop"
    }
  }
}
```

At the same time, you may define custom analyzers using the [appsettings.json](../../core/Configuration/README.md) file as well. In the following example, we are enabling the [standard](https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-standard-analyzer.html) analyzer, customizing the [stop](https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-stop-analyzer.html) analyzer and creating a [custom analyzer](https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-custom-analyzer.html) named `english_analyzer`. 

```json
"OrchardCore_Elasticsearch": {
  "Analyzers": {
    "standard": {
      "type": "standard"
    },
    "stop": {
      "type": "stop",
      "stopwords": [
         "a", 
         "the", 
         "and",
         "or" 
       ]
    },
    "english_analyzer": {
      "type": "custom",
      "tokenizer": "standard",
      "filter": [
        "lowercase",
        "stop"
      ],
      "char_filter": [
        "html_strip"
      ]
    }
  }
}
```

## Elasticsearch vs Lucene

Both modules are complementary and can be enabled at the same time.
While the Lucene module uses Lucene.NET it is not as feature complete as the Elasticsearch module.

There will be discrepancies between both modules' implementation because of the fact that Lucene.NET implements an older version of Lucene. Though the most basic types of Queries will work with both.

The Lucene module though will always only return `stored` fields from Lucene Queries while the Elasticsearch module can be set to return specific Fields or return the entire source data.

Here is one example of a Query that will return only specific fields from Elasticsearch.

```json
{
  "query": {
    "match_all": { }
  },
  "fields": [
    "ContentItemId.keyword", "ContentItemVersionId.keyword"
  ],
  "_source": false
}
```

The Elasticsearch index settings allows to store the "source" data or not. It is set to store the source data by default.

Elasticsearch will do an automatic mapping based on CLR Types. Every data field that is passed to Elasticsearch that is mapped as a "string" will become `text` and `keyword`. For example, the `Content.ContentItem.DisplayText` will result as a `text` field and `Content.ContentItem.DisplayText.keyword` will become a `keyword` field so that it can be used as a technical value.

There may be differences between Lucene and Elasticsearch indexed fields. Lucene allows to `store` and set a field as a `keyword` explicitly. Elasticsearch, for now, is not affected by the `stored` or `keyword` options on a ContentField index settings. We may allow it eventually by executing manual mapping on the indices. So, right now, this can result in having fields that are `text` in Lucene and `keyword` in Elasticsearch when using the same Field name in a Query. You then need to adapt your Queries to use the proper type of Queries.

### Indexed vs Stored

When we say that a field is indexed it means that it is parsed by the configured Analyzer that is set on the index (Elasticsearch also allows to pass custom Analyzers on Queries too). 

Though, when a field is stored it can have different contexts.

As an example, Elasticsearch stores the original value passed in the "_source" fields of its index. All the automatically mapped fields are never stored in the index. They are indexed.

Lucene though will currently be able to store the original value passed when the `Store source data` option is set on a specific index setting. Lucene also has `stored` fields by design like the `ContentItemId` of a content item.

The equivalent of a `StringField` that will behave the same way as a `keyword` in Elasticsearch has been added to all ContentFields that are passing "string" values by using the `.keyword` suffix on the field name.

Here is a small table to compare Lucene and Elasticsearch (string) types:

| Lucene | Elasticsearch | Description |  When Stored  | Search Query type |
|--------|---------------|---------------------------|-----------------|------------------|
| StringField | Keyword  | A field that is indexed but not tokenized: the entire value is indexed as a single token     | original value AND indexed | [stored fields](https://www.elastic.co/guide/en/elasticsearch/reference/current/term-level-queries.html) because indexed as a single token.  |
| TextField   | Text     | A field that is indexed and tokenized, without term vectors | original value AND indexed  | [analyzed fields](https://www.elastic.co/guide/en/elasticsearch/reference/current/full-text-queries.html). Also known as full-text search |
| StoredField | stored in _source by mapping configuration | A field containing original value (not analyzed) | original value | [stored fields](https://www.elastic.co/guide/en/elasticsearch/reference/current/term-level-queries.html) |

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/7Mx1Vjsy3Xw" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>