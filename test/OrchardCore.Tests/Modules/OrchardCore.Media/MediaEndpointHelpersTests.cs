using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.Services;
using OrchardCore.Security;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaEndpointHelpersTests
{
    [Theory]
    [InlineData("/blog/media/images/a%20b.png", "https://cms.example.com:8443/blog/media/images/a%20b.png?v=version", true)]
    [InlineData("/media/logo.png", "https://cms.example.com:8443/media/logo.png?v=version", true)]
    [InlineData("media/logo.png", "https://cms.example.com:8443/blog/media/logo.png?v=version", true)]
    [InlineData("https://cdn.example.com/assets/a%23b.png?sig=a%2Fb", "https://cdn.example.com/assets/a%23b.png?sig=a%2Fb&v=version", true)]
    [InlineData("//cdn.example.com/assets/logo.png", "https://cdn.example.com/assets/logo.png?v=version", true)]
    [InlineData("/blog/media/logo.png", "/blog/media/logo.png?v=version", false)]
    public void CreateFileResult_MappedUrl_PreservesPathAndResolvesManagementUrl(string mappedUrl, string expectedUrl, bool management)
    {
        const string path = "images/a b.png";
        var store = new Mock<IMediaFileStore>();
        store.Setup(value => value.MapPathToPublicUrl(path)).Returns(mappedUrl);
        var versions = new Mock<IFileVersionProvider>();
        versions.Setup(value => value.AddFileVersionToPath(It.IsAny<PathString>(), mappedUrl))
            .Returns<PathString, string>((_, url) => url + (url.Contains('?') ? "&" : "?") + "v=version");
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("cms.example.com", 8443);
        context.Request.PathBase = "/blog";
        if (management)
        {
            context.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(new CliOperationMetadata(["media", "files"], "show")), null));
        }

        var file = MediaEndpointHelpers.CreateFileResult(
            Mock.Of<IFileStoreEntry>(entry => entry.Path == path && entry.Name == "a b.png" && entry.DirectoryPath == "images"),
            context, new FileExtensionContentTypeProvider(), versions.Object, store.Object);

        Assert.Equal(path, file.FilePath);
        Assert.Equal("images", file.DirectoryPath);
        Assert.Equal(expectedUrl, file.Url);
    }

    [Theory]
    [InlineData("../unauthorized/file.jpg", "file.jpg")]
    [InlineData(@"..\unauthorized\file.jpg", "file.jpg")]
    public void GetFileName_PathSegments_ReturnsOnlyFileName(string path, string expected)
    {
        var mediaFileStore = new Mock<IMediaFileStore>();

        var result = MediaEndpointHelpers.GetFileName(mediaFileStore.Object, path);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRequestedExtensions_RestrictedFieldFilter_DoesNotFallBackToAllExtensions()
    {
        var options = CreateOptions();

        var result = MediaEndpointHelpers.GetRequestedExtensions(
            options,
            ".svg",
            canUploadRestrictedMedia: false);

        Assert.Empty(result);
    }

    [Fact]
    public void GetRequestedExtensions_MixedCaseFilter_IntersectsCaseInsensitively()
    {
        var options = CreateOptions();

        var result = MediaEndpointHelpers.GetRequestedExtensions(
            options,
            ".JPG,.svg",
            canUploadRestrictedMedia: false);

        Assert.Equal(".jpg", Assert.Single(result), ignoreCase: true);
    }

    private static MediaOptions CreateOptions() => new()
    {
        AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".png" },
        RestrictedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".svg" },
    };

    [Fact]
    public async Task PreCacheRemoteMedia_StreamRetrievalFails_DoesNotPropagate()
    {
        var entry = Mock.Of<IFileStoreEntry>(item => item.Path == "file.txt");
        var mediaFileStore = new Mock<IMediaFileStore>();
        var cache = new Mock<IMediaFileStoreCache>();
        mediaFileStore.Setup(store => store.GetFileStreamAsync(entry)).ThrowsAsync(new IOException());

        await MediaEndpointHelpers.PreCacheRemoteMediaAsync(
            entry,
            mediaFileStore.Object,
            cache.Object,
            new DefaultHttpContext(),
            NullLogger.Instance);
    }

    [Fact]
    public async Task PreCacheRemoteMedia_CacheWritingFails_DisposesStreamAndDoesNotPropagate()
    {
        var entry = Mock.Of<IFileStoreEntry>(item => item.Path == "file.txt");
        var stream = new MemoryStream();
        var mediaFileStore = new Mock<IMediaFileStore>();
        var cache = new Mock<IMediaFileStoreCache>();
        mediaFileStore.Setup(store => store.GetFileStreamAsync(entry)).ReturnsAsync(stream);
        cache.Setup(item => item.SetCacheAsync(stream, entry, It.IsAny<CancellationToken>())).ThrowsAsync(new IOException());

        await MediaEndpointHelpers.PreCacheRemoteMediaAsync(
            entry,
            mediaFileStore.Object,
            cache.Object,
            new DefaultHttpContext(),
            NullLogger.Instance);

        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    [Fact]
    public async Task ToDtoAsync_UnauthorizedDescendants_FiltersTree()
    {
        var user = new ClaimsPrincipal();
        var authorizationService = CreateAuthorizationService("Allowed", "Allowed/Visible");
        var root = new DirectoryTreeNode
        {
            Name = string.Empty,
            Path = string.Empty,
            Children =
            [
                new()
                {
                    Name = "Allowed",
                    Path = "Allowed",
                    HasChildren = true,
                    Children =
                    [
                        new() { Name = "Visible", Path = "Allowed/Visible", Children = [] },
                        new() { Name = "Hidden", Path = "Allowed/Hidden", Children = [] },
                    ],
                },
                new() { Name = "Denied", Path = "Denied", Children = [] },
            ],
        };

        var result = await MediaEndpointHelpers.ToDtoAsync(authorizationService, user, root);

        var allowed = Assert.Single(result.Children);
        Assert.Equal("Allowed", allowed.Path);
        Assert.True(allowed.HasChildren);
        Assert.Equal("Allowed/Visible", Assert.Single(allowed.Children).Path);
    }

    [Fact]
    public async Task GetDirectoryFoldersAsync_UnauthorizedFolders_FiltersFoldersAndHasChildren()
    {
        var user = new ClaimsPrincipal();
        var authorizationService = CreateAuthorizationService(
            "Allowed",
            "Allowed/Visible",
            "AllowedWithoutVisibleChildren");
        var mediaFileStore = new Mock<IMediaFileStore>();

        mediaFileStore
            .Setup(store => store.GetDirectoriesAsync(string.Empty))
            .Returns(GetEntries(
                ("Allowed", "Allowed"),
                ("AllowedWithoutVisibleChildren", "AllowedWithoutVisibleChildren"),
                ("Denied", "Denied")));
        mediaFileStore
            .Setup(store => store.GetDirectoriesAsync("Allowed"))
            .Returns(GetEntries(
                ("Visible", "Allowed/Visible"),
                ("Hidden", "Allowed/Hidden")));
        mediaFileStore
            .Setup(store => store.GetDirectoriesAsync("AllowedWithoutVisibleChildren"))
            .Returns(GetEntries(
                ("Hidden", "AllowedWithoutVisibleChildren/Hidden")));

        var result = await MediaEndpointHelpers.GetDirectoryFoldersAsync(
            mediaFileStore.Object,
            authorizationService,
            user,
            string.Empty);

        Assert.Collection(
            result,
            folder =>
            {
                Assert.Equal("Allowed", folder.DirectoryPath);
                Assert.True(folder.HasChildren);
            },
            folder =>
            {
                Assert.Equal("AllowedWithoutVisibleChildren", folder.DirectoryPath);
                Assert.False(folder.HasChildren);
            });
    }

    private static IAuthorizationService CreateAuthorizationService(params string[] allowedPaths)
    {
        var allowed = allowedPaths.ToHashSet(StringComparer.Ordinal);
        var authorizationService = new Mock<IAuthorizationService>();

        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, resource, requirements) =>
            {
                var isManageMediaFolder = requirements
                    .OfType<PermissionRequirement>()
                    .Any(requirement => requirement.Permission.Name == MediaPermissions.ManageMediaFolder.Name);

                return Task.FromResult(
                    isManageMediaFolder &&
                    resource is string path &&
                    allowed.Contains(path)
                        ? AuthorizationResult.Success()
                        : AuthorizationResult.Failed());
            });

        return authorizationService.Object;
    }

    private static async IAsyncEnumerable<IFileStoreEntry> GetEntries(
        params (string Name, string Path)[] entries)
    {
        await Task.Yield();

        foreach (var (name, path) in entries)
        {
            var entry = new Mock<IFileStoreEntry>();
            entry.SetupGet(item => item.Name).Returns(name);
            entry.SetupGet(item => item.Path).Returns(path);
            entry.SetupGet(item => item.IsDirectory).Returns(true);
            yield return entry.Object;
        }
    }
}
