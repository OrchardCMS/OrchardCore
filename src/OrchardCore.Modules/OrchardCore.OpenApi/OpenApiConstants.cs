namespace OrchardCore.OpenApi;

internal static class OpenApiConstants
{
    /// <summary>
    /// Conventional client id of the public OAuth2/PKCE application the documentation UIs
    /// (Swagger, Scalar) use to silently acquire a bearer token against the same-tenant
    /// OpenID Connect server. Provisioned by the <c>OpenApiPkce</c> recipe.
    /// </summary>
    public const string DocumentationClientId = "openapi";

    /// <summary>
    /// Scopes requested for the documentation UI token. <c>roles</c> is required so the token
    /// carries the caller's roles, which the API permission checks rely on.
    /// </summary>
    public const string DocumentationScopes = "openid email profile roles";

    /// <summary>
    /// Module-relative path (served from the module's <c>wwwroot</c>) of the hidden-iframe page
    /// that completes the silent (prompt=none) token renewal. Must match a redirect URI
    /// registered on the <see cref="DocumentationClientId"/> client.
    /// </summary>
    public const string SilentCallbackPath = "/OrchardCore.OpenApi/openapi-oidc-silent.html";

    /// <summary>
    /// Module-relative path (served from the module's <c>wwwroot</c>) of the bundled
    /// oidc-client-ts silent-auth script injected into the Swagger and Scalar pages.
    /// </summary>
    public const string AuthScriptPath = "/OrchardCore.OpenApi/Scripts/openapi-ui-auth/openapi-ui-auth.js";
}
