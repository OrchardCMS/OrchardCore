# Azure Cognitive Search (`OrchardCore.Search.Azure.CognitiveService`)

The Azure Cognitive Search module allows you to manage Azure Cognitive Search indices.

Before enabling the service, you'll need to configure the connection to the server. You can do that by adding the following into `appsettings.json` file

```
{
  "OrchardCore":{
    "OrchardCore_CognitiveSearch_Azure":{
      "Endpoint":"https://[search service name].search.windows.net",
      "IndexesPrefix":"",
      "Credential":{
        "Key":"the server key goes here"
      }
    }
  }
}
```

Then navigate to `Search` > `Indexing` > `Azure Cognitive Indices` to add an index.

## Search Module (`OrchardCore.Search`)

When the Search module is enabled along with Azure Cognitive Search, you'll be able to use run the frontend site search against your Azure Cognitive Search indices.

To configure the frontend site search settings, navigate to `Search` > `Settings`. On the `Content` tab, change the default search provider to `Azure Cognitive Search`. Then click on the `Azure Cognitive Search` tab select the default search index to use.

![azure-cognitive-search](https://github.com/OrchardCMS/OrchardCore/assets/24724371/15d42a3b-b3ad-48d3-b778-4e2a65953c21)
