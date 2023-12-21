# Azure Cognitive Search (`OrchardCore.Search.Azure.CognitiveService`)

The Azure Cognitive Search module allows you to manage Azure Cognitive Search indices.

Before enabling the service, you'll need to configure the connection to the server. You can do that by adding the following into `appsettings.json` file

```
{
  "OrchardCore":{
    "OrchardCore_CognitiveSearch_Azure":{
      "Endpoint": "The server host",
      "Credential":{
        "key":"the server key goes here"
      }
    }
  }
}
```

Then navigate to Search > Indices > Azure Cognitive Search to add an index.

![azure-cognitive-search](https://github.com/OrchardCMS/OrchardCore/assets/24724371/15d42a3b-b3ad-48d3-b778-4e2a65953c21)
