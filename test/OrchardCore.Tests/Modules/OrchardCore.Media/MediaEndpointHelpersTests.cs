using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.Services;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaEndpointHelpersTests
{
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
