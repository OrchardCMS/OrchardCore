namespace OrchardCore.OpenApi;

internal static class OpenApiAuthenticationDefaults
{
    /// <summary>
    /// A policy scheme scoped to this module: forwards to the shared "Api" (token) scheme
    /// when an Authorization header is present, otherwise falls back to the app's default
    /// (cookie) scheme so that same-origin requests from the OpenApi Vue settings page work.
    /// Unlike the shared "Api" scheme, this cookie fallback never applies to any other module.
    /// </summary>
    public const string CookieOrTokenScheme = "OpenApi.CookieOrToken";
}
