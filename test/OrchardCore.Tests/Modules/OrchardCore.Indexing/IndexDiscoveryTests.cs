using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Endpoints.Management;
using OrchardCore.Indexing.Models;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexDiscoveryTests
{
    [Theory]
    [InlineData("AccessRemoteManagement")]
    [InlineData("ManageIndexes")]
    public async Task Discovery_DeniedPermission_DoesNotAccessManagerOrOptions(string denied)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var manager = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        var authorization = Authorize(denied);
        Assert.Equal(403, Status(await IndexDiscoveryEndpoints.ListAsync(http, authorization, manager.Object, new())));
        Assert.Equal(403, Status(await IndexDiscoveryEndpoints.GetAsync(http, authorization, manager.Object, "index")));
        Assert.Equal(403, Status(await IndexDiscoveryEndpoints.ProvidersAsync(http, authorization, null, null)));
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task List_UsesBoundedStorePage_AndOmitsPrivateMetadata()
    {
        var profile = new IndexProfile { Id = "id", Name = "Articles", ProviderName = "Lucene", Type = "Contents", IndexName = "articles",
            IndexFullName = "private-physical-name", Author = "private-author", OwnerId = "private-owner", };
        profile.Properties["ConnectionString"] = "private-connection";
        var manager = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        manager.Setup(value => value.PageAsync(2, 10, It.Is<QueryContext>(query => query.Name == "Article" && query.Sorted)))
            .ReturnsAsync(new PageResult<IndexProfile> { Count = 25, Models = [profile] });
        var result = Assert.IsType<Ok<IndexListResponse>>(await IndexDiscoveryEndpoints.ListAsync(new DefaultHttpContext(), Authorize(), manager.Object,
            new() { Skip = 10, Take = 10, Search = "Article" }));
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(10, result.Value.Skip);
        Assert.Equal(10, result.Value.Take);
        Assert.Equal("id", Assert.Single(result.Value.Items).Id);
        var json = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain("private-", json);
        Assert.DoesNotContain("Properties", json);
        manager.Verify(value => value.PageAsync(2, 10, It.IsAny<QueryContext>()), Times.Once);
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task List_SkipBetweenStorePages_ReadsBothAndReturnsTheRequestedSlice()
    {
        var names = Enumerable.Range(0, 10).Select(i => new IndexProfile { Id = "id" + i, Name = "Index" + i }).ToArray();
        var manager = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        manager.Setup(value => value.PageAsync(It.IsAny<int>(), 4, It.IsAny<QueryContext>()))
            .ReturnsAsync((int page, int size, QueryContext _) => new PageResult<IndexProfile> { Count = 10, Models = names.Skip((page - 1) * size).Take(size).ToArray() });

        var result = Assert.IsType<Ok<IndexListResponse>>(await IndexDiscoveryEndpoints.ListAsync(new DefaultHttpContext(), Authorize(), manager.Object,
            new() { Skip = 3, Take = 4 }));

        Assert.Equal(["id3", "id4", "id5", "id6"], result.Value.Items.Select(item => item.Id));
        Assert.Equal(10, result.Value.TotalCount);
        manager.Verify(value => value.PageAsync(1, 4, It.IsAny<QueryContext>()), Times.Once);
        manager.Verify(value => value.PageAsync(2, 4, It.IsAny<QueryContext>()), Times.Once);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 201)]
    [InlineData(int.MaxValue, 200)]
    public async Task List_InvalidPaging_RejectsBeforeReading(int skip, int take)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var manager = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        Assert.Equal(400, Status(await IndexDiscoveryEndpoints.ListAsync(new DefaultHttpContext { RequestServices = services }, Authorize(), manager.Object,
            new() { Skip = skip, Take = take })));
        manager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Providers_UsesRegisteredProviderSourcePairs()
    {
        var options = new IndexingOptions();
        options.AddIndexingSource("Lucene", IndexingConstants.ContentsIndexSource);
        var lifecycle = new IndexLifecycleOptions();
        lifecycle.RemoteProviders.Add("Lucene");
        options.AddIndexingSource("custom", "Documents");
        options.AddIndexingSource("LUCENE", "Media");
        var result = Assert.IsType<Ok<IReadOnlyList<IndexProviderResponse>>>(await IndexDiscoveryEndpoints.ProvidersAsync(
            new DefaultHttpContext(), Authorize(), Options.Create(options), Options.Create(lifecycle)));
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("custom", result.Value[0].Name);
        var lucene = result.Value[1];
        Assert.Equal(new[] { IndexingConstants.ContentsIndexSource, "Media" }, lucene.Sources.Select(source => source.Type));
        Assert.Equal(new[] { "synchronize", "reset", "rebuild" }, lucene.Sources[0].LifecycleActions);
        Assert.Empty(lucene.Sources[1].LifecycleActions);
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
