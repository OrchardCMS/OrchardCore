using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media.Processing;

internal static class SecureMediaHttpContextExtensions
{
    // Must stay the same as the key used by SecureMediaExtensions in the OrchardCore.Media module,
    // which marks the requests of secure media.
    private const string IsSecureMediaKey = "IsSecureMedia";

    public static bool IsSecureMediaRequested(this HttpContext httpContext)
        => httpContext.Items.ContainsKey(IsSecureMediaKey);
}
