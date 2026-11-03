using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using OrchardCore.Indexing;
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

    [Fact]
    public async Task Delete_MissingProfileOrOtherProvider_ReturnsNotFoundWithoutDeleting()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var profiles = new Mock<IIndexProfileManager>();
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        Assert.Equal(404, Status(await ElasticsearchIndexDefinitionEndpoints.DeleteAsync(http, Authorize(), profiles.Object, management.Object, "missing")));
        Assert.Equal(404, Status(await AzureAISearchIndexDefinitionEndpoints.DeleteAsync(http, Authorize(), profiles.Object, management.Object, "missing")));

        profiles.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(new IndexProfile { Id = "id", ProviderName = "Other", Type = "Content" });
        Assert.Equal(404, Status(await ElasticsearchIndexDefinitionEndpoints.DeleteAsync(http, Authorize(), profiles.Object, management.Object, "id")));
        Assert.Equal(404, Status(await AzureAISearchIndexDefinitionEndpoints.DeleteAsync(http, Authorize(), profiles.Object, management.Object, "id")));
        management.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Show_ElasticsearchDefinition_ReturnsItsVersion()
    {
        var profile = ElasticsearchProfile();
        var http = CreateContext();

        await ElasticsearchIndexDefinitionEndpoints.GetAsync(http, Authorize(), Profiles(profile).Object, profile.Id);

        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(ElasticsearchIndexDefinitionEndpoints.Describe(profile)), http.Response.Headers.ETag);
    }

    [Fact]
    public async Task Create_EquivalentElasticsearchDefinition_ReturnsItsVersion()
    {
        var profile = ElasticsearchProfile();
        var profiles = Profiles(profile);
        profiles.Setup(value => value.FindByNameAsync(profile.Name)).ReturnsAsync(profile);
        var http = CreateContext();

        var result = await ElasticsearchIndexDefinitionEndpoints.CreateAsync(http, Authorize(), profiles.Object, Mock.Of<IIndexProfileManagementService>(),
            Options.Create(_elasticsearchOptions), ElasticsearchIndexDefinitionEndpoints.Describe(profile).Definition);

        Assert.Equal(200, Status(result));
        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(ElasticsearchIndexDefinitionEndpoints.Describe(profile)), http.Response.Headers.ETag);
    }

    [Fact]
    public async Task Update_StaleElasticsearchVersion_ChangesNothing()
    {
        var profile = ElasticsearchProfile();
        var profiles = Profiles(profile);
        var definition = ElasticsearchIndexDefinitionEndpoints.Describe(profile).Definition;
        definition.IndexLatest = !definition.IndexLatest;
        var http = CreateContext();
        http.Request.Headers.IfMatch = "\"stale\"";

        var result = await ElasticsearchIndexDefinitionEndpoints.UpdateAsync(http, Authorize(), profiles.Object, Options.Create(_elasticsearchOptions), profile.Id, definition);

        Assert.Equal(412, Status(result));
        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(ElasticsearchIndexDefinitionEndpoints.Describe(profile)), http.Response.Headers.ETag);
        profiles.Verify(value => value.UpdateAsync(It.IsAny<IndexProfile>(), It.IsAny<JsonNode>()), Times.Never);
    }

    [Fact]
    public async Task Update_CurrentElasticsearchVersion_AppliesAndReturnsTheNewVersion()
    {
        var profile = ElasticsearchProfile();
        var profiles = Profiles(profile);
        var definition = ElasticsearchIndexDefinitionEndpoints.Describe(profile).Definition;
        var current = ApiResourceVersionExtensions.ComputeApiETag(ElasticsearchIndexDefinitionEndpoints.Describe(profile));
        definition.StoreSourceData = false;
        var http = CreateContext();
        http.Request.Headers.IfMatch = current;

        var result = await ElasticsearchIndexDefinitionEndpoints.UpdateAsync(http, Authorize(), profiles.Object, Options.Create(_elasticsearchOptions), profile.Id, definition);

        Assert.Equal(200, Status(result));
        Assert.NotEqual(current, http.Response.Headers.ETag.ToString());
        profiles.Verify(value => value.UpdateAsync(profile, It.IsAny<JsonNode>()), Times.Once);
    }

    [Fact]
    public async Task Show_AzureDefinition_ReturnsItsVersion()
    {
        var profile = new IndexProfile { Id = "id", Name = "name", IndexName = "index", ProviderName = AzureAISearchConstants.ProviderName, Type = "Content" };
        var http = CreateContext();

        await AzureAISearchIndexDefinitionEndpoints.GetAsync(http, Authorize(), Profiles(profile).Object, profile.Id);

        Assert.Equal(ApiResourceVersionExtensions.ComputeApiETag(AzureAISearchIndexDefinitionEndpoints.Describe(profile)), http.Response.Headers.ETag);
    }

    private static IndexProfile ElasticsearchProfile()
    {
        var profile = new IndexProfile
        {
            Id = "id",
            Name = "name",
            IndexName = "index",
            ProviderName = ElasticsearchConstants.ProviderName,
            Type = "Content",
        };
        profile.Put(new ContentIndexMetadata { IndexedContentTypes = ["Article"] });

        return profile;
    }

    private static Mock<IIndexProfileManager> Profiles(IndexProfile profile)
    {
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.FindByIdAsync(profile.Id)).ReturnsAsync(profile);

        return profiles;
    }

    private static DefaultHttpContext CreateContext() => new()
    {
        RequestServices = new ServiceCollection()
            .AddSingleton(Mock.Of<IStringLocalizer<ProblemDetailsApiLocalization>>())
            .BuildServiceProvider(),
    };

    private static IAuthorizationService Authorize()
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorization.Object;
    }

    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;

    private static ElasticsearchOptions CreateElasticsearchOptions()
    {
        var options = new ElasticsearchOptions();
        options.Analyzers["custom"] = [];

        return options;
    }
}
