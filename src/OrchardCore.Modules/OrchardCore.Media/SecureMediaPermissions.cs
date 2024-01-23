using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media
{
    public class SecureMediaPermissions : IPermissionProvider
    {
        // Note: The ManageMediaFolder permission grants all access, so viewing must be implied by it too.
        public static readonly Permission ViewMedia = new("ViewMediaContent", "View media content in all folders", new[] { Permissions.ManageMediaFolder });
        public static readonly Permission ViewRootMedia = new("ViewRootMediaContent", "View media content in the root folder", new[] { ViewMedia });
        public static readonly Permission ViewOthersMedia = new("ViewOthersMediaContent", "View others media content", new[] { Permissions.ManageMediaFolder });
        public static readonly Permission ViewOwnMedia = new("ViewOwnMediaContent", "View own media content", new[] { ViewOthersMedia });

        private static readonly Permission s_viewMediaTemplate = new("ViewMediaContent_{0}", "View media content in {0}", new[] { ViewMedia });

        private static Dictionary<ValueTuple<string, string>, Permission> s_permissionsByFolder = new();
        private static readonly char[] s_trimSecurePathChars = ['/', '\\', ' '];
        public static readonly ReadOnlyDictionary<string, Permission> PermissionTemplates = new(new Dictionary<string, Permission>()
        {
            { ViewMedia.Name, s_viewMediaTemplate },
        });

        private readonly MediaOptions _mediaOptions;
        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
        private readonly IMediaFileStore _fileStore;

        public SecureMediaPermissions(
            IOptions<MediaOptions> options,
            IMediaFileStore fileStore,
            AttachedMediaFieldFileService attachedMediaFieldFileService)
        {
            _mediaOptions = options.Value;
            _fileStore = fileStore;
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            // The ViewRootMedia permission must be implied by any sub folder permission.
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
                    continue;

                if (entry.Name == _mediaOptions.AssetsUsersFolder ||
                    entry.Name == _attachedMediaFieldFileService.MediaFieldsFolder)
                    continue;

                var folderPath = entry.Path;

                foreach (var template in PermissionTemplates)
                {
                    var dynamicPermission = CreateDynamicPermission(template.Value, folderPath);
                    result.Add(dynamicPermission);
                    viewRootImpliedBy.Add(dynamicPermission);
                }
            }

            return result.AsEnumerable();
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
        /// Returns a dynamic permission for a secure folder, based on a global view content permission template-
        /// </summary>
        public static Permission ConvertToDynamicPermission(Permission permission)
        {
            if (PermissionTemplates.TryGetValue(permission.Name, out var result))
            {
                return result;
            }

            return null;
        }

        internal static Permission CreateDynamicPermission(Permission template, string secureFolder)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            secureFolder = secureFolder?.Trim(s_trimSecurePathChars)?.ToLowerInvariant();

            var key = new ValueTuple<string, string>(template.Name, secureFolder);

            if (s_permissionsByFolder.TryGetValue(key, out var permission))
            {
                return permission;
            }

            permission = new Permission(
                string.Format(template.Name, secureFolder),
                string.Format(template.Description, secureFolder),
                (template.ImpliedBy ?? Array.Empty<Permission>()).Select(t => CreateDynamicPermission(t, secureFolder))
            );

            var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(s_permissionsByFolder)
            {
                [key] = permission,
            };

            s_permissionsByFolder = localPermissions;

            return permission;
        }
    }
}
