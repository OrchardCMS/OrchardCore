using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticServiceOptionsSetup(IElasticClient elasticClient, ILogger<ElasticServiceOptionsSetup> logger) : IAsyncConfigureOptions<ElasticServiceOptions>
{
    private readonly IElasticClient _elasticClient = elasticClient;
    private readonly ILogger _logger = logger;

    public async ValueTask ConfigureAsync(ElasticServiceOptions options)
    {
        try
        {
            var response = await _elasticClient.PingAsync();

            options.IsServiceVerified = response.IsValid;
        }
        catch (Exception ex)
        {
            options.IsServiceVerified = false;
            _logger.LogError(ex, "Elasticsearch is enabled but not active because the connection failed.");
        }
    }
}
