using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Security;

namespace OrchardCore.Media.Services;

/// <summary>
/// Checks if the user has related permission to manage the path resource which is passed from AuthorizationHandler.
/// </summary>
public sealed class ManageMediaFolderAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediaFileStore _fileStore;
    private readonly IUserAssetFolderNameProvider _userAssetFolderNameProvider;

    private readonly char _pathSeparator;
    private readonly string _mediaFieldsFolder;
    private readonly string _usersFolder;

    public ManageMediaFolderAuthorizationHandler(IServiceProvider serviceProvider,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        IMediaFileStore fileStore,
        IOptions<MediaOptions> options,
        IUserAssetFolderNameProvider userAssetFolderNameProvider)
    {
        _serviceProvider = serviceProvider;
        _fileStore = fileStore;
        _userAssetFolderNameProvider = userAssetFolderNameProvider;

        _pathSeparator = _fileStore.Combine("a", "b").Contains('/') ? '/' : '\\';
        _mediaFieldsFolder = EnsureTrailingSlash(attachedMediaFieldFileService.MediaFieldsFolder);
        _usersFolder = EnsureTrailingSlash(options.Value.AssetsUsersFolder);
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded)
        {
            // This handler is not revoking any pre-existing grants.
            return;
        }

        if (requirement.Permission.Name != MediaPermissions.ManageMediaFolder.Name)
        {
            return;
        }

        if (context.Resource is not string resourcePath)
        {
            return;
        }

        var path = await _fileStore.ResolveAuthorizedPathAsync(resourcePath);

        var userOwnFolder = EnsureTrailingSlash(
            _fileStore.Combine(_usersFolder, _userAssetFolderNameProvider.GetUserAssetFolderName(context.User)));

        var permission = MediaPermissions.ManageMedia;

        // Handle attached media field folder.
        if (IsAuthorizedFolder(_mediaFieldsFolder, path) || IsDescendantOfAuthorizedFolder(_mediaFieldsFolder, path))
        {
            permission = MediaPermissions.ManageAttachedMediaFieldsFolder;
        }

        if (IsAuthorizedFolder(_usersFolder, path) || IsAuthorizedFolder(userOwnFolder, path) || IsDescendantOfAuthorizedFolder(userOwnFolder, path))
        {
            permission = MediaPermissions.ManageOwnMedia;
        }

        if (IsDescendantOfAuthorizedFolder(_usersFolder, path) && !IsAuthorizedFolder(userOwnFolder, path) && !IsDescendantOfAuthorizedFolder(userOwnFolder, path))
        {
            permission = MediaPermissions.ManageOthersMedia;
        }

        // Lazy load to prevent circular dependencies.
        var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

        if (await authorizationService.AuthorizeAsync(context.User, permission))
        {
            // Check if viewing is allowed for this folder, if secure media is also enabled.
            if (!_serviceProvider.IsSecureMediaEnabled() ||
                await authorizationService.AuthorizeAsync(context.User, MediaPermissions.ViewMedia, (object)path))
            {
                context.Succeed(requirement);
            }
        }
    }

    private bool IsAuthorizedFolder(string authorizedFolder, string childPath)
    {
        // Ensure end trailing slash.
        childPath = EnsureTrailingSlash(childPath);

        return childPath.Equals(authorizedFolder, StringComparison.Ordinal);
    }

    private bool IsDescendantOfAuthorizedFolder(string authorizedFolder, string childPath)
        => _fileStore.NormalizePath(childPath).StartsWith(authorizedFolder, StringComparison.Ordinal);

    private string EnsureTrailingSlash(string path)
        => _fileStore.NormalizePath(path) + _pathSeparator;
}
