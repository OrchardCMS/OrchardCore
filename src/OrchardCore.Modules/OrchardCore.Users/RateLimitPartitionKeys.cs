using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.RateLimits;

namespace OrchardCore.Users;

/// <summary>
/// Selects the client that the requests of the sign-in steps that follow the password check are counted against.
/// </summary>
/// <remarks>
/// These steps are only useful to a user who already passed the password check, so they are counted against that user
/// instead of the IP address. Many users who share an IP address, such as the employees of an office, then don't share
/// a single budget, while each account stays protected against code guessing and email or SMS flooding.
/// </remarks>
internal static class RateLimitPartitionKeys
{
    /// <summary>
    /// Gets the key of the user who passed the password check and is completing the two-factor authentication,
    /// otherwise the key of the client.
    /// </summary>
    internal static string GetTwoFactorUser(HttpContext context)
    {
        // The 'Identity.TwoFactorUserId' cookie stores the user identifier in the 'Name' claim.
        return GetCookieUser(context, IdentityConstants.TwoFactorUserIdScheme, ClaimTypes.Name);
    }

    /// <summary>
    /// Gets the key of the user who passed the password check but hasn't confirmed their email yet,
    /// otherwise the key of the client.
    /// </summary>
    internal static string GetEmailConfirmationUser(HttpContext context)
    {
        return GetCookieUser(context, UserConstants.EmailConfirmationAuthenticationScheme, ClaimTypes.NameIdentifier);
    }

    private static string GetCookieUser(HttpContext context, string scheme, string userIdClaimType)
    {
        // The rate limiter selects partitions synchronously, so the cookie is read directly instead of authenticating the scheme.
        var options = context.RequestServices.GetService<IOptionsMonitor<CookieAuthenticationOptions>>()?.Get(scheme);
        var cookie = options?.CookieManager?.GetRequestCookie(context, options.Cookie.Name);

        if (!string.IsNullOrEmpty(cookie) && options.TicketDataFormat is not null)
        {
            var ticket = options.TicketDataFormat.Unprotect(cookie);
            var expiresUtc = ticket?.Properties.ExpiresUtc;
            var userId = ticket?.Principal?.FindFirstValue(userIdClaimType);

            if (!string.IsNullOrEmpty(userId) &&
                (expiresUtc is null || expiresUtc > (options.TimeProvider ?? TimeProvider.System).GetUtcNow()))
            {
                return RateLimitPartitionHelpers.GetUserPartitionKey(userId);
            }
        }

        return RateLimitPartitionHelpers.GetClientPartitionKey(context);
    }
}
