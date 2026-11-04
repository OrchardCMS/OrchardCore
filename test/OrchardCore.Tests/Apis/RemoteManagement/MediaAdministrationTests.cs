using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.Documents;
using OrchardCore.Media;
using OrchardCore.Media.Core.Processing;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class MediaAdministrationTests
{
    [Fact]
    public async Task RenameCannotOverwriteAnotherProfile_AndCommitsOnce()
    {
        var document = new MediaProfilesDocument();
        var first = new MediaProfile { Width = 100 };
        var second = new MediaProfile { Width = 200 };
        document.MediaProfiles["first"] = first;
        document.MediaProfiles["second"] = second;
        var store = new Mock<IDocumentManager<MediaProfilesDocument>>();
        store.Setup(manager => manager.GetOrCreateMutableAsync(null)).ReturnsAsync(document);
        store.Setup(manager => manager.GetOrCreateImmutableAsync(null)).ReturnsAsync(document);
        var manager = new MediaProfilesManager(store.Object);
        var service = new MediaProfileManagementService(manager, Options.Create(new MediaOptions { UseTokenizedQueryString = true }));
        Assert.True((await service.SaveAsync("second", first, "first")).Conflict);
        Assert.Same(second, document.MediaProfiles["second"]);
        store.Verify(manager => manager.UpdateAsync(document, null), Times.Never);
        var renamed = await service.SaveAsync(" New ", first, "first");
        Assert.True(renamed.Changed);
        Assert.Equal("new", renamed.Name);
        Assert.False(document.MediaProfiles.ContainsKey("first"));
        Assert.Same(first, document.MediaProfiles["new"]);
        store.Verify(manager => manager.UpdateAsync(document, null), Times.Once);
        Assert.False((await service.SaveAsync("new", first)).Changed);
        var commands = await new MediaProfileService(manager).GetMediaProfileCommands("new");
        Assert.Equal("100", commands[MediaCommands.WidthCommand]);
    }

    [Theory]
    [InlineData(-1, 90, "#fff", "width")]
    [InlineData(100, 101, "#fff", "quality")]
    [InlineData(100, 90, "#xyzxyz", "backgroundColor")]
    public async Task InvalidProfilePreservesDocument(int width, int quality, string color, string error)
    {
        var store = new Mock<IDocumentManager<MediaProfilesDocument>>(MockBehavior.Strict);
        var service = new MediaProfileManagementService(new MediaProfilesManager(store.Object),
            Options.Create(new MediaOptions { UseTokenizedQueryString = true }));
        var result = await service.SaveAsync("profile", new MediaProfile { Width = width, Quality = quality, BackgroundColor = color });
        Assert.Contains(error, result.Errors.Keys);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public void UntokenizedImagesRequireSupportedSizes()
    {
        var options = new MediaOptions { UseTokenizedQueryString = false, SupportedSizes = [100, 200] };
        Assert.Contains("width", MediaProfileManagementService.Validate("profile", new MediaProfile { Width = 123 }, options).Keys);
        Assert.Empty(MediaProfileManagementService.Validate("profile", new MediaProfile { Width = 100 }, options));
    }

    [Fact]
    public void TenantUploadRestrictionsCannotExpandHostPolicyOrBypassRestrictedPermission()
    {
        var options = new MediaOptions
        {
            MaxFileSize = 1000,
            AllowedFileExtensions = new HashSet<string>([".png", ".jpg"], StringComparer.OrdinalIgnoreCase),
            RestrictedFileExtensions = new HashSet<string>([".svg"], StringComparer.OrdinalIgnoreCase),
        };
        MediaUploadOptionsConfiguration.Apply(options, new MediaUploadSettings { MaxFileSize = 2000, AllowedFileExtensions = [".PNG", ".svg", ".exe"] });
        Assert.Equal(1000, options.MaxFileSize);
        Assert.Equal([".png"], options.AllowedFileExtensions);
        Assert.Equal([".svg"], options.RestrictedFileExtensions);
        MediaUploadOptionsConfiguration.Apply(options, new MediaUploadSettings { MaxFileSize = 100, AllowedFileExtensions = [] });
        Assert.Equal(100, options.MaxFileSize);
        Assert.Empty(options.AllowedFileExtensions);
        Assert.Empty(options.RestrictedFileExtensions);
    }

    [Fact]
    public async Task PhysicalCachePurgeIsLimitedToItsTenant()
    {
        var root = Path.Combine(Path.GetTempPath(), "media-cache-test-" + Guid.NewGuid().ToString("N"));
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.WebRootPath).Returns(root);
        var first = new PhysicalFileSystemResizedImageCache(environment.Object, new ShellSettings { Name = "First" },
            NullLogger<PhysicalFileSystemResizedImageCache>.Instance);
        var second = new PhysicalFileSystemResizedImageCache(environment.Object, new ShellSettings { Name = "Second" },
            NullLogger<PhysicalFileSystemResizedImageCache>.Instance);
        try
        {
            using var input = new MemoryStream([1, 2, 3]);
            await first.SetAsync("abcdef", input, "image/png", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);
            input.Position = 0;
            await second.SetAsync("abcdef", input, "image/png", TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);
            await first.ClearAsync(TestContext.Current.CancellationToken);
            Assert.Null(await first.GetAsync("abcdef", ".png", TestContext.Current.CancellationToken));
            var retained = await second.GetAsync("abcdef", ".png", TestContext.Current.CancellationToken);
            Assert.NotNull(retained);
            await retained.Value.Content.DisposeAsync();
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    [Fact]
    public async Task CachePurgeReportsUnavailableAndErrorsWithoutTouchingOtherCache()
    {
        var remote = new Mock<IMediaFileStoreCache>();
        remote.Setup(cache => cache.PurgeAsync()).ReturnsAsync(true);
        var resized = new Mock<IResizedImageCache>();
        resized.Setup(cache => cache.ClearAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("test"));
        using var services = new ServiceCollection().AddSingleton(remote.Object).AddSingleton(resized.Object).BuildServiceProvider();
        var service = new MediaCacheManagementService(services);
        Assert.Equal(MediaCachePurgeStatus.Failed, await service.PurgeAsync("remote", TestContext.Current.CancellationToken));
        resized.Verify(cache => cache.ClearAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(MediaCachePurgeStatus.Failed, await service.PurgeAsync("resized", TestContext.Current.CancellationToken));
        remote.Verify(cache => cache.PurgeAsync(), Times.Once);
        using var empty = new ServiceCollection().BuildServiceProvider();
        Assert.Equal(MediaCachePurgeStatus.Unavailable, await new MediaCacheManagementService(empty).PurgeAsync("remote", TestContext.Current.CancellationToken));
        Assert.Equal(MediaCachePurgeStatus.Invalid, await service.PurgeAsync("other", TestContext.Current.CancellationToken));
    }
}
