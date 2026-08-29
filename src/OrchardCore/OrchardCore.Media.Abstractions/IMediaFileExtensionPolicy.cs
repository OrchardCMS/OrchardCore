using System.Security.Claims;

namespace OrchardCore.Media;

/// <summary>
/// Evaluates configured media file extensions against the permissions of a user.
/// </summary>
public interface IMediaFileExtensionPolicy
{
    /// <summary>
    /// Returns the configured file extensions that the specified user may upload.
    /// </summary>
    /// <param name="user">The user whose effective extension list should be returned.</param>
    /// <returns>The file extensions the user may upload.</returns>
    Task<HashSet<string>> GetAllowedFileExtensionsAsync(ClaimsPrincipal user);

    /// <summary>
    /// Returns whether the specified user may upload the file extension.
    /// </summary>
    /// <param name="user">The user attempting the upload.</param>
    /// <param name="extension">The file extension to evaluate.</param>
    /// <returns><see langword="true"/> when the extension is configured and the user has any required permission.</returns>
    Task<bool> IsAllowedAsync(ClaimsPrincipal user, string extension);
}
