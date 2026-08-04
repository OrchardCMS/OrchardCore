using System.Collections.Generic;
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
    private const char PathSeparator = '/';

    private readonly IServiceProvider _serviceProvider;
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
    private readonly IMediaFileStore _fileStore;
    private readonly MediaOptions _mediaOptions;
    private readonly IUserAssetFolderNameProvider _userAssetFolderNameProvider;

    private string _mediaFieldsFolder;
    private string _usersFolder;

    public ManageMediaFolderAuthorizationHandler(IServiceProvider serviceProvider,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        IMediaFileStore fileStore,
        IOptions<MediaOptions> options,
        IUserAssetFolderNameProvider userAssetFolderNameProvider)
    {
        _serviceProvider = serviceProvider;
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
        _fileStore = fileStore;
        _mediaOptions = options.Value;
        _userAssetFolderNameProvider = userAssetFolderNameProvider;
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

        _mediaFieldsFolder = EnsureTrailingSlash(_attachedMediaFieldFileService.MediaFieldsFolder);
        _usersFolder = EnsureTrailingSlash(_mediaOptions.AssetsUsersFolder);

        var path = await ResolveAuthorizedPathAsync(resourcePath);

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

    private async Task<string> ResolveAuthorizedPathAsync(string path)
    {
        path = _fileStore.NormalizePath(Uri.UnescapeDataString(path));

        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var file = await _fileStore.GetFileInfoAsync(path);
        if (file is not null)
        {
            return _fileStore.NormalizePath(file.Path);
        }

        var directory = await _fileStore.GetDirectoryInfoAsync(path);
        if (directory is not null)
        {
            return _fileStore.NormalizePath(directory.Path);
        }

        return await ResolveNonExistingPathAsync(path);
    }

    private async Task<string> ResolveNonExistingPathAsync(string path)
    {
        var segments = path
            .Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return string.Empty;
        }

        for (var i = segments.Length; i >= 0; i--)
        {
            var ancestorPath = string.Join(PathSeparator, segments[..i]);
            var ancestor = await _fileStore.GetDirectoryInfoAsync(ancestorPath);
            if (ancestor is null)
            {
                continue;
            }

            return CollapseSegments(_fileStore.NormalizePath(ancestor.Path), segments[i..]);
        }

        return CollapseSegments(string.Empty, segments);
    }

    private static string CollapseSegments(string basePath, IReadOnlyList<string> extraSegments)
    {
        var resolvedSegments = new List<string>();

        if (!string.IsNullOrEmpty(basePath))
        {
            resolvedSegments.AddRange(basePath.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries));
        }

        foreach (var segment in extraSegments)
        {
            if (string.IsNullOrEmpty(segment) || segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (resolvedSegments.Count > 0)
                {
                    resolvedSegments.RemoveAt(resolvedSegments.Count - 1);
                }

                continue;
            }

            resolvedSegments.Add(segment);
        }

        return string.Join(PathSeparator, resolvedSegments);
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
        => _fileStore.NormalizePath(path).TrimEnd(PathSeparator) + PathSeparator;
}
