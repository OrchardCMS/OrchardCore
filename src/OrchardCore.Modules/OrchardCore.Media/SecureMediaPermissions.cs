using System.Collections.ObjectModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using OrchardCore.Media.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public sealed class SecureMediaPermissions : IPermissionProvider
{
    // Note: The ManageMediaFolder permission grants all access, so viewing must be implied by it too.
    public static readonly Permission ViewMedia = new("ViewMediaContent", "View media content in all folders", new[] { Permissions.ManageMediaFolder });
    public static readonly Permission ViewRootMedia = new("ViewRootMediaContent", "View media content in the root folder", new[] { ViewMedia });
    public static readonly Permission ViewOthersMedia = new("ViewOthersMediaContent", "View others media content", new[] { Permissions.ManageMediaFolder });
    public static readonly Permission ViewOwnMedia = new("ViewOwnMediaContent", "View own media content", new[] { ViewOthersMedia });

    private static readonly Permission _viewMediaTemplate = new("ViewMediaContent_{0}", "View media content in folder '{0}'", new[] { ViewMedia });

    private static Dictionary<ValueTuple<string, string>, Permission> _permissionsByFolder = new();
    private static readonly char[] _trimSecurePathChars = ['/', '\\', ' '];
    private static readonly ReadOnlyDictionary<string, Permission> _permissionTemplates = new(new Dictionary<string, Permission>()
    {
        { ViewMedia.Name, _viewMediaTemplate },
    });

    private readonly MediaOptions _mediaOptions;
    private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
    private readonly ISignal _signal;
    private readonly IMediaFileStore _fileStore;
    private readonly IMemoryCache _cache;

    public SecureMediaPermissions(
        IOptions<MediaOptions> options,
        IMediaFileStore fileStore,
        IMemoryCache cache,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        ISignal signal)
    {
        _mediaOptions = options.Value;
        _fileStore = fileStore;
        _cache = cache;
        _attachedMediaFieldFileService = attachedMediaFieldFileService;
        _signal = signal;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return await _cache.GetOrCreateAsync(nameof(SecureMediaPermissions), async (entry) =>
        {
            // Ensure to rebuild at least after some time, to detect directory changes from outside of
            // the media module. The signal gets set if a directory is created or deleted in the Media
            // Library directly.
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                 .AddExpirationToken(_signal.GetToken(nameof(SecureMediaPermissions)));

            return await GetPermissionsInternalAsync();
        });
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ViewMedia,
                    ViewOthersMedia
                }
            },
            new PermissionStereotype
            {
                Name = "Authenticated",
                Permissions = new[]
                {
                    ViewOwnMedia
                }
            },
            new PermissionStereotype
            {
                Name = "Anonymous",
                Permissions = new[]
                {
                    ViewMedia
                }
            }
        };
    }

    /// <summary>
    /// Returns a dynamic permission for a secure folder, based on a global view media permission template.
    /// </summary>
    internal static Permission ConvertToDynamicPermission(Permission permission) => _permissionTemplates.TryGetValue(permission.Name, out var result) ? result : null;

    internal static Permission CreateDynamicPermission(Permission template, string secureFolder)
    {
        ArgumentNullException.ThrowIfNull(template);

        secureFolder = secureFolder?.Trim(_trimSecurePathChars);

        var key = new ValueTuple<string, string>(template.Name, secureFolder);

        if (_permissionsByFolder.TryGetValue(key, out var permission))
        {
            return permission;
        }

        permission = new Permission(
            string.Format(template.Name, secureFolder),
            string.Format(template.Description, secureFolder),
            (template.ImpliedBy ?? Array.Empty<Permission>()).Select(t => CreateDynamicPermission(t, secureFolder))
        );

        var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(_permissionsByFolder)
        {
            [key] = permission,
        };

        _permissionsByFolder = localPermissions;

        return permission;
    }

    private async Task<IEnumerable<Permission>> GetPermissionsInternalAsync()
    {
        // The ViewRootMedia permission must be implied by any subfolder permission.
        var viewRootImpliedBy = new List<Permission>(ViewRootMedia.ImpliedBy);
        var result = new List<Permission>()
        {
            ViewMedia,
            new (ViewRootMedia.Name, ViewRootMedia.Description, viewRootImpliedBy),
            ViewOthersMedia,
            ViewOwnMedia
        };

        await foreach (var entry in _fileStore.GetDirectoryContentAsync())
        {
            if (!entry.IsDirectory)
            {
                continue;
            }

            if (entry.Name == _mediaOptions.AssetsUsersFolder ||
                entry.Name == _attachedMediaFieldFileService.MediaFieldsFolder)
            {
                continue;
            }

            var folderPath = entry.Path;

            foreach (var template in _permissionTemplates)
            {
                var dynamicPermission = CreateDynamicPermission(template.Value, folderPath);
                result.Add(dynamicPermission);
                viewRootImpliedBy.Add(dynamicPermission);
            }
        }

        return result.AsEnumerable();
    }
}
