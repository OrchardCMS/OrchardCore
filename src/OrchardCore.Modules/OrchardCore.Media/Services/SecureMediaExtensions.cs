using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media.Services;

internal static class SecureMediaExtensions
{
    private const string IsSecureMediaKey = "IsSecureMedia";

    public static bool IsSecureMediaEnabled(this IServiceProvider serviceProvider)
        => serviceProvider.GetService(typeof(SecureMediaMarker)) is not null;

    public static bool IsSecureMediaEnabled(this HttpContext httpContext)
        => httpContext.RequestServices.IsSecureMediaEnabled();

    public static bool IsSecureMediaRequested(this HttpContext httpContext)
        => httpContext.Items.ContainsKey(IsSecureMediaKey);

    public static void MarkAsSecureMediaRequested(this HttpContext httpContext)
        => httpContext.Items[IsSecureMediaKey] = bool.TrueString;
}
