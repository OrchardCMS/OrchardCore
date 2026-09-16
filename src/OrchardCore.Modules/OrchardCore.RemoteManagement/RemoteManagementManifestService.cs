using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules.Manifest;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.RemoteManagement;

internal sealed class RemoteManagementManifestService
{
    private static readonly Uri s_jsonSchemaDialect = new("https://json-schema.org/draft/2020-12/schema");
    private readonly IEnumerable<IRemoteManagementCapabilityProvider> _capabilityProviders;
    private readonly OpenIddictServerOptions _serverOptions;
    private readonly ShellSettings _shellSettings;
    private readonly bool _cliEnabled;

    public RemoteManagementManifestService(
        IEnumerable<IRemoteManagementCapabilityProvider> capabilityProviders,
        IOptions<OpenIddictServerOptions> serverOptions,
        ShellSettings shellSettings,
        IOptions<RemoteManagementOptions> options = null)
    {
        _capabilityProviders = capabilityProviders;
        _serverOptions = serverOptions.Value;
        _shellSettings = shellSettings;
        _cliEnabled = options?.Value.CliEnabled == true;
    }

    public RemoteManagementManifest CreateBootstrap(HttpRequest request)
    {
        var tenantUrl = GetTenantUrl(request);

        return new RemoteManagementManifest
        {
            Authentication = CreateAuthentication(tenantUrl),
            ManagementManifestUrl = new Uri(tenantUrl, "api/management/manifest"),
        };
    }

    public async ValueTask<RemoteManagementManifest> CreateAuthenticatedAsync(HttpRequest request)
    {
        var tenantUrl = GetTenantUrl(request);
        var manifest = CreateBootstrap(request);

        manifest.ProductVersion = ManifestConstants.OrchardCoreVersion;
        manifest.TenantId = _shellSettings.Name;
        manifest.OpenApiUrl = new Uri(tenantUrl, "openapi/v1.json");
        manifest.JsonSchemaDialect = s_jsonSchemaDialect;
        if (_cliEnabled)
        {
            manifest.MinimumCliVersion = "1.0.0";
            manifest.RecommendedCliVersion = "1.0.0";
        }
        manifest.DocumentationIndexUrl = new Uri("https://docs.orchardcore.net/en/latest/search/search_index.json");

        foreach (var provider in _capabilityProviders)
        {
            foreach (var capability in await provider.GetCapabilitiesAsync())
            {
                manifest.Capabilities.Add(capability);
            }
        }

        return manifest;
    }

    private RemoteManagementAuthentication CreateAuthentication(Uri tenantUrl)
    {
        var authentication = new RemoteManagementAuthentication
        {
            Authority = _serverOptions.Issuer ?? tenantUrl,
            ClientId = _cliEnabled ? RemoteManagementConstants.CliClientId : null,
        };

        AddGrantType(authentication, GrantTypes.AuthorizationCode);
        AddGrantType(authentication, GrantTypes.DeviceCode);
        AddGrantType(authentication, GrantTypes.ClientCredentials);
        authentication.Scopes.Add("openid");
        authentication.Scopes.Add("profile");
        authentication.Scopes.Add("roles");
        if (_serverOptions.GrantTypes.Contains(GrantTypes.RefreshToken))
        {
            authentication.Scopes.Add("offline_access");
        }

        authentication.Scopes.Add(RemoteManagementConstants.ManagementScope);

        return authentication;
    }

    private void AddGrantType(RemoteManagementAuthentication authentication, string grantType)
    {
        if (_serverOptions.GrantTypes.Contains(grantType))
        {
            authentication.GrantTypes.Add(grantType);
        }
    }

    private static Uri GetTenantUrl(HttpRequest request)
    {
        var pathBase = request.PathBase.HasValue ? $"{request.PathBase.Value}/" : "/";
        return new Uri($"{request.Scheme}://{request.Host}{pathBase}");
    }
}
