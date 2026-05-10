using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.OpenApi.Services;

public sealed class OpenApiJSLocalizer(IStringLocalizer<OpenApiJSLocalizer> S) : IJSLocalizer
{
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
            ["allowAnonymousSchemaAccessHint"] = S["When enabled, the JSON schema endpoints can be accessed without authentication. Disable this to require the <strong>Manage API</strong> permission. External tools like NSwag will need to provide authentication when disabled."].Value,
            ["apiAuthentication"] = S["API Authentication"].Value,
            ["authenticationType"] = S["Authentication Type"].Value,
            ["cookieDefault"] = S["Cookie (default)"].Value,
            ["oauth2Pkce"] = S["OAuth2 Authorization Code + PKCE"].Value,
            ["oauth2ClientCredentials"] = S["OAuth2 Client Credentials"].Value,
            ["authenticationTypeHint"] = S["Select the authentication method used by the \"Try it out\" buttons in the API documentation UIs."].Value,
            ["cookieInfo"] = S["The API documentation UIs will use cookie authentication. If you are logged in, the \"Try it out\" buttons will automatically use your session cookie. No additional configuration is needed."].Value,
            ["pkceInfo"] = S["For interactive authentication. The \"Authorize\" button will redirect users to the authorization server to sign in, then exchange the code for a token using PKCE."].Value,
            ["pkceEnsure"] = S["Ensure your OpenID Connect application has <strong>Allow Authorization Code Flow</strong> enabled and a redirect URI configured for the Swagger UI callback."].Value,
            ["clientCredsInfo"] = S["For machine-to-machine authentication. The \"Authorize\" dialog will prompt for client ID and client secret, then exchange them for a bearer token."].Value,
            ["clientCredsEnsure"] = S["Ensure your OpenID Connect application has <strong>Allow Client Credentials Flow</strong> enabled and is configured as a <strong>Confidential client</strong>. Assign the appropriate <strong>Client Credentials Roles</strong>."].Value,
            ["openIdTokenValidation"] = S["The <strong>OpenID Token Validation</strong> feature must be enabled for Bearer token authentication to work on API endpoints."].Value,
            ["sessionCookieHint"] = S["Hint: If you are logged in, the \"Try it out\" buttons will still work using your session cookie even without clicking \"Authorize\"."].Value,
            ["authorizationUrl"] = S["Authorization URL"].Value,
            ["authorizationUrlHint"] = S["The OAuth2 authorization endpoint. Relative URLs are resolved against the current tenant."].Value,
            ["tokenUrl"] = S["Token URL"].Value,
            ["tokenUrlHint"] = S["The OAuth2 token endpoint. Relative URLs are resolved against the current tenant."].Value,
            ["clientId"] = S["Client ID"].Value,
            ["clientIdHint"] = S["The OAuth2 client ID registered for the API documentation UIs."].Value,
            ["scopes"] = S["Scopes"].Value,
            ["scopesHint"] = S["A space-separated list of OAuth2 scopes to request."].Value,
            ["testConnection"] = S["Test Connection"].Value,
            ["clientSecret"] = S["Client Secret"].Value,
            ["enterClientSecret"] = S["Enter client secret"].Value,
            ["clientSecretHint"] = S["For testing only — this value is not saved to settings."].Value,
            ["testing"] = S["Testing..."].Value,
        };
    }
}
