using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Creates the tokens of download links, and decides who may download a shared file: a signed-in recipient, before the
/// link expires or is revoked. A request through the link must also carry its token; recipients can also download their
/// files from the list of the files shared with them, which has no token.
/// </summary>
public static class DataPipelineSharedFileAccess
{
    /// <summary>
    /// Creates a new random token, safe in a URL.
    /// </summary>
    /// <returns>The token.</returns>
    public static string CreateToken()
        => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    /// <summary>
    /// Hashes a token, as stored. The token itself is never stored.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The SHA-256 hash of the token, as hexadecimal text.</returns>
    public static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty)));

    /// <summary>
    /// Decides whether a user may download a shared file.
    /// </summary>
    /// <param name="file">The shared file.</param>
    /// <param name="token">The token of the link, or <see langword="null"/> for a download from the list of the files shared
    /// with the user.</param>
    /// <param name="user">The signed-in user.</param>
    /// <param name="utcNow">The current time, in UTC.</param>
    /// <returns>Whether the download is allowed, or why not.</returns>
    public static DataPipelineSharedFileAccessResult Check(DataPipelineSharedFile file, string token, ClaimsPrincipal user, DateTime utcNow)
    {
        if (file is null)
        {
            return DataPipelineSharedFileAccessResult.NotFound;
        }

        if (!file.IsActive(utcNow))
        {
            return DataPipelineSharedFileAccessResult.Expired;
        }

        if (token is not null)
        {
            var expected = Convert.FromHexString(file.TokenHash ?? string.Empty);
            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            if (expected.Length != actual.Length || !CryptographicOperations.FixedTimeEquals(expected, actual))
            {
                return DataPipelineSharedFileAccessResult.InvalidToken;
            }
        }

        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || !file.RecipientUserIds.Contains(userId, StringComparer.Ordinal))
        {
            return DataPipelineSharedFileAccessResult.NotARecipient;
        }

        return DataPipelineSharedFileAccessResult.Allowed;
    }
}

/// <summary>
/// Tells whether a user may download a shared file, or why not.
/// </summary>
public enum DataPipelineSharedFileAccessResult
{
    /// <summary>
    /// The user may download the file.
    /// </summary>
    Allowed,

    /// <summary>
    /// There is no such shared file.
    /// </summary>
    NotFound,

    /// <summary>
    /// The link expired or was revoked.
    /// </summary>
    Expired,

    /// <summary>
    /// The token of the link is wrong.
    /// </summary>
    InvalidToken,

    /// <summary>
    /// The user is not a recipient of the file.
    /// </summary>
    NotARecipient,
}
