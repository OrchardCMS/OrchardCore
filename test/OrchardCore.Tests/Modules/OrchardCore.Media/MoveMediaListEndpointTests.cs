using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.ViewModels;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MoveMediaListEndpointTests
{
    private static readonly MethodInfo _handleAsyncMethod = typeof(MoveMediaListEndpoint)
        .GetMethod("HandleAsync", BindingFlags.NonPublic | BindingFlags.Static);

    [Fact]
    public async Task HandleAsync_TargetPathOutsideAuthorizedFolders_DoesNotMoveFile()
    {
        // A user only authorized for their own folder tries to move another user's file into
        // their own folder by supplying a multi-segment "mediaNames" entry that resolves outside
        // both authorized folders. The check on the resolved source/target paths must catch this
        // even though the caller-declared folders ("root" / attacker's own folder) both pass.
        var authorizationService = CreateAuthorizationService(string.Empty, "_users/attacker");
        var mediaFileStore = new Mock<IMediaFileStore>();

        var model = new MoveMedias
        {
            mediaNames = ["_users/victim/photo.jpg"],
            sourceFolder = "root",
            targetFolder = "_users/attacker",
        };

        var result = await InvokeAsync(authorizationService, mediaFileStore, model);

        Assert.IsType<ValidationProblem>(result);
        mediaFileStore.Verify(
            store => store.MoveFileAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_TraversalSegmentInName_DoesNotMoveFile()
    {
        var authorizationService = CreateAuthorizationService("_users/attacker", "_users/attacker/deep");
        var mediaFileStore = new Mock<IMediaFileStore>();

        var model = new MoveMedias
        {
            mediaNames = ["../victim/secret.pdf"],
            sourceFolder = "_users/attacker",
            targetFolder = "_users/attacker/deep",
        };

        var result = await InvokeAsync(authorizationService, mediaFileStore, model);

        Assert.IsType<ValidationProblem>(result);
        mediaFileStore.Verify(
            store => store.MoveFileAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SourceAndTargetWithinAuthorizedFolders_MovesFile()
    {
        var authorizationService = CreateAuthorizationService("_users/owner", "_users/owner/sub");
        var mediaFileStore = new Mock<IMediaFileStore>();
        mediaFileStore.Setup(store => store.MoveFileAsync("_users/owner/photo.jpg", "_users/owner/sub/photo.jpg"))
            .Returns(Task.CompletedTask);

        var model = new MoveMedias
        {
            mediaNames = ["photo.jpg"],
            sourceFolder = "_users/owner",
            targetFolder = "_users/owner/sub",
        };

        var result = await InvokeAsync(authorizationService, mediaFileStore, model);

        Assert.IsType<Ok>(result);
        mediaFileStore.Verify(
            store => store.MoveFileAsync("_users/owner/photo.jpg", "_users/owner/sub/photo.jpg"),
            Times.Once);
    }

    private static async Task<IResult> InvokeAsync(
        IAuthorizationService authorizationService,
        Mock<IMediaFileStore> mediaFileStore,
        MoveMedias model)
    {
        var services = new ServiceCollection();

        var problemLocalizer = new Mock<IStringLocalizer<ProblemDetailsApiLocalization>>();
        problemLocalizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        services.AddSingleton(problemLocalizer.Object);

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity("Test")),
            RequestServices = services.BuildServiceProvider(),
        };

        var localizer = new Mock<IStringLocalizer<MediaApiEndpoints>>();
        localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, string.Format(key, args)));

        var task = (Task<IResult>)_handleAsyncMethod.Invoke(
            null,
            [httpContext, authorizationService, mediaFileStore.Object, localizer.Object, model]);

        return await task;
    }

    private static IAuthorizationService CreateAuthorizationService(params string[] authorizedFolders)
    {
        var authorized = authorizedFolders.ToHashSet(StringComparer.Ordinal);
        var authorizationService = new Mock<IAuthorizationService>();

        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, resource, requirements) =>
            {
                var permissionRequirement = requirements.OfType<PermissionRequirement>().FirstOrDefault();

                if (permissionRequirement is null)
                {
                    return Task.FromResult(AuthorizationResult.Failed());
                }

                if (permissionRequirement.Permission.Name == MediaPermissions.ManageMedia.Name)
                {
                    return Task.FromResult(AuthorizationResult.Success());
                }

                if (permissionRequirement.Permission.Name == MediaPermissions.ManageMediaFolder.Name
                    && resource is string rawPath
                    && authorized.Any(folder =>
                    {
                        // Mirror ManageMediaFolderAuthorizationHandler's real behavior: it resolves
                        // ".." segments against the file store before comparing, so the mock must
                        // do the same instead of a naive string-prefix check (which a raw ".."
                        // segment would otherwise slip past).
                        var path = CollapseDotSegments(rawPath);

                        return string.Equals(path, folder, StringComparison.Ordinal)
                            || path.StartsWith(folder + "/", StringComparison.Ordinal)
                            || (folder.Length == 0 && !path.Contains('/'));
                    }))
                {
                    return Task.FromResult(AuthorizationResult.Success());
                }

                return Task.FromResult(AuthorizationResult.Failed());
            });

        return authorizationService.Object;
    }

    private static string CollapseDotSegments(string path)
    {
        var segments = new List<string>();

        foreach (var segment in path.Split('/'))
        {
            if (segment is "" or ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (segments.Count > 0)
                {
                    segments.RemoveAt(segments.Count - 1);
                }

                continue;
            }

            segments.Add(segment);
        }

        return string.Join('/', segments);
    }
}
