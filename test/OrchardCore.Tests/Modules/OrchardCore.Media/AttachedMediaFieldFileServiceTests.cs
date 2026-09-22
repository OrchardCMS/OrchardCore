using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Media;
using OrchardCore.Media.Core;
using OrchardCore.Media.Core.Helpers;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class AttachedMediaFieldFileServiceTests
{
    [Fact]
    public async Task HandleFilesOnFieldUpdateAsync_PathOutsideOwnTempAndContentItemFolder_DoesNotMoveFile()
    {
        // An attacker edits a content item they can edit (e.g. as an Author) and supplies a Paths
        // entry that points at another content item's already-attached file, hoping it gets
        // relocated into their own content item's folder (exfiltration) or deleted.
        using var testContext = await AttachedMediaFixture.CreateAsync();

        var victimContentItem = new ContentItem { ContentType = "Page", ContentItemId = "victim" };
        var attackerContentItem = new ContentItem { ContentType = "Page", ContentItemId = "attacker" };

        var service = testContext.CreateService();
        var victimPath = await testContext.CreateAttachedFileAsync(service.GetContentItemFolder(victimContentItem), "secret.pdf");

        var items = new List<EditMediaFieldItemInfo>
        {
            new() { Path = victimPath, IsNew = false, IsRemoved = false },
        };

        await service.HandleFilesOnFieldUpdateAsync(items, attackerContentItem);

        // The path must be left untouched: neither moved into the attacker's folder nor deleted.
        Assert.Equal(victimPath, items[0].Path);
        Assert.NotNull(await testContext.FileStore.GetFileInfoAsync(victimPath));
        Assert.Empty(await testContext.FileStore.GetDirectoryContentAsync(service.GetContentItemFolder(attackerContentItem)).ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleFilesOnFieldUpdateAsync_PathInOwnTempFolder_MovesFileToContentItemFolder()
    {
        using var testContext = await AttachedMediaFixture.CreateAsync();

        var contentItem = new ContentItem { ContentType = "Page", ContentItemId = "owner" };
        var service = testContext.CreateService();

        var tempPath = await testContext.CreateAttachedFileAsync(service.GetMediaFieldsTempSubFolder(), "upload.png");

        var items = new List<EditMediaFieldItemInfo>
        {
            new() { Path = tempPath, IsNew = true, IsRemoved = false },
        };

        await service.HandleFilesOnFieldUpdateAsync(items, contentItem);

        // Legitimate case: the file was in the caller's own temp folder, so it should be moved
        // into the content item's attached-media folder.
        Assert.StartsWith(service.GetContentItemFolder(contentItem), items[0].Path, StringComparison.Ordinal);
        Assert.Null(await testContext.FileStore.GetFileInfoAsync(tempPath));
        Assert.NotNull(await testContext.FileStore.GetFileInfoAsync(items[0].Path));
    }

    [Fact]
    public async Task HandleFilesOnFieldUpdateAsync_RemovedPathOutsideOwnTempFolder_DoesNotDeleteFile()
    {
        using var testContext = await AttachedMediaFixture.CreateAsync();

        var service = testContext.CreateService();
        var victimContentItem = new ContentItem { ContentType = "Page", ContentItemId = "victim" };
        var attackerContentItem = new ContentItem { ContentType = "Page", ContentItemId = "attacker" };

        var victimPath = await testContext.CreateAttachedFileAsync(service.GetContentItemFolder(victimContentItem), "secret.pdf");

        var items = new List<EditMediaFieldItemInfo>
        {
            new() { Path = victimPath, IsNew = true, IsRemoved = true },
        };

        await service.HandleFilesOnFieldUpdateAsync(items, attackerContentItem);

        Assert.NotNull(await testContext.FileStore.GetFileInfoAsync(victimPath));
    }

    [Fact]
    public async Task HandleFilesOnFieldUpdateAsync_RemovedPathInOwnTempFolder_DeletesFile()
    {
        using var testContext = await AttachedMediaFixture.CreateAsync();

        var service = testContext.CreateService();
        var contentItem = new ContentItem { ContentType = "Page", ContentItemId = "owner" };

        var tempPath = await testContext.CreateAttachedFileAsync(service.GetMediaFieldsTempSubFolder(), "upload.png");

        var items = new List<EditMediaFieldItemInfo>
        {
            new() { Path = tempPath, IsNew = true, IsRemoved = true },
        };

        await service.HandleFilesOnFieldUpdateAsync(items, contentItem);

        Assert.Null(await testContext.FileStore.GetFileInfoAsync(tempPath));
    }

    private sealed class AttachedMediaFixture : IDisposable
    {
        private readonly string _root;

        public IMediaFileStore FileStore { get; private init; }

        private AttachedMediaFixture(string root, IMediaFileStore fileStore)
        {
            _root = root;
            FileStore = fileStore;
        }

        public static Task<AttachedMediaFixture> CreateAsync()
        {
            var root = Directory.CreateTempSubdirectory("attached-media-field-tests").FullName;

            var store = new DefaultMediaFileStore(
                new FileSystemStore(root, NullLogger<FileSystemStore>.Instance),
                "/media", "", [], [],
                new FileSizeHelper(Mock.Of<IStringLocalizer<FileSizeHelper>>()),
                NullLogger<DefaultMediaFileStore>.Instance);

            return Task.FromResult(new AttachedMediaFixture(root, store));
        }

        public AttachedMediaFieldFileService CreateService()
        {
            var httpContext = new DefaultHttpContext();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var userAssetFolderNameProvider = new Mock<IUserAssetFolderNameProvider>();
            userAssetFolderNameProvider.Setup(p => p.GetUserAssetFolderName(It.IsAny<ClaimsPrincipal>()))
                .Returns("me");

            return new AttachedMediaFieldFileService(FileStore, httpContextAccessor.Object, userAssetFolderNameProvider.Object);
        }

        public async Task<string> CreateAttachedFileAsync(string folder, string fileName)
        {
            var path = FileStore.Combine(folder, fileName);
            await FileStore.TryCreateDirectoryAsync(folder);
            using var stream = new MemoryStream("content"u8.ToArray());
            return await FileStore.CreateFileFromStreamAsync(path, stream);
        }

        public void Dispose()
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
