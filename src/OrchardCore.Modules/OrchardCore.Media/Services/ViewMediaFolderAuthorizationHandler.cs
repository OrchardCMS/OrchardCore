using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.FileStorage;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Services;

/// <summary>
/// Checks if the user has related permission to view media in the path resource which is passed from AuthorizationHandler.
/// </summary>
public class ViewMediaFolderAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private const char PathSeparator = '/';

    private static readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentManager _contentManager;
    private readonly IMediaFileStore _fileStore;
    private readonly IUserAssetFolderNameProvider _userAssetFolderNameProvider;
    private readonly MediaOptions _mediaOptions;
    private readonly string _mediaFieldsFolder;
    private readonly string _usersFolder;

    public ViewMediaFolderAuthorizationHandler(
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        IMediaFileStore fileStore,
        IOptions<MediaOptions> options,
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        IContentManager contentManager)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        _fileStore = fileStore;
        _userAssetFolderNameProvider = userAssetFolderNameProvider;
        _contentManager = contentManager;
        _mediaOptions = options.Value;
        _mediaFieldsFolder = EnsureTrailingSlash(attachedMediaFieldFileService.MediaFieldsFolder);
        _usersFolder = EnsureTrailingSlash(_mediaOptions.AssetsUsersFolder);
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded)
        {
            // This handler is not revoking any pre-existing grants.
            return;
        }

        if (requirement.Permission.Name != SecureMediaPermissions.ViewMedia.Name)
        {
            return;
        }

        if (context.Resource is not string path)
        {
            return;
        }

        path = Uri.UnescapeDataString(path);

        path = _fileStore.NormalizePath(path);

        // Permissions are only set for the root and the first folder tier. Only for users and
        // media fields we will check sub folders too.
        var i = path.IndexOf(PathSeparator);
        var folderPath = i >= 0 ? path[..i] : path;
        var directory = await _fileStore.GetDirectoryInfoAsync(folderPath);
        if (directory is null && path.IndexOf(PathSeparator, folderPath.Length) < 0)
        {
            // This could be a new directory, or a new or existing file in the root folder. As we cannot directly determine
            // whether a file is uploaded or a new directory is created, we will check against the list of allowed extensions.
            // If none is matched, we assume a new directory is created, otherwise we will check the root access only.
            // Note: The file path is currently not authorized during upload, only the folder is checked. Therefore checking 
            // the file extensions is not actually required, but let's leave this in case we add an authorization call later.
            if (await _fileStore.GetFileInfoAsync(folderPath) is not null ||
               _mediaOptions.AllowedFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                path = string.Empty;
            }
        }

        if (IsAuthorizedFolder("/", path))
        {
            await AuthorizeAsync(context, requirement, SecureMediaPermissions.ViewRootMedia);

            return;
        }

        if (IsAuthorizedFolder(_mediaFieldsFolder, path) || IsDescendantOfAuthorizedFolder(_mediaFieldsFolder, path))
        {
            await AuthorizeAttachedMediaFieldsFolderAsync(context, requirement, path);

            return;
        }

        if (IsAuthorizedFolder(_usersFolder, path) || IsDescendantOfAuthorizedFolder(_usersFolder, path))
        {
            await AuthorizeUsersFolderAsync(context, requirement, path);

            return;
        }

        // Create a dynamic permission for the folder path. This allows to give access to a specific folders only.
        var template = SecureMediaPermissions.ConvertToDynamicPermission(SecureMediaPermissions.ViewMedia);
        if (template != null)
        {
            var permission = SecureMediaPermissions.CreateDynamicPermission(template, folderPath);
            await AuthorizeAsync(context, requirement, permission);
        }
        else
        {
            // Not a secure file
            context.Succeed(requirement);
        }
    }

    private async Task AuthorizeAttachedMediaFieldsFolderAsync(AuthorizationHandlerContext context, PermissionRequirement requirement, string path)
    {
        var attachedMediaPathParts = path
            .Substring(_mediaFieldsFolder.Length - 1)
            .Split(PathSeparator, 3, StringSplitOptions.RemoveEmptyEntries);

        // Don't allow 'mediafields' directly.
        if (attachedMediaPathParts.Length == 0)
        {
            return;
        }

        if (string.Equals(attachedMediaPathParts[0], "temp", StringComparison.OrdinalIgnoreCase))
        {
            // Authorize per-user temporary files
            var userId = attachedMediaPathParts.Length > 1 ? attachedMediaPathParts[1] : null;
            var userAssetsFolderName = EnsureTrailingSlash(_userAssetFolderNameProvider.GetUserAssetFolderName(context.User));

            if (IsAuthorizedFolder(userAssetsFolderName, userId))
            {
                await AuthorizeAsync(context, requirement, SecureMediaPermissions.ViewOwnMedia);
            }
            else
            {
                await AuthorizeAsync(context, requirement, SecureMediaPermissions.ViewOthersMedia);
            }
        }
        else
        {
            // Authorize by using the content item permission. The user must have access to the content item to allow its media
            // as well.
            var contentItemId = attachedMediaPathParts.Length > 1 ? attachedMediaPathParts[1] : null;
            var contentItem = !string.IsNullOrEmpty(contentItemId) ? await _contentManager.GetAsync(contentItemId) : null;

            // Disallow if content item is not found or allowed
            if (contentItem is not null)
            {
                await AuthorizeAsync(context, requirement, Contents.CommonPermissions.ViewContent, contentItem);
            }
        }
    }

    private async Task AuthorizeUsersFolderAsync(AuthorizationHandlerContext context, PermissionRequirement requirement, string path)
    {
        // We need to allow the _Users folder for own media access too. If someone uploads into this folder, we are screwed.
        Permission permission;
        if (path.IndexOf(PathSeparator) < 0)
        {
            permission = SecureMediaPermissions.ViewOwnMedia;
        }
        else
        {
            permission = SecureMediaPermissions.ViewOthersMedia;

            var userFolderName = _userAssetFolderNameProvider.GetUserAssetFolderName(context.User);
            if (!string.IsNullOrEmpty(userFolderName))
            {
                var userOwnFolder = EnsureTrailingSlash(_fileStore.Combine(_usersFolder, userFolderName));

                if (IsAuthorizedFolder(userOwnFolder, path) || IsDescendantOfAuthorizedFolder(userOwnFolder, path))
                {
                    permission = SecureMediaPermissions.ViewOwnMedia;
                }
            }
        }

        await AuthorizeAsync(context, requirement, permission);
    }

    private async Task AuthorizeAsync(AuthorizationHandlerContext context, PermissionRequirement requirement, Permission permission, object resource = null)
    {
        var authorizationService = _serviceProvider.GetService<IAuthorizationService>();
        if (await authorizationService.AuthorizeAsync(context.User, permission, resource))
        {
            // If anonymous access is also possible, we want to use default browser caching policies.
            // Otherwise we set a marker which causes a different caching policy being used.
            if ((context.User.Identity?.IsAuthenticated ?? false) && !await authorizationService.AuthorizeAsync(_anonymous, permission, resource))
            {
                _httpContextAccessor.HttpContext.MarkAsSecureMediaRequested();
            }

            context.Succeed(requirement);
        }
        else
        {
            // Note: We don't want other authorization handlers to succeed the requirement. This would allow access to the
            // users and attached media field folders, e.g. if the anonymous role has the "ViewMedia" permission set.
            context.Fail(new AuthorizationFailureReason(this, "View media permission not granted"));
        }
    }

    private static bool IsAuthorizedFolder(string authorizedFolder, string childPath)
    {
        // Ensure end trailing slash. childPath is already normalized.
        childPath += PathSeparator;

        return childPath.Equals(authorizedFolder, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDescendantOfAuthorizedFolder(string authorizedFolder, string childPath)
        => childPath.StartsWith(authorizedFolder, StringComparison.OrdinalIgnoreCase);

    private string EnsureTrailingSlash(string path) => _fileStore.NormalizePath(path) + PathSeparator;
}
