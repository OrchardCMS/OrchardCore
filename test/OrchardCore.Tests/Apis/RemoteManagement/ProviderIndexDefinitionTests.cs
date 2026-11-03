using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.AzureAI;
using OrchardCore.AzureAI.Endpoints.Management;
using OrchardCore.AzureAI.Handlers;
using OrchardCore.AzureAI.Models;
using OrchardCore.Elasticsearch;
using OrchardCore.Elasticsearch.Core.Models;
using OrchardCore.Elasticsearch.Endpoints.Management;
using OrchardCore.Elasticsearch.Models;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class ProviderIndexDefinitionTests
{
    private static readonly ElasticsearchOptions _elasticsearchOptions = CreateElasticsearchOptions();
    private static readonly AzureAISearchDefaultOptions _azureOptions = new() { Analyzers = ["standard.lucene", "fr.lucene"] };

    [Theory]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"]}", true)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[]}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\",\"Article\"]}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\" \"]}", false)]
    [InlineData("{\"Name\":\" name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"]}", false)]
    [InlineData("{\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"]}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexedContentTypes\":[\"Article\"]}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"],\"Culture\":\" \"}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"],\"AnalyzerName\":\"unknown\"}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"],\"QueryAnalyzerName\":\"unknown\"}", false)]
    [InlineData("{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"],\"DefaultSearchFields\":[\"\"]}", false)]
    public void Validate_Definition_AcceptsOnlyValidOnes(string json, bool valid)
    {
        var elasticsearch = ElasticsearchIndexDefinitionEndpoints.Validate(JsonSerializer.Deserialize<ElasticsearchIndexDefinition>(json), _elasticsearchOptions);
        var azure = AzureAISearchIndexDefinitionEndpoints.Validate(JsonSerializer.Deserialize<AzureAISearchIndexDefinition>(json), _azureOptions);

        if (valid)
        {
            Assert.Null(elasticsearch);
            Assert.Null(azure);
        }
        else
        {
            Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(elasticsearch).StatusCode);
            Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(azure).StatusCode);
        }
    }

    [Fact]
    public void Validate_NullDefinition_IsRejected()
    {
        Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(ElasticsearchIndexDefinitionEndpoints.Validate(null, _elasticsearchOptions)).StatusCode);
        Assert.Equal(400, Assert.IsAssignableFrom<IStatusCodeHttpResult>(AzureAISearchIndexDefinitionEndpoints.Validate(null, _azureOptions)).StatusCode);
    }

    [Fact]
    public void Deserialize_PrivateMappingProperty_IsRejected()
    {
        const string json = "{\"Name\":\"name\",\"IndexName\":\"index\",\"IndexedContentTypes\":[\"Article\"],\"IndexMappings\":{}}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ElasticsearchIndexDefinition>(json));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<AzureAISearchIndexDefinition>(json));
    }

    [Fact]
    public void GetAnalyzers_Options_IncludeTheDefaultOneOnce()
    {
        Assert.Equal(["custom", ElasticsearchConstants.DefaultAnalyzer], ElasticsearchIndexDefinitionEndpoints.GetAnalyzers(_elasticsearchOptions));
        Assert.Contains(AzureAISearchDefaultOptions.DefaultAnalyzer, AzureAISearchIndexDefinitionEndpoints.GetAnalyzers(new AzureAISearchDefaultOptions()));
        Assert.Contains("fr.lucene", AzureAISearchIndexDefinitionEndpoints.GetAnalyzers(_azureOptions));
    }

    [Fact]
    public void ApplyProviderSettings_ElasticsearchDefinition_IsDescribedBack()
    {
        var profile = new IndexProfile { Id = "id", Name = "name", IndexName = "index", ProviderName = ElasticsearchConstants.ProviderName };
        profile.Put(new ContentIndexMetadata { IndexLatest = true, IndexedContentTypes = ["Article"], Culture = "fr" });
        profile.Put(new ElasticsearchDefaultQueryMetadata { SearchType = "custom", DefaultQuery = "{}" });
        var definition = new ElasticsearchIndexDefinition
        {
            Name = "name",
            IndexName = "index",
            IndexLatest = true,
            IndexedContentTypes = ["Article"],
            Culture = "fr",
            AnalyzerName = "custom",
            StoreSourceData = false,
            QueryAnalyzerName = "custom",
            DefaultSearchFields = ["Field"],
        };

        ElasticsearchIndexDefinitionEndpoints.ApplyProviderSettings(profile, definition);

        var described = ElasticsearchIndexDefinitionEndpoints.Describe(profile);
        Assert.Equal("id", described.Id);
        Assert.Equal(JsonSerializer.Serialize(definition), JsonSerializer.Serialize(described.Definition));

        // The default query stays managed by the admin page.
        var query = profile.GetOrCreate<ElasticsearchDefaultQueryMetadata>();
        Assert.Equal("custom", query.SearchType);
        Assert.Equal("{}", query.DefaultQuery);
    }

    [Fact]
    public void Describe_ElasticsearchProfileWithoutMetadata_UsesTheDefaults()
    {
        var definition = ElasticsearchIndexDefinitionEndpoints.Describe(new IndexProfile { Name = "name", IndexName = "index" }).Definition;

        Assert.Equal(ElasticsearchConstants.DefaultAnalyzer, definition.AnalyzerName);
        Assert.True(definition.StoreSourceData);
        Assert.Equal("any", definition.Culture);
        Assert.NotEmpty(definition.DefaultSearchFields);
    }

    [Fact]
    public async Task ToData_AzureDefinition_IsReadByTheProfileHandlerAndDescribedBack()
    {
        var profile = new IndexProfile { Id = "id", Name = "name", IndexName = "index", ProviderName = AzureAISearchConstants.ProviderName };
        profile.Put(new ContentIndexMetadata { IndexedContentTypes = ["Article"], Culture = "fr" });
        var definition = new AzureAISearchIndexDefinition
        {
            Name = "name",
            IndexName = "index",
            IndexedContentTypes = ["Article"],
            Culture = "fr",
            AnalyzerName = "fr.lucene",
            QueryAnalyzerName = "fr.lucene",
            DefaultSearchFields = ["Field"],
        };
        var handler = new AzureAISearchIndexProfileHandler(new StringLocalizer<AzureAISearchIndexProfileHandler>(new global::OrchardCore.Localization.NullStringLocalizerFactory()));

        await handler.UpdatingAsync(new UpdatingContext<IndexProfile>(profile, AzureAISearchIndexDefinitionEndpoints.ToData(definition)));

        var described = AzureAISearchIndexDefinitionEndpoints.Describe(profile);
        Assert.Equal(JsonSerializer.Serialize(definition), JsonSerializer.Serialize(described.Definition));
        Assert.Empty(profile.GetOrCreate<AzureAISearchIndexMetadata>().IndexMappings);
    }

    private static ElasticsearchOptions CreateElasticsearchOptions()
    {
        var options = new ElasticsearchOptions();
        options.Analyzers["custom"] = [];

        return options;
    }
}
