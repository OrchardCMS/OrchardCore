using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.Services;
using OrchardCore.Security;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class MediaItemsListingTests
{
    [Theory]
    [InlineData("", 2, "allowed/nested.png")]
    [InlineData("?skip=1&take=1", 2, "root.jpg")]
    [InlineData("?extensions=.png", 1, "allowed/nested.png")]
    public async Task ListItems_MixedTree_PagesOnlyAccessibleFiles(string query, int totalCount, string firstPath)
    {
        using var result = await InvokeAsync("api/media/items", query);
        Assert.Equal(totalCount, result.RootElement.GetProperty("totalCount").GetInt32());
        var items = result.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.Equal(firstPath, items[0].GetProperty("filePath").GetString());
        Assert.All(items, item =>
        {
            Assert.False(item.GetProperty("isDirectory").GetBoolean());
            Assert.Equal("https://cms.example.com:8443/blog/media/" + item.GetProperty("filePath").GetString(), item.GetProperty("url").GetString());
        });
        Assert.DoesNotContain(items, item => item.GetProperty("filePath").GetString().StartsWith("denied/", StringComparison.Ordinal));
        Assert.Equal(string.IsNullOrEmpty(query) ? 2 : 1, items.Length);
    }

    [Fact]
    public async Task ListFolders_MixedTree_ReturnsOnlyAccessibleFolders()
    {
        using var result = await InvokeAsync("api/media/folders", string.Empty);
        Assert.Equal(1, result.RootElement.GetProperty("totalCount").GetInt32());
        var folder = Assert.Single(result.RootElement.GetProperty("items").EnumerateArray());
        Assert.True(folder.GetProperty("isDirectory").GetBoolean());
        Assert.Equal("allowed", folder.GetProperty("directoryPath").GetString());
    }

    [Fact]
    public async Task LegacyListItems_MixedTree_PreservesFilesAndFolders()
    {
        using var result = await InvokeAsync("api/media/GetAllMediaItems", string.Empty);
        var items = result.RootElement.EnumerateArray().ToArray();
        Assert.Equal(3, items.Length);
        Assert.Single(items, item => item.GetProperty("isDirectory").GetBoolean());
    }

    [Fact]
    public async Task ListFiles_Root_ReturnsPathAndDirectUrl()
    {
        using var result = await InvokeAsync("api/media/files", string.Empty);
        var file = Assert.Single(result.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal("root.jpg", file.GetProperty("filePath").GetString());
        Assert.Equal("https://cms.example.com:8443/blog/media/root.jpg", file.GetProperty("url").GetString());
    }

    [Fact]
    public async Task MoveBatch_Success_ReturnsDestinationPathsAndDirectUrls()
    {
        using var result = await InvokeAsync("api/media/files:move-batch", string.Empty,
            """{"mediaNames":["one.jpg","two.png"],"sourceFolder":"images","targetFolder":"archive"}""");
        Assert.Equal("images", result.RootElement.GetProperty("sourceFolder").GetString());
        Assert.Equal("archive", result.RootElement.GetProperty("targetFolder").GetString());
        var files = result.RootElement.GetProperty("files").EnumerateArray().ToArray();
        Assert.Equal(2, files.Length);
        Assert.Equal("archive/one.jpg", files[0].GetProperty("filePath").GetString());
        Assert.Equal("https://cms.example.com:8443/blog/media/archive/one.jpg", files[0].GetProperty("url").GetString());
        Assert.Equal("archive/two.png", files[1].GetProperty("filePath").GetString());
        Assert.Equal("https://cms.example.com:8443/blog/media/archive/two.png", files[1].GetProperty("url").GetString());
    }

    private static async Task<JsonDocument> InvokeAsync(string route, string query, string requestBody = null)
    {
        var store = new Mock<IMediaFileStore>();
        store.Setup(value => value.GetFilesAsync(string.Empty)).Returns(Entries(Entry("root.jpg")));
        var allowed = Entry("allowed", true);
        var denied = Entry("denied", true);
        store.Setup(value => value.GetDirectoryContentAsync(string.Empty)).Returns(Entries(allowed, denied, Entry("root.jpg")));
        store.Setup(value => value.GetDirectoryContentAsync("allowed")).Returns(Entries(Entry("allowed/nested.png")));
        store.Setup(value => value.GetDirectoryContentAsync("denied")).Returns(Entries(Entry("denied/secret.png")));
        store.Setup(value => value.GetDirectoriesAsync(It.IsAny<string>())).Returns(Entries());
        store.Setup(value => value.GetDirectoriesAsync(string.Empty)).Returns(Entries(allowed, denied));
        store.Setup(value => value.MapPathToPublicUrl(It.IsAny<string>())).Returns<string>(path => "/blog/media/" + path);

        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, resource, requirements) =>
                Task.FromResult(resource is string path && path == "denied"
                    || requirements.OfType<PermissionRequirement>().Any(requirement => requirement.Permission.Name == MediaPermissions.ManageOwnMedia.Name)
                        ? AuthorizationResult.Failed() : AuthorizationResult.Success()));
        var versions = new Mock<IFileVersionProvider>();
        versions.Setup(value => value.AddFileVersionToPath(It.IsAny<PathString>(), It.IsAny<string>()))
            .Returns<PathString, string>((_, path) => path);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLocalization();
        builder.Services.AddSingleton(store.Object);
        builder.Services.AddSingleton(authorization.Object);
        builder.Services.AddSingleton(versions.Object);
        builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
        builder.Services.AddSingleton(Mock.Of<IUserAssetFolderNameProvider>());
        builder.Services.Configure<MediaOptions>(options => options.AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".png" });
        await using var app = builder.Build();
        app.AddGetAllMediaItemsEndpoint().AddGetFoldersEndpoint().AddGetMediaItemsEndpoint().AddMoveMediaListEndpoint();
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Single(value => value.RoutePattern.RawText == route);
        using var body = new MemoryStream();
        var context = new DefaultHttpContext { RequestServices = app.Services };
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.example.com", 8443);
        context.Request.PathBase = "/blog";
        context.Request.Method = requestBody is null ? "GET" : "POST";
        using var input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody ?? string.Empty));
        if (requestBody is not null)
        {
            context.Features.Set(Mock.Of<IHttpRequestBodyDetectionFeature>(feature => feature.CanHaveBody));
            context.Request.Body = input;
            context.Request.ContentType = "application/json";
            context.Request.ContentLength = input.Length;
        }
        context.Request.QueryString = new QueryString(query);
        context.Response.Body = body;
        context.SetEndpoint(endpoint);
        await endpoint.RequestDelegate(context);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        store.Verify(value => value.GetDirectoryContentAsync("denied"), Times.Never);
        body.Position = 0;
        return await JsonDocument.ParseAsync(body, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static IFileStoreEntry Entry(string path, bool directory = false) => Mock.Of<IFileStoreEntry>(entry =>
        entry.Name == Path.GetFileName(path) && entry.Path == path && entry.IsDirectory == directory);

    private static async IAsyncEnumerable<IFileStoreEntry> Entries(params IFileStoreEntry[] entries)
    {
        await Task.Yield();
        foreach (var entry in entries)
        {
            yield return entry;
        }
    }
}
