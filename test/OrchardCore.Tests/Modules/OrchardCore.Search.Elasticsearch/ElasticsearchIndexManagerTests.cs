using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using OrchardCore.Elasticsearch;
using OrchardCore.Elasticsearch.Core.Models;
using OrchardCore.Elasticsearch.Core.Services;
using OrchardCore.Elasticsearch.Models;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.Elasticsearch;

public sealed class ElasticsearchIndexManagerTests
{
    [Fact]
    public async Task CreateAsync_SameAnalysisForSeveralIndexes_SendsConfiguredTypes()
    {
        var requestBodies = new List<string>();

        var settings = new ElasticsearchClientSettings(new SingleNodePool(new Uri("http://localhost:9200")), new InMemoryRequestInvoker(responseBody: null, statusCode: 404))
            .DisableDirectStreaming()
            .OnRequestCompleted(details =>
            {
                if (details.HttpMethod == Elastic.Transport.HttpMethod.PUT)
                {
                    requestBodies.Add(Encoding.UTF8.GetString(details.RequestBodyInBytes));
                }
            });

        var elasticsearchOptions = new ElasticsearchOptions();
        elasticsearchOptions.Analyzers.Add("mystop", new JsonObject { ["type"] = "stop" });
        elasticsearchOptions.TokenFilters.Add("mylowercase", new JsonObject { ["type"] = "lowercase" });

        var indexManager = new ElasticsearchIndexManager(
            new ElasticsearchClient(settings),
            Options.Create(elasticsearchOptions),
            [],
            Mock.Of<IClock>(),
            NullLogger<ElasticsearchIndexManager>.Instance,
            Mock.Of<IDistributedLock>());

        foreach (var indexFullName in new[] { "first", "second" })
        {
            var index = new IndexProfile
            {
                IndexFullName = indexFullName,
            };

            index.Put(new ElasticsearchIndexMetadata
            {
                AnalyzerName = "mystop",
                IndexMappings = new ElasticsearchIndexMap(),
            });

            await indexManager.CreateAsync(index);
        }

        Assert.Equal(2, requestBodies.Count);
        Assert.All(requestBodies, body =>
        {
            var analysis = JsonNode.Parse(body)["settings"]["analysis"];

            Assert.Equal("stop", analysis["analyzer"]["mystop"]["type"].GetValue<string>());
            Assert.Equal("lowercase", analysis["filter"]["mylowercase"]["type"].GetValue<string>());
        });
    }
}
