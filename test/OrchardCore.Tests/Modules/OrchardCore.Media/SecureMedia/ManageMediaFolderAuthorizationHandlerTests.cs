using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Security;
using OrchardCore.Security.AuthorizationHandlers;
using OrchardCore.Tests.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.SecureMedia;

public class ManageMediaFolderAuthorizationHandlerTests
{
    private const string UsersFolder = "_users";

    [Theory]
    [InlineData("_users/user-folder/../other-user-folder/victim-private.svg", false)]
    [InlineData("_users/user-folder/../user-folder/own-private.svg", true)]
    [InlineData("_users/user-folder/%2e%2e/other-user-folder/victim-private.svg", false)]
    [InlineData("_users/user-folder/%2e%2e/user-folder/own-private.svg", true)]
    public async Task OwnMediaPermissionFollowsResolvedPath(string resource, bool shouldSucceed)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ManageMediaFolder,
            [MediaPermissions.ManageOwnMedia.Name],
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
            MediaPermissions.ManageMediaFolder,
            [MediaPermissions.ManageOthersMedia.Name],
            authenticated: true,
            "_users/user-folder/../other-user-folder/victim-private.svg");

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("_users/user-folder/new-folder/new-file.svg", true)]
    [InlineData("_users/user-folder/../other-user-folder/new-file.svg", false)]
    public async Task OwnMediaPermissionForNonExistingTargetsUsesResolvedAncestor(string resource, bool shouldSucceed)
    {
        // Arrange
        var handler = CreateHandler();
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            MediaPermissions.ManageMediaFolder,
            [MediaPermissions.ManageOwnMedia.Name],
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
            MediaPermissions.ManageMediaFolder,
            [MediaPermissions.ManageOthersMedia.Name],
            authenticated: true,
            "_users/user-folder/../other-user-folder/new-file.svg");

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    private static ManageMediaFolderAuthorizationHandler CreateHandler()
    {
        var defaultHttpContext = new DefaultHttpContext();
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(hca => hca.HttpContext == defaultHttpContext);

        var mockMediaFileStore = new Mock<IMediaFileStore>();

        var fileMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["admin-secret.svg"] = "admin-secret.svg",
            ["_users/other-user-folder/victim-private.svg"] = "_users/other-user-folder/victim-private.svg",
            ["_users/user-folder/../other-user-folder/victim-private.svg"] = "_users/other-user-folder/victim-private.svg",
            ["_users/user-folder/../user-folder/own-private.svg"] = "_users/user-folder/own-private.svg",
            ["_users/user-folder/own-private.svg"] = "_users/user-folder/own-private.svg",
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
            ["_users"] = "_users",
            ["_users/user-folder"] = "_users/user-folder",
            ["_users/other-user-folder"] = "_users/other-user-folder",
            ["mediafields"] = "mediafields",
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
            AllowedFileExtensions = [".png", ".svg"],
        });

        var mockUserAssetFolderNameProvider = new Mock<IUserAssetFolderNameProvider>();
        mockUserAssetFolderNameProvider
            .Setup(afp => afp.GetUserAssetFolderName(It.Is<ClaimsPrincipal>(cp => cp.Identity.AuthenticationType == "Test")))
            .Returns("user-folder");

        var attachedMediaFieldFileService = new AttachedMediaFieldFileService(
            mockMediaFileStore.Object,
            httpContextAccessor,
            mockUserAssetFolderNameProvider.Object);

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

        return new ManageMediaFolderAuthorizationHandler(
            serviceProvider,
            attachedMediaFieldFileService,
            mockMediaFileStore.Object,
            mockMediaOptions.Object,
            mockUserAssetFolderNameProvider.Object);
    }
}
