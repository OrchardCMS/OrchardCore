using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services;

internal sealed class MediaFileExtensionPolicy : IMediaFileExtensionPolicy
{
    private readonly IAuthorizationService _authorizationService;
    private readonly MediaOptions _mediaOptions;

    public MediaFileExtensionPolicy(
        IAuthorizationService authorizationService,
        IOptions<MediaOptions> mediaOptions)
    {
        _authorizationService = authorizationService;
        _mediaOptions = mediaOptions.Value;
    }

    public async Task<HashSet<string>> GetAllowedFileExtensionsAsync(ClaimsPrincipal user)
    {
        var allowedExtensions = _mediaOptions.AllowedFileExtensions
            .Except(_mediaOptions.AllowedFileExtensionsWithPermission, StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (await _authorizationService.AuthorizeAsync(user, MediaPermissions.UploadRestrictedMedia))
        {
            allowedExtensions.UnionWith(_mediaOptions.AllowedFileExtensionsWithPermission);
        }

        return allowedExtensions;
    }

    public async Task<bool> IsAllowedAsync(ClaimsPrincipal user, string extension)
    {
        if (_mediaOptions.AllowedFileExtensionsWithPermission.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return await _authorizationService.AuthorizeAsync(user, MediaPermissions.UploadRestrictedMedia);
        }

        return _mediaOptions.AllowedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public static HashSet<string> GetConfiguredFileExtensions(MediaOptions mediaOptions)
    {
        var extensions = mediaOptions.AllowedFileExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        extensions.UnionWith(mediaOptions.AllowedFileExtensionsWithPermission);

        return extensions;
    }
}
