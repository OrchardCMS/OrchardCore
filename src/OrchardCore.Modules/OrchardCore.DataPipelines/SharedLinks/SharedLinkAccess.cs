using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Creates the tokens of shared links, and decides who may use a link: a signed-in recipient, with the token of the
/// link, before the link expires, is used up or is revoked.
/// </summary>
public static class SharedLinkAccess
{
    /// <summary>
    /// Creates a new random token of 256 bits, safe in a URL.
    /// </summary>
    /// <returns>The token.</returns>
    public static string CreateToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    /// <summary>
    /// Hashes a token, as stored. The token itself is never stored.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The SHA-256 hash of the token, as hexadecimal text.</returns>
    public static string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty)));
    }

    /// <summary>
    /// Decides whether a user may use a link.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <param name="token">The token of the link, from its URL.</param>
    /// <param name="user">The signed-in user.</param>
    /// <param name="utcNow">The current time, in UTC.</param>
    /// <returns>Whether the user may use the link, or why not.</returns>
    public static SharedLinkAccessResult Check(SharedLink link, string token, ClaimsPrincipal user, DateTime utcNow)
    {
        if (link is null)
        {
            return SharedLinkAccessResult.NotFound;
        }

        // The token is checked first, so that only the holders of the link learn about its state.
        if (string.IsNullOrEmpty(token))
        {
            return SharedLinkAccessResult.InvalidToken;
        }

        var expected = Convert.FromHexString(link.TokenHash ?? string.Empty);
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        if (expected.Length != actual.Length || !CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            return SharedLinkAccessResult.InvalidToken;
        }

        if (link.RevokedUtc is not null || link.ExpiresUtc <= utcNow)
        {
            return SharedLinkAccessResult.Expired;
        }

        if (link.IsUsedUp)
        {
            return SharedLinkAccessResult.UsedUp;
        }

        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || !link.RecipientUserIds.Contains(userId, StringComparer.Ordinal))
        {
            return SharedLinkAccessResult.NotARecipient;
        }

        return SharedLinkAccessResult.Allowed;
    }
}
