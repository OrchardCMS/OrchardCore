using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.OpenApi.Services;

public sealed class OpenApiJSLocalizer : IJSLocalizer
{
    private readonly IStringLocalizer S;

    public OpenApiJSLocalizer(IStringLocalizer<OpenApiJSLocalizer> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (!string.Equals(group, "openapi-settings", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new Dictionary<string, string>
        {
            ["apiDocumentationUIs"] = S["API Documentation UIs"].Value,
            ["enabled"] = S["Enabled"].Value,
            ["disabled"] = S["Disabled"].Value,
            ["swaggerUI"] = S["Swagger UI"].Value,
            ["interactiveApiExplorerAt"] = S["Interactive API explorer at"].Value,
            ["reDocUI"] = S["ReDoc UI"].Value,
            ["readOnlyApiDocumentationAt"] = S["Read-only API documentation at"].Value,
            ["scalarUI"] = S["Scalar UI"].Value,
            ["modernApiReferenceAt"] = S["Modern API reference at"].Value,
            ["enableDisableUIsHint"] = S["Enable or disable API documentation UIs via the Features page."].Value,
            ["apiSchemaAccess"] = S["API Schema Access"].Value,
            ["allowAnonymousSchemaAccess"] = S["Allow anonymous access to the API schema"].Value,
            ["allowAnonymousSchemaAccessHint"] = S["When enabled, the JSON schema endpoints can be accessed without authentication. When disabled (the default), they require the <strong>View OpenAPI Content</strong> permission, and external tools like NSwag must provide authentication. The OpenApi Generation recipes enable this setting."].Value,
            ["apiAuthentication"] = S["API Authentication"].Value,
            ["authenticationType"] = S["Authentication Type"].Value,
            ["cookieDefault"] = S["None (browse documentation only)"].Value,
            ["oauth2Pkce"] = S["OAuth2 Authorization Code + PKCE"].Value,
            ["authenticationTypeHint"] = S["Select the authentication method used by the \"Try it out\" buttons in the API documentation UIs."].Value,
            ["cookieInfo"] = S["The documentation UIs are accessible with your admin session, but API endpoints require a Bearer token: \"Try it out\" requests will not be authenticated. Select OAuth2 Authorization Code + PKCE to enable authenticated requests."].Value,
            ["pkceInfo"] = S["For interactive authentication. The \"Authorize\" button will redirect users to the authorization server to sign in, then exchange the code for a token using PKCE."].Value,
            ["pkceEnsure"] = S["Ensure your OpenID Connect application has <strong>Allow Authorization Code Flow</strong> enabled and a redirect URI configured for each documentation UI you use (e.g. Swagger UI's <code>oauth2-redirect.html</code> or the Scalar page URL)."].Value,
            ["openIdTokenValidation"] = S["The <strong>OpenID Token Validation</strong> feature must be enabled for Bearer token authentication to work on API endpoints."].Value,
            ["authorizationUrl"] = S["Authorization URL"].Value,
            ["authorizationUrlHint"] = S["The OAuth2 authorization endpoint. Filled automatically from the Server Metadata URL when left empty. Relative URLs are resolved against the current tenant."].Value,
            ["tokenUrl"] = S["Token URL"].Value,
            ["tokenUrlHint"] = S["The OAuth2 token endpoint. Filled automatically from the Server Metadata URL when left empty. Relative URLs are resolved against the current tenant."].Value,
            ["serverConfiguration"] = S["OpenID Connect Server"].Value,
            ["clientConfiguration"] = S["OAuth2 Client"].Value,
            ["autoFillEndpoints"] = S["Auto-fill endpoints"].Value,
            ["fetchingMetadata"] = S["Fetching..."].Value,
            ["metadataFetchSuccess"] = S["The endpoint URLs below were filled from the server metadata."].Value,
            ["availableScopes"] = S["Scopes supported by the server (click to add or remove):"].Value,
            ["editEndpointsManually"] = S["Edit the endpoint URLs manually"].Value,
            ["metadataFetchError"] = S["Could not fetch the server metadata from the browser. Check the URL (and the server's CORS policy if it is external), or fill the endpoint URLs manually — they are also filled automatically on save when left empty."].Value,
            ["serverMetadataUrl"] = S["Server Metadata URL"].Value,
            ["serverMetadataUrlHint"] = S["Optional. The OpenID Connect server metadata document used to validate this configuration on save. The Authorization and Token URLs are filled from it when left empty. Relative URLs are resolved against the current tenant. When empty, no validation is performed."].Value,
            ["clientId"] = S["Client ID"].Value,
            ["clientIdHint"] = S["The OAuth2 client ID registered for the API documentation UIs."].Value,
            ["scopes"] = S["Scopes"].Value,
            ["scopesHint"] = S["A space-separated list of OAuth2 scopes to request."].Value,
        };
    }
}
