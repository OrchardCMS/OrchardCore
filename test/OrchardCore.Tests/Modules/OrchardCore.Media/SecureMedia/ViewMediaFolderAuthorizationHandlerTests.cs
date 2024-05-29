using OrchardCore.ContentManagement;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Security;
using OrchardCore.Security.AuthorizationHandlers;
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
    public async Task GrantsRootViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task DoesNotGrantRootViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task GrantsAllFoldersViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task DoesNotGrantSpecialFoldersViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task GrantsFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task DoesNotGrantFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    // Attached media fields folder is using content permissions, but user permissions for temp folder (tested below).

    [Theory]
    [InlineData("ViewContent", MediafieldsFolder + "/content-type/content-item-id")]
    [InlineData("ViewContent", MediafieldsFolder + "/content-type/content-item-id" + "/filename.png")]
    public async Task GrantsMediafieldsFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task DoesNotGrantMediafieldsFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task GrantsOwnUserFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task DoesNotGrantOwnUserFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

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
    public async Task GrantsOtherUserFolderViewPermission(string permission, string resource)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(SecureMediaPermissions.ViewMedia, [permission], true, resource);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    private static ViewMediaFolderAuthorizationHandler CreateHandler()
    {
        var defaultHttpContext = new DefaultHttpContext();
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(hca => hca.HttpContext == defaultHttpContext);

        var mockMediaFileStore = new Mock<IMediaFileStore>();
        mockMediaFileStore.Setup(fs => fs.GetDirectoryInfoAsync(It.IsAny<string>()));
        mockMediaFileStore.Setup(fs => fs.GetDirectoryInfoAsync(It.Is("folder", StringComparer.Ordinal))).ReturnsAsync(Mock.Of<IFileStoreEntry>(e => e.IsDirectory == true));
        mockMediaFileStore.Setup(fs => fs.GetDirectoryInfoAsync(It.Is("otherfolder", StringComparer.Ordinal))).ReturnsAsync(Mock.Of<IFileStoreEntry>(e => e.IsDirectory == true));
        mockMediaFileStore.Setup(fs => fs.GetFileInfoAsync(It.IsAny<string>()));
        mockMediaFileStore.Setup(fs => fs.GetFileInfoAsync(It.Is("filename.png", StringComparer.Ordinal))).ReturnsAsync(Mock.Of<IFileStoreEntry>(e => e.IsDirectory == false));

        var mockMediaOptions = new Mock<IOptions<MediaOptions>>();
        mockMediaOptions.Setup(o => o.Value).Returns(new MediaOptions
        {
            AssetsUsersFolder = UsersFolder,
            AllowedFileExtensions = [".png"]
        });

        var mockUserAssetFolderNameProvider = new Mock<IUserAssetFolderNameProvider>();
        mockUserAssetFolderNameProvider.Setup(afp => afp.GetUserAssetFolderName(It.Is<ClaimsPrincipal>(ci => ci.Identity.AuthenticationType == "Test"))).Returns("user-folder");

        var mockContentManager = new Mock<IContentManager>();
        mockContentManager.Setup(cm => cm.GetAsync(It.IsAny<string>(), It.IsAny<VersionOptions>())).ReturnsAsync(Mock.Of<ContentItem>()); // Pretends an existing content item.

        var attachedMediaFieldFileService = new AttachedMediaFieldFileService(
            mockMediaFileStore.Object,
            httpContextAccessor,
            mockUserAssetFolderNameProvider.Object,
            NullLogger<AttachedMediaFieldFileService>.Instance);

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
}

