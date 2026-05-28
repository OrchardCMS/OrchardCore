using Azure.Search.Documents.Indexes.Models;
using OrchardCore.AzureAI;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.AzureAI;

public class VectorSearchIndexTests
{
    [Fact]
    public void GetSearchIndex_CreatesDefaultProfileAndAlgorithm_ForVectorField()
    {
        var indexProfile = CreateIndexProfile();
        var metadata = indexProfile.GetOrCreate<AzureAISearchIndexMetadata>();

        metadata.IndexMappings.Add(new AzureAISearchIndexMap("embedding", DocumentIndex.Types.Vector)
        {
            IndexingKey = "embedding",
            VectorInfo = new AzureAISearchIndexMapVectorInfo
            {
                Dimensions = 1536,
                VectorSearchConfiguration = "embedding-profile",
            },
        });

        indexProfile.Put(metadata);

        var searchIndex = GetSearchIndex(indexProfile);

        Assert.NotNull(searchIndex.VectorSearch);
        Assert.Single(searchIndex.VectorSearch.Algorithms);
        Assert.Single(searchIndex.VectorSearch.Profiles);
        Assert.Equal("embedding-profile-algorithm", searchIndex.VectorSearch.Algorithms[0].Name);
        Assert.Equal("embedding-profile", searchIndex.VectorSearch.Profiles[0].Name);
        Assert.Equal("embedding-profile-algorithm", searchIndex.VectorSearch.Profiles[0].AlgorithmConfigurationName);
    }

    [Fact]
    public void GetSearchIndex_UsesConfiguredVectorSearchMetadata()
    {
        var indexProfile = CreateIndexProfile();
        var metadata = indexProfile.GetOrCreate<AzureAISearchIndexMetadata>();
        metadata.VectorSearchMappings = new VectorSearchMappings();

        metadata.IndexMappings.Add(new AzureAISearchIndexMap("embedding", DocumentIndex.Types.Vector)
        {
            IndexingKey = "embedding",
            VectorInfo = new AzureAISearchIndexMapVectorInfo
            {
                Dimensions = 1536,
                VectorSearchConfiguration = "semantic-profile",
            },
        });

        metadata.VectorSearchMappings.Algorithms.Add(new VectorSearchAlgorithmMap
        {
            Name = "semantic-algorithm",
            Kind = "exhaustiveKnn",
            ExhaustiveKnnParametersMap = new ExhaustiveKnnParametersMap
            {
                Metric = "cosine",
            },
        });

        metadata.VectorSearchMappings.Profiles.Add(new VectorSearchProfileMap
        {
            Name = "semantic-profile",
            AlgorithmConfigurationName = "semantic-algorithm",
        });

        indexProfile.Put(metadata);

        var searchIndex = GetSearchIndex(indexProfile);

        var algorithm = Assert.Single(searchIndex.VectorSearch.Algorithms);
        var profile = Assert.Single(searchIndex.VectorSearch.Profiles);

        Assert.IsType<ExhaustiveKnnAlgorithmConfiguration>(algorithm);
        Assert.Equal("semantic-algorithm", algorithm.Name);
        Assert.Equal("semantic-profile", profile.Name);
        Assert.Equal("semantic-algorithm", profile.AlgorithmConfigurationName);
    }

    [Fact]
    public void GetSearchIndex_UsesConfiguredProfileAsFallback_WhenVectorFieldOmitsProfile()
    {
        var indexProfile = CreateIndexProfile();
        var metadata = indexProfile.GetOrCreate<AzureAISearchIndexMetadata>();
        metadata.VectorSearchMappings = new VectorSearchMappings();

        metadata.IndexMappings.Add(new AzureAISearchIndexMap("embedding", DocumentIndex.Types.Vector)
        {
            IndexingKey = "embedding",
            VectorInfo = new AzureAISearchIndexMapVectorInfo
            {
                Dimensions = 1536,
            },
        });

        metadata.VectorSearchMappings.Profiles.Add(new global::OrchardCore.AzureAI.Models.VectorSearchProfileMap
        {
            Name = "semantic-profile",
        });

        indexProfile.Put(metadata);

        var searchIndex = GetSearchIndex(indexProfile);

        var profile = Assert.Single(searchIndex.VectorSearch.Profiles);
        var field = Assert.Single(searchIndex.Fields, x => x.Name == "embedding");

        Assert.Equal("semantic-profile", profile.Name);
        Assert.Equal("semantic-profile", field.VectorSearchProfileName);
    }

    private static SearchIndex GetSearchIndex(IndexProfile indexProfile)
    {
        var method = typeof(AzureAISearchIndexManager).GetMethod("GetSearchIndex", BindingFlags.NonPublic | BindingFlags.Static);

        return Assert.IsType<SearchIndex>(method.Invoke(null, [indexProfile]));
    }

    private static IndexProfile CreateIndexProfile()
        => new()
        {
            IndexFullName = "test-index",
            ProviderName = AzureAISearchConstants.ProviderName,
            Type = "Content",
        };
}
