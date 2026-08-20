using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Cache;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Security;
using OrchardCore.Security.AuthorizationHandlers;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.SecureMedia;

public class ViewMediaFolderAuthorizationHandlerTests
{
    private const string UsersFolder = "_users";
    private const string MediafieldsFolder = "mediafields";

    // Note: The handler must normalize the path (i.e. remove leading slashes). This is only tested for the root view permission.

    [Theory]
    [InlineData("ViewRootMediaContent", "")]
    [InlineData("ViewRootMediaContent", "/")]
    [InlineData("ViewRootMediaContent", "filename.png")]
    [InlineData("ViewRootMediaContent", "/filename.png")]

    // ViewMediaContent must allow root access as well.
    [InlineData("ViewMediaContent", "")]
    [InlineData("ViewMediaContent", "/")]
    [InlineData("ViewMediaContent", "filename.png")]
    [InlineData("ViewMediaContent", "/filename.png")]

    // ManageMediaFolder must also allow viewing, because it allows to manage all folders.
    [InlineData("ManageMediaFolder", "")]
    [InlineData("ManageMediaFolder", "/")]
    [InlineData("ManageMediaFolder", "filename.png")]
    [InlineData("ManageMediaFolder", "/filename.png")]
    public async Task GrantsRootViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("NotAllowed", "")]
    [InlineData("NotAllowed", "/")]
    [InlineData("NotAllowed", "filename.png")]
    [InlineData("NotAllowed", "/filename.png")]
    [InlineData("NotAllowed", "folder")]
    [InlineData("NotAllowed", "/folder")]
    [InlineData("NotAllowed", "non-existent-folder")]
    [InlineData("NotAllowed", "/non-existent-folder")]
    [InlineData("NotAllowed", "folder/filename.png")]
    [InlineData("NotAllowed", "/folder/filename.png")]
    [InlineData("ViewRootMediaContent", "folder")]
    [InlineData("ViewRootMediaContent", "/folder")]
    [InlineData("ViewRootMediaContent", "non-existent-folder")]
    [InlineData("ViewRootMediaContent", "/non-existent-folder")]
    [InlineData("ViewRootMediaContent", "folder/filename.png")]
    [InlineData("ViewRootMediaContent", "/folder/filename.png")]
    [InlineData("ViewRootMediaContent", "non-existent-folder/filename.png")]
    [InlineData("ViewRootMediaContent", "/non-existent-folder/filename.png")]

    [InlineData("ViewRootMediaContent", UsersFolder)]
    [InlineData("ViewRootMediaContent", "/" + UsersFolder)]
    [InlineData("ViewRootMediaContent", UsersFolder + "/filename.png")]
    [InlineData("ViewRootMediaContent", "/" + UsersFolder + "/filename.png")]

    [InlineData("ViewRootMediaContent", MediafieldsFolder)]
    [InlineData("ViewRootMediaContent", "/" + MediafieldsFolder)]
    [InlineData("ViewRootMediaContent", MediafieldsFolder + "/filename.png")]
    [InlineData("ViewRootMediaContent", "/" + MediafieldsFolder + "/filename.png")]
    public async Task DoesNotGrantRootViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [InlineData("ViewMediaContent", "folder")]
    [InlineData("ViewMediaContent", "folder/filename.png")]

    [InlineData("ViewMediaContent", "otherfolder")]
    [InlineData("ViewMediaContent", "otherfolder/filename.png")]

    [InlineData("ViewMediaContent", "non-existent-folder")]
    [InlineData("ViewMediaContent", "non-existent-folder/filename.png")]

    // ManageMediaFolder must also allow viewing, because it allows to manage all folders
    [InlineData("ManageMediaFolder", "folder")]
    [InlineData("ManageMediaFolder", "folder/filename.png")]

    [InlineData("ManageMediaFolder", "otherfolder")]
    [InlineData("ManageMediaFolder", "otherfolder/filename.png")]

    [InlineData("ManageMediaFolder", "non-existent-folder")]
    [InlineData("ManageMediaFolder", "non-existent-folder/filename.png")]
    public async Task GrantsAllFoldersViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]

    // Users and mediafields folders are not directly allowed by the ViewMediaContent permission.

    [InlineData("ViewMediaContent", UsersFolder)]
    [InlineData("ViewMediaContent", UsersFolder + "/filename.png")]
    [InlineData("ViewMediaContent", MediafieldsFolder)]
    [InlineData("ViewMediaContent", MediafieldsFolder + "/filename.png")]
    public async Task DoesNotGrantSpecialFoldersViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [InlineData("ViewMediaContent_folder", "folder")]
    [InlineData("ViewMediaContent_folder", "folder/filename.png")]
    [InlineData("ViewMediaContent_folder", "/folder")]
    [InlineData("ViewMediaContent_folder", "/folder/filename.png")]
    public async Task GrantsFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("ViewMediaContent_folder", "otherfolder")]
    [InlineData("ViewMediaContent_folder", "otherfolder/filename.png")]
    [InlineData("ViewMediaContent_folder", "/otherfolder")]
    [InlineData("ViewMediaContent_folder", "/otherfolder/filename.png")]

    [InlineData("ViewMediaContent_otherfolder", "folder")]
    [InlineData("ViewMediaContent_otherfolder", "folder/filename.png")]
    [InlineData("ViewMediaContent_otherfolder", "/folder")]
    [InlineData("ViewMediaContent_otherfolder", "/folder/filename.png")]

    [InlineData("ViewMediaContent_folder", "non-existent-folder")]
    [InlineData("ViewMediaContent_folder", "non-existent-folder/filename.png")]
    [InlineData("ViewMediaContent_folder", "/non-existent-folder")]
    [InlineData("ViewMediaContent_folder", "/non-existent-folder/filename.png")]

    [InlineData("ViewMediaContent_folder", UsersFolder)]
    [InlineData("ViewMediaContent_folder", UsersFolder + "/filename.png")]

    [InlineData("ViewMediaContent_folder", MediafieldsFolder)]
    [InlineData("ViewMediaContent_folder", MediafieldsFolder + "/filename.png")]
    public async Task DoesNotGrantFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    // Attached media fields folder is using content permissions, but user permissions for temp folder (tested below).

    [Theory]
    [InlineData("ViewContent", MediafieldsFolder + "/content-type/content-item-id")]
    [InlineData("ViewContent", MediafieldsFolder + "/content-type/content-item-id" + "/filename.png")]
    public async Task GrantsMediafieldsFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("NotAllowed", MediafieldsFolder + "/content_type/content-item-id")]
    [InlineData("NotAllowed", MediafieldsFolder + "/content_type/content-item-id" + "/filename.png")]

    [InlineData("ViewMediaContent_folder", MediafieldsFolder)]
    [InlineData("ViewMediaContent_folder", MediafieldsFolder + "/filename.png")]

    [InlineData("ManageMediaFolder", MediafieldsFolder)]
    [InlineData("ManageMediaFolder", MediafieldsFolder + "/filename.png")]
    public async Task DoesNotGrantMediafieldsFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    // User folders
    // Note: Temp files for attached media fields are also handled like _Users folder. 
    [Theory]
    [InlineData("ViewOwnMediaContent", UsersFolder + "/user-folder/")]
    [InlineData("ViewOwnMediaContent", UsersFolder + "/user-folder/filename.png")]

    [InlineData("ViewOwnMediaContent", MediafieldsFolder + "/temp/user-folder/")]
    [InlineData("ViewOwnMediaContent", MediafieldsFolder + "/temp/user-folder/filename.png")]
    public async Task GrantsOwnUserFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("ViewOwnMediaContent", UsersFolder + "/other-user-folder/")]
    [InlineData("ViewOwnMediaContent", UsersFolder + "/other-user-folder/filename.png")]

    [InlineData("ViewOwnMediaContent", MediafieldsFolder + "/temp/other-user-folder/")]
    [InlineData("ViewOwnMediaContent", MediafieldsFolder + "/temp/other-user-folder/filename.png")]
    public async Task DoesNotGrantOwnUserFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [InlineData("ViewOthersMediaContent", UsersFolder + "/user-folder/")]
    [InlineData("ViewOthersMediaContent", UsersFolder + "/user-folder/filename.png")]

    [InlineData("ViewOthersMediaContent", MediafieldsFolder + "/temp/user-folder/")]
    [InlineData("ViewOthersMediaContent", MediafieldsFolder + "/temp/user-folder/filename.png")]

    [InlineData("ViewOthersMediaContent", UsersFolder + "/other-user-folder/")]
    [InlineData("ViewOthersMediaContent", UsersFolder + "/other-user-folder/filename.png")]

    [InlineData("ViewOthersMediaContent", MediafieldsFolder + "/temp/other-user-folder/")]
    [InlineData("ViewOthersMediaContent", MediafieldsFolder + "/temp/other-user-folder/filename.png")]
    public async Task GrantsOtherUserFolderViewPermission_Default_Succeeds(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(MediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    // Path traversal tests

    [Theory]
    [InlineData(UsersFolder + "/user-folder/../other-user-folder/victim-private.svg", false)]
    [InlineData(UsersFolder + "/user-folder/../user-folder/own-private.svg", true)]
    [InlineData(UsersFolder + "/user-folder/%2e%2e/other-user-folder/victim-private.svg", false)]
    [InlineData(UsersFolder + "/user-folder/%2e%2e/user-folder/own-private.svg", true)]
    public async Task OwnMediaPermissionFollowsResolvedPath(string resource, bool shouldSucceed)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewOwnMediaContent"],
            authenticated: true,
            resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.Equal(shouldSucceed, context.HasSucceeded);
    }

    [Fact]
    public async Task OthersMediaPermissionAllowsResolvedTraversalTarget()
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewOthersMediaContent"],
            authenticated: true,
            "_users/user-folder/../other-user-folder/victim-private.svg");

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData(UsersFolder + "/user-folder/new-folder/new-file.svg", true)]
    [InlineData(UsersFolder + "/user-folder/../other-user-folder/new-file.svg", false)]
    public async Task OwnMediaPermissionForNonExistingTargetsUsesResolvedAncestor(string resource, bool shouldSucceed)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewOwnMediaContent"],
            authenticated: true,
            resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.Equal(shouldSucceed, context.HasSucceeded);
    }

    [Fact]
    public async Task OthersMediaPermissionAllowsNonExistingResolvedTarget()
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewOthersMediaContent"],
            authenticated: true,
            "_users/user-folder/../other-user-folder/new-file.svg");

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("filename.png")]
    public async Task SecureFolderPermissionGrantsRootViewPermission(string resource)
    {
        // Arrange
        var handler = CreateHandler(withSecureMediaPermissions: true);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewMediaContent_folder"],
            authenticated: true,
            resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        // The root must be viewable, otherwise the granted folder cannot be navigated to.
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("otherfolder")]
    [InlineData("otherfolder/filename.png")]
    [InlineData(UsersFolder)]
    [InlineData(MediafieldsFolder)]
    public async Task SecureFolderPermissionDoesNotGrantOtherFolders(string resource)
    {
        // Arrange
        var handler = CreateHandler(withSecureMediaPermissions: true);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewMediaContent_folder"],
            authenticated: true,
            resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    public async Task NoFolderPermissionDoesNotGrantRootViewPermission(string resource)
    {
        // Arrange
        var handler = CreateHandler(withSecureMediaPermissions: true);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ViewMedia,
            ["ViewOwnMediaContent"],
            authenticated: true,
            resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    private static ViewMediaFolderAuthorizationHandler CreateHandler(bool withSecureMediaPermissions = false)
    {
        var defaultHttpContext = new DefaultHttpContext();
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(hca => hca.HttpContext == defaultHttpContext);

        var mockMediaFileStore = new Mock<IMediaFileStore>();

        var fileMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["filename.png"] = "filename.png",
            ["folder/filename.png"] = "folder/filename.png",
            ["otherfolder/filename.png"] = "otherfolder/filename.png",
            ["_users/user-folder/filename.png"] = "_users/user-folder/filename.png",
            ["_users/other-user-folder/filename.png"] = "_users/other-user-folder/filename.png",
            ["_users/other-user-folder/victim-private.svg"] = "_users/other-user-folder/victim-private.svg",
            ["_users/user-folder/own-private.svg"] = "_users/user-folder/own-private.svg",
            // The file store resolves traversal paths to their canonical path.
            ["_users/user-folder/../other-user-folder/victim-private.svg"] = "_users/other-user-folder/victim-private.svg",
            ["_users/user-folder/../user-folder/own-private.svg"] = "_users/user-folder/own-private.svg",
            ["mediafields/temp/user-folder/filename.png"] = "mediafields/temp/user-folder/filename.png",
            ["mediafields/temp/other-user-folder/filename.png"] = "mediafields/temp/other-user-folder/filename.png",
        };

        mockMediaFileStore
            .Setup(fs => fs.GetFileInfoAsync(It.IsAny<string>()))
            .Returns((string path) =>
            {
                if (path != null && fileMap.TryGetValue(path, out var resolvedPath))
                {
                    return Task.FromResult<IFileStoreEntry>(Mock.Of<IFileStoreEntry>(e => e.Path == resolvedPath && e.IsDirectory == false));
                }

                return Task.FromResult<IFileStoreEntry>(null);
            });

        var directoryMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["folder"] = "folder",
            ["otherfolder"] = "otherfolder",
            ["_users"] = "_users",
            ["_users/user-folder"] = "_users/user-folder",
            ["_users/other-user-folder"] = "_users/other-user-folder",
            ["mediafields"] = "mediafields",
            ["mediafields/temp"] = "mediafields/temp",
            ["mediafields/temp/user-folder"] = "mediafields/temp/user-folder",
            ["mediafields/temp/other-user-folder"] = "mediafields/temp/other-user-folder",
        };

        mockMediaFileStore
            .Setup(fs => fs.GetDirectoryInfoAsync(It.IsAny<string>()))
            .Returns((string path) =>
            {
                if (path != null && directoryMap.TryGetValue(path, out var resolvedPath))
                {
                    return Task.FromResult<IFileStoreEntry>(Mock.Of<IFileStoreEntry>(e => e.Path == resolvedPath && e.IsDirectory));
                }

                return Task.FromResult<IFileStoreEntry>(null);
            });

        var mockMediaOptions = new Mock<IOptions<MediaOptions>>();
        mockMediaOptions.Setup(o => o.Value).Returns(new MediaOptions
        {
            AssetsUsersFolder = UsersFolder,
            AllowedFileExtensions = [".png"],
        });

        var mockUserAssetFolderNameProvider = new Mock<IUserAssetFolderNameProvider>();
        mockUserAssetFolderNameProvider.Setup(afp => afp.GetUserAssetFolderName(It.Is<ClaimsPrincipal>(ci => ci.Identity.AuthenticationType == "Test"))).Returns("user-folder");

        var mockContentManager = new Mock<IContentManager>();
        mockContentManager.Setup(cm => cm.GetAsync(It.IsAny<string>(), It.IsAny<VersionOptions>())).ReturnsAsync(Mock.Of<ContentItem>()); // Pretends an existing content item.

        var attachedMediaFieldFileService = new AttachedMediaFieldFileService(
            mockMediaFileStore.Object,
            httpContextAccessor,
            mockUserAssetFolderNameProvider.Object);

        // Create an IAuthorizationService mock that mimics how OC is granting permissions. 
        var mockAuthorizationService = new Mock<IAuthorizationService>();
        mockAuthorizationService
            .Setup(authorizeService => authorizeService.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>(async (user, resource, requirements) =>
            {
                var context = new AuthorizationHandlerContext(requirements, user, resource);
                var permissionGrantingService = new DefaultPermissionGrantingService();
                var handler = new PermissionHandler(permissionGrantingService);

                await handler.HandleAsync(context);

                return new DefaultAuthorizationEvaluator().Evaluate(context);
            });

        var services = new ServiceCollection();
        services.AddTransient(sp => mockAuthorizationService.Object);

        if (withSecureMediaPermissions)
        {
            // The root level folders the dynamic 'ViewMediaContent_{folder}' permissions are created from.
            mockMediaFileStore
                .Setup(fs => fs.GetDirectoryContentAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(ToAsyncEnumerable(directoryMap.Keys
                    .Where(path => !path.Contains('/'))
                    .Select(path => Mock.Of<IFileStoreEntry>(e => e.Path == path && e.Name == path && e.IsDirectory))));

            services.AddScoped<IPermissionProvider>(sp => new SecureMediaPermissions(
                mockMediaOptions.Object,
                mockMediaFileStore.Object,
                new MemoryCache(new MemoryCacheOptions()),
                attachedMediaFieldFileService,
                new Signal()));
        }

        var serviceProvider = services.BuildServiceProvider();

        return new ViewMediaFolderAuthorizationHandler(
            serviceProvider,
            httpContextAccessor,
            attachedMediaFieldFileService,
            mockMediaFileStore.Object,
            mockMediaOptions.Object,
            mockUserAssetFolderNameProvider.Object,
            mockContentManager.Object
        );
    }

    private static async IAsyncEnumerable<IFileStoreEntry> ToAsyncEnumerable(IEnumerable<IFileStoreEntry> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
        }

        await Task.CompletedTask;
    }
}

