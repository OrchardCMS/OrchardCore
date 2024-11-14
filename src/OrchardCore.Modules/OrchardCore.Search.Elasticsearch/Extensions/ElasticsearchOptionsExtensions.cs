using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Search.Elasticsearch;

internal static class ElasticsearchOptionsExtensions
{
    internal static ElasticsearchOptions AddAnalyzers(this ElasticsearchOptions options, IConfigurationSection configuration)
    {
        var jsonNode = configuration.GetSection(nameof(options.Analyzers)).AsJsonNode();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonNode);

        var analyzersObject = JsonObject.Create(jsonElement, new JsonNodeOptions()
        {
            PropertyNameCaseInsensitive = true,
        });

        if (analyzersObject is not null)
        {
            if (jsonNode is JsonObject jAnalyzers)
            {
                foreach (var analyzer in jAnalyzers)
                {
                    if (analyzer.Value is not JsonObject jAnalyzer)
                    {
                        continue;
                    }

                    options.Analyzers.Add(analyzer.Key, jAnalyzer);
                }
            }
        }

        if (options.Analyzers.Count == 0)
        {
            // When no analyzers are configured, we'll define a default analyzer.
            options.Analyzers.Add(ElasticsearchConstants.DefaultAnalyzer, new JsonObject
            {
                ["type"] = ElasticsearchConstants.DefaultAnalyzer,
            });
        }

        return options;
    }

    internal static ElasticsearchOptions AddTokenFilters(this ElasticsearchOptions options, IConfigurationSection configuration)
    {
        var jsonNode = configuration.GetSection(nameof(options.TokenFilters)).AsJsonNode();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonNode);

        var filterObject = JsonObject.Create(jsonElement, new JsonNodeOptions()
        {
            PropertyNameCaseInsensitive = true,
        });

        if (filterObject is not null)
        {
            if (jsonNode is JsonObject jFilters)
            {
                foreach (var filter in jFilters)
                {
                    if (filter.Value is not JsonObject jFilter)
                    {
                        continue;
                    }

                    options.TokenFilters.Add(filter.Key, jFilter);
                }
            }
        }

        return options;
    }

    internal static ElasticsearchOptions AddIndexPrefix(this ElasticsearchOptions options, IConfigurationSection configuration)
    {
        options.IndexPrefix = configuration.GetValue<string>(nameof(options.IndexPrefix));

        return options;
    }
}
