# Elasticsearch (`OrchardCore.Search.Elasticsearch`)

The Elasticsearch module allows you to manage Elasticsearch indices.

## How to use

You can use an Elasticsearch cloud service like offered on https://www.elastic.co or install it on premise. For development, testing purpose, it is also available to be deployed with Docker.

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

Elasticsearch indices can be created during recipe execution using the `elasticsearch-index` step.  
Here is a sample step:

```json
{
  "name": "elasticsearch-index",
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
      "CloudId": "Orchard_Core_deployment:ZWFzdHVzMi5henVyZS5lbGFzdGljLWNsb3VkLmNvbTo0NDMkNmMxZGQ4YzBrQ2Y2NDI5ZDkyNzc1MTUxN2IyYjZkYTgkMTJmMjA1MzBlOTU0NDgyNDlkZWVmZWYzNmZlY2Q5Yjc="
      "Username": "admin",
      "Password": "admin",
      "CertificateFingerprint": "75:21:E7:92:8F:D5:7A:27:06:38:8E:A4:35:FE:F5:17:D7:37:F4:DF:F0:9A:D2:C0:C4:B6:FF:EE:D1:EA:2B:A7",
      "EnableApiVersioningHeader": false
    }
```

The connection types documentation and examples can be found at this url : 

https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/connection-pooling.html

## Elasticsearch vs Lucene

Both modules are complementary and can be enabled at the same time.
While the Lucene module uses Lucene.NET it is not as feature complete as the Elasticsearch module.

There will be discrepancies between both modules' implementation because of the fact that Lucene.NET implements an older version of Lucene. Though the most basic types of Queries will work with both.

The Lucene module though will always only return `stored` fields from Lucene Queries while the Elasticsearch module can be set to return specific Fields or return the entire source data.

Here is one example of a Query that will return only specific fields from Elasticsearch.

```
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

The Elasticsearch index settings allows to store the "source" data or not. It is set to store the source data in the index settings by default.

Elasticsearch will do an automatic mapping based on CLR Types. Every data field that is passed to Elasticsearch that is mapped as a "string" will become `analyzed` and `stored`. For example, the `Content.ContentItem.DisplayText` will result as an `analyzed` field and `Content.ContentItem.DisplayText.keyword` will become a `stored` field so that it can be used as a technical value.

There may be differences between Lucene and Elasticsearch indexed fields. Lucene allows overriding the CLR type mapping by selecting a "Stored" option on a ContentField for example. Elasticsearch, for now, is not affected by the "Stored" or "Analyzed" options on a ContentField index settings. We may allow it eventually by executing manual mapping on the indices. So, this can result in having fields that are analyzed in Lucene and stored in Elasticsearch when using the same Field name in a Query. You then need to adapt your Queries to use the proper type of Queries.

### Stored fields Query types (structured data search):

https://www.elastic.co/guide/en/elasticsearch/reference/current/term-level-queries.html

### Analyzed fields Query types (full-text search): 

https://www.elastic.co/guide/en/elasticsearch/reference/current/full-text-queries.html


