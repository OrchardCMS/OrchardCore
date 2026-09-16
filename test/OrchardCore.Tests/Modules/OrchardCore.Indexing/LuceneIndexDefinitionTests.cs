using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene.Endpoints.Management;
using OrchardCore.Lucene.Models;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class LuceneIndexDefinitionTests
{
    [Theory]
    [InlineData("ManageIndexes")]
    [InlineData("AccessRemoteManagement")]
    public async Task DeniedPermission_PreventsAllProfileAndProviderAccess(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        var authorization = Authorize(denied);
        Assert.Equal(403, Status(await LuceneIndexDefinitionEndpoints.GetAsync(http, authorization, profiles.Object, "id")));
        Assert.Equal(403, Status(await LuceneIndexDefinitionEndpoints.CreateAsync(http, authorization, profiles.Object, management.Object, Definition())));
        Assert.Equal(403, Status(await LuceneIndexDefinitionEndpoints.UpdateAsync(http, authorization, profiles.Object, "id", Definition())));
        Assert.Equal(403, Status(await LuceneIndexDefinitionEndpoints.DeleteAsync(http, authorization, profiles.Object, management.Object, "id")));
        Assert.Equal(403, Status(await LuceneIndexDefinitionEndpoints.AnalyzersAsync(http, authorization, null)));
        profiles.VerifyNoOtherCalls();
        management.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EquivalentCreateAndUpdate_DoNotWriteOrReschedule()
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        profiles.Setup(value => value.FindByNameAsync("Articles")).ReturnsAsync(profile);
        profiles.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(profile);
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        var definition = LuceneIndexDefinitionEndpoints.Describe(profile).Definition;

        Assert.Equal(200, Status(await LuceneIndexDefinitionEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, management.Object, definition)));
        Assert.Equal(200, Status(await LuceneIndexDefinitionEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, "id", definition)));

        profiles.Verify(value => value.FindByNameAsync("Articles"), Times.Once);
        profiles.Verify(value => value.FindByIdAsync("id"), Times.Once);
        profiles.VerifyNoOtherCalls();
        management.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_UsesCoordinatorAndTenantRelativeLocation()
    {
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.NewAsync("Lucene", "Content", It.IsAny<System.Text.Json.Nodes.JsonNode>())).ReturnsAsync(profile);
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        management.Setup(value => value.CreateAsync(profile)).ReturnsAsync(IndexProfileManagementResult.Success);
        var http = new DefaultHttpContext();
        http.Request.PathBase = "/tenant";

        var result = Assert.IsType<Created<LuceneIndexDefinitionResponse>>(await LuceneIndexDefinitionEndpoints.CreateAsync(http, Authorize(), profiles.Object, management.Object, Definition()));

        Assert.Equal("/tenant/api/indexes/lucene/by-id?id=id", result.Location);
        Assert.Equal("id", result.Value.Id);
        management.Verify(value => value.CreateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task Update_RejectsChangingProviderNameBeforeMutation()
    {
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        profiles.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(Profile());
        var definition = Definition();
        definition.IndexName = "replacement";

        Assert.Equal(400, Status(await LuceneIndexDefinitionEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, "id", definition)));

        profiles.Verify(value => value.FindByIdAsync("id"), Times.Once);
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidDefinition_RejectsBeforeProfileAccess()
    {
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        var definition = Definition();
        definition.IndexedContentTypes = [];
        Assert.Equal(400, Status(await LuceneIndexDefinitionEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, null, definition)));
        Assert.Equal(400, Status(await LuceneIndexDefinitionEndpoints.UpdateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, "id", definition)));
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public void Describe_OmitsPrivateMetadataAndPhysicalIdentity()
    {
        var profile = Profile();
        profile.IndexFullName = "private-physical";
        profile.Author = "private-author";
        profile.Properties["ConnectionString"] = "private-connection";

        var json = JsonSerializer.Serialize(LuceneIndexDefinitionEndpoints.Describe(profile));

        Assert.DoesNotContain("private-", json);
        Assert.DoesNotContain("Properties", json);
    }

    [Fact]
    public async Task MinimalDefinition_UsesDefaultsAndCanBeCreated()
    {
        var definition = JsonSerializer.Deserialize<LuceneIndexDefinition>(
            """{"name":"Articles","indexName":"articles","indexedContentTypes":["Article"]}""", JsonSerializerOptions.Web);
        var profile = Profile();
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.NewAsync("Lucene", "Content", It.IsAny<System.Text.Json.Nodes.JsonNode>())).ReturnsAsync(profile);
        var management = new Mock<IIndexProfileManagementService>();
        management.Setup(value => value.CreateAsync(profile)).ReturnsAsync(IndexProfileManagementResult.Success);

        Assert.Equal(201, Status(await LuceneIndexDefinitionEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, management.Object, definition)));
        Assert.Equal("any", definition.Culture);
        Assert.Equal("standardanalyzer", definition.AnalyzerName);
        Assert.Equal("standardanalyzer", definition.QueryAnalyzerName);
        Assert.NotEmpty(definition.DefaultSearchFields);
    }

    [Theory]
    [InlineData("culture")]
    [InlineData("analyzerName")]
    [InlineData("queryAnalyzerName")]
    [InlineData("defaultSearchFields")]
    public async Task ExplicitNullDefaultedSetting_IsRejectedBeforeReading(string property)
    {
        var data = System.Text.Json.Nodes.JsonNode.Parse("""{"name":"Articles","indexName":"articles","indexedContentTypes":["Article"]}""");
        data[property] = null;
        var definition = data.Deserialize<LuceneIndexDefinition>(JsonSerializerOptions.Web);
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        Assert.Equal(400, Status(await LuceneIndexDefinitionEndpoints.CreateAsync(new DefaultHttpContext(), Authorize(), profiles.Object, null, definition)));
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public void Definition_RejectsArbitraryProperties()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LuceneIndexDefinition>("""{"Properties":{"Secret":"value"}}"""));
    }

    [Fact]
    public async Task Delete_MissingProfileIsIdempotentAndWrongProviderIsNotDeleted()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var profiles = new Mock<IIndexProfileManager>();
        var management = new Mock<IIndexProfileManagementService>(MockBehavior.Strict);
        Assert.Equal(204, Status(await LuceneIndexDefinitionEndpoints.DeleteAsync(http, Authorize(), profiles.Object, management.Object, "missing")));
        var profile = Profile();
        profile.ProviderName = "Other";
        profiles.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(profile);
        Assert.Equal(404, Status(await LuceneIndexDefinitionEndpoints.DeleteAsync(http, Authorize(), profiles.Object, management.Object, "id")));
        management.VerifyNoOtherCalls();
    }

    private static LuceneIndexDefinition Definition() => new() { Name = "Articles", IndexName = "articles", IndexedContentTypes = ["Article"] };

    private static IndexProfile Profile()
    {
        var profile = new IndexProfile { Id = "id", ProviderName = "Lucene", Type = "Content", Name = "Articles", IndexName = "articles" };
        profile.Put(new ContentIndexMetadata { IndexedContentTypes = ["Article"], Culture = "any" });
        profile.Put(new LuceneIndexMetadata());
        profile.Put(new LuceneIndexDefaultQueryMetadata());
        return profile;
    }

    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;

    private static IAuthorizationService Authorize(string denied = null)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().Any(requirement => requirement.Permission.Name == denied)
                    ? AuthorizationResult.Failed() : AuthorizationResult.Success());
        return authorization.Object;
    }
}
