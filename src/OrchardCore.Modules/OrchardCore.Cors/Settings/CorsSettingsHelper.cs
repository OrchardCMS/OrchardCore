using CorsConstants = Microsoft.AspNetCore.Cors.Infrastructure.CorsConstants;

namespace OrchardCore.Cors.Settings;

/// <summary>
/// Shared helpers for evaluating CORS policy settings consistently
/// across the admin UI and the options configuration pipeline.
/// </summary>
internal static class CorsSettingsHelper
{
    /// <summary>
    /// Returns <see langword="true"/> when the policy effectively allows any origin —
    /// either via the <paramref name="allowAnyOrigin"/> flag or by the presence of
    /// a wildcard <c>"*"</c> entry in <paramref name="allowedOrigins"/>.
    /// </summary>
    public static bool IsAnyOriginAllowed(bool allowAnyOrigin, string[]? allowedOrigins)
        => allowAnyOrigin
            || allowedOrigins?.Any(origin =>
                string.Equals(origin?.Trim(), CorsConstants.AnyOrigin, StringComparison.Ordinal)) == true;
}
