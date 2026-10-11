using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Media;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Checks the media paths of steps and the access of the user a run is for.
/// </summary>
internal static class MediaStepPaths
{
    /// <summary>
    /// Normalizes a media path, or returns <see langword="null"/> when it leaves the media library.
    /// </summary>
    public static string Normalize(string path)
    {
        var normalized = (path ?? string.Empty).Replace('\\', '/').Trim('/', ' ');

        if (normalized.Split('/').Any(segment => segment is ".." or "."))
        {
            return null;
        }

        return normalized;
    }

    /// <summary>
    /// Tells whether a user may manage the media of a folder, as the media library would.
    /// </summary>
    public static async Task<bool> CanManageAsync(IAuthorizationService authorizationService, ClaimsPrincipal user, string folder)
        => user is not null &&
            await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMedia) &&
            await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)(folder ?? string.Empty));
}
