using System.Collections.ObjectModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using OrchardCore.Localization;
using OrchardCore.Media.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media;

public sealed class SecureMediaPermissions : IPermissionProvider
{
    private static readonly PermissionTemplate s_viewMediaTemplate = new(
        "ViewMediaContent_{0}",
        LocalizationSource.Create<SecureMediaPermissions>("View media content in folder '{0}'"),
        MediaPermissions.ViewMedia);

    private static Dictionary<ValueTuple<string, string>, Permission> s_permissionsByFolder = new();
    private static readonly char[] s_trimSecurePathChars = ['/', '\\', ' '];
    private static readonly ReadOnlyDictionary<string, PermissionTemplate> s_permissionTemplates = new Dictionary<string, PermissionTemplate>
    {
        [MediaPermissions.ViewMedia.Name] = s_viewMediaTemplate,
    }.AsReadOnly();

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
                Name = OrchardCoreConstants.Roles.Administrator,
                Permissions = new[]
                {
                    MediaPermissions.ViewMedia,
                    MediaPermissions.ViewOthersMedia,
                },
            },
            new PermissionStereotype
            {
                Name = OrchardCoreConstants.Roles.Authenticated,
                Permissions = new[]
                {
                    MediaPermissions.ViewOwnMedia,
                },
            },
            // Note: No stereotype for the Anonymous role. Secure Media exists to decide who may see which
            // media, so enabling it must not hand every folder to the public up front. An administrator
            // grants 'ViewMediaContent', or the folder permissions below it, to whatever should stay
            // publicly readable. Anonymous claims are evaluated for authenticated users too, so a grant
            // here would also silently override every folder-scoped restriction on signed-in roles.
        };
    }

    /// <summary>
    /// Create a dynamic permission specific to the provided <paramref name="secureFolder"/> using internal templates,
    /// based on the permission identified by the <paramref name="basePermissionName"/>.
    /// </summary>
    /// <param name="basePermissionName"></param>
    /// <param name="secureFolder"></param>
    /// <returns></returns>
    internal static Permission CreateDynamicPermissionOf(string basePermissionName, string secureFolder)
    {
        if (!s_permissionTemplates.TryGetValue(basePermissionName, out var template))
        {
            return null;
        }

        secureFolder = secureFolder?.Trim(s_trimSecurePathChars);
        var key = new ValueTuple<string, string>(template.Name, secureFolder);

        if (s_permissionsByFolder.TryGetValue(key, out var permissionByFolder))
        {
            return permissionByFolder;
        }

        var permission = template.CreateDynamicPermission(secureFolder);
        var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(s_permissionsByFolder)
        {
            [key] = permission,
        };

        s_permissionsByFolder = localPermissions;
        return permission;
    }

    private async Task<IEnumerable<Permission>> GetPermissionsInternalAsync()
    {
        // The ViewRootMedia permission must be implied by any subfolder permission.
        var viewRootImpliedBy = new List<Permission>(MediaPermissions.ViewRootMedia.ImpliedBy);
        var result = new List<Permission>()
        {
            MediaPermissions.ViewMedia,
            new (MediaPermissions.ViewRootMedia.Name, MediaPermissions.ViewRootMedia.Description, viewRootImpliedBy),
            MediaPermissions.ViewOthersMedia,
            MediaPermissions.ViewOwnMedia,
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

            foreach (var template in s_permissionTemplates)
            {
                var dynamicPermission = CreateDynamicPermissionOf(template.Key, folderPath);
                result.Add(dynamicPermission);
                viewRootImpliedBy.Add(dynamicPermission);
            }
        }

        return result.AsEnumerable();
    }
}
