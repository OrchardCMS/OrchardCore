using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId;

/// <summary>
/// Registers public, PKCE-protected OAuth applications for MCP clients.
/// </summary>
public sealed class RemoteManagementMcpConfigurationService
{
    private const string ApplicationMarker = "orchardcore:remote-management:client";
    private static readonly string[] s_permissions =
    [
        OpenIddictConstants.Permissions.Endpoints.Authorization,
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.Endpoints.Revocation,
        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
        OpenIddictConstants.Permissions.ResponseTypes.Code,
        OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
        OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
        OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
        OpenIddictConstants.Permissions.Prefixes.Scope + RemoteManagementConstants.ManagementScope,
    ];

    private readonly IOpenIdApplicationManager _applicationManager;
    private readonly IRemoteManagementTenantConfigurationService _tenantConfiguration;

    public RemoteManagementMcpConfigurationService(
        IOpenIdApplicationManager applicationManager,
        IRemoteManagementTenantConfigurationService tenantConfiguration)
    {
        _applicationManager = applicationManager;
        _tenantConfiguration = tenantConfiguration;
    }

    /// <summary>
    /// Loads a client previously registered by the MCP configuration action.
    /// </summary>
    /// <param name="clientId">The client identifier to inspect.</param>
    public async Task<RemoteManagementMcpViewModel> GetConfigurationAsync(string clientId = "orchardcore-mcp")
    {
        clientId = string.IsNullOrWhiteSpace(clientId) ? "orchardcore-mcp" : clientId;
        var model = new RemoteManagementMcpViewModel { ClientId = clientId };
        var application = await _applicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            return model;
        }

        var descriptor = new OpenIdApplicationDescriptor();
        await _applicationManager.PopulateAsync(descriptor, application);
        if (!IsMcpApplication(descriptor))
        {
            return model;
        }

        model.RedirectUris = string.Join(System.Environment.NewLine, descriptor.RedirectUris.Select(uri => uri.AbsoluteUri).Order(StringComparer.Ordinal));
        model.IsConfigured = descriptor.ClientType == OpenIddictConstants.ClientTypes.Public &&
            descriptor.ConsentType == OpenIddictConstants.ConsentTypes.Explicit &&
            descriptor.Requirements.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange) &&
            s_permissions.All(descriptor.Permissions.Contains) && descriptor.RedirectUris.Count > 0;
        return model;
    }

    /// <summary>
    /// Configures shared authentication and creates or repairs a dedicated MCP application.
    /// </summary>
    /// <param name="model">The client identifier and complete set of callback URIs.</param>
    public async Task ConfigureAsync(RemoteManagementMcpViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        Validator.ValidateObject(model, new ValidationContext(model), validateAllProperties: true);
        if (model.ClientId.Any(char.IsWhiteSpace) || model.ClientId == RemoteManagementConstants.CliClientId)
        {
            throw new ValidationException("Use a client identifier without whitespace that is different from the Pomi client identifier.");
        }

        var redirects = ParseRedirectUris(model.RedirectUris);
        var application = await _applicationManager.FindByClientIdAsync(model.ClientId);
        var descriptor = new OpenIdApplicationDescriptor();
        if (application is not null)
        {
            await _applicationManager.PopulateAsync(descriptor, application);
            if (!IsMcpApplication(descriptor) || descriptor.ClientType != OpenIddictConstants.ClientTypes.Public)
            {
                throw new ValidationException("This client identifier belongs to another OpenID application. Choose a different identifier.");
            }
        }

        descriptor.ClientId = model.ClientId;
        descriptor.DisplayName ??= "MCP client";
        descriptor.ApplicationType = redirects.Any(uri => uri.IsLoopback)
            ? OpenIddictConstants.ApplicationTypes.Native : OpenIddictConstants.ApplicationTypes.Web;
        descriptor.ClientType = OpenIddictConstants.ClientTypes.Public;
        descriptor.ConsentType = OpenIddictConstants.ConsentTypes.Explicit;
        descriptor.RedirectUris.Clear();
        descriptor.RedirectUris.UnionWith(redirects);
        descriptor.Permissions.UnionWith(s_permissions);
        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        descriptor.Properties[ApplicationMarker] = JsonSerializer.SerializeToElement("mcp");

        // Validate the client inputs and ownership before changing shared server settings.
        await _tenantConfiguration.ConfigureAsync();
        if (application is null)
        {
            await _applicationManager.CreateAsync(descriptor);
        }
        else
        {
            await _applicationManager.UpdateAsync(application, descriptor);
        }
    }

    private static bool IsMcpApplication(OpenIdApplicationDescriptor descriptor) =>
        descriptor.Properties.TryGetValue(ApplicationMarker, out var value) &&
        value.ValueKind == JsonValueKind.String && value.GetString() == "mcp";

    internal static Uri[] ParseRedirectUris(string value)
    {
        var lines = value.Split(['\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            throw new ValidationException("Enter at least one OAuth callback URI supplied by your MCP client.");
        }

        var redirects = new List<Uri>();
        foreach (var line in lines)
        {
            if (!Uri.TryCreate(line, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)) ||
                !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo) || line.Contains('*'))
            {
                throw new ValidationException("Callback URIs must use HTTPS or loopback HTTP, without fragments, credentials, or wildcards.");
            }

            redirects.Add(uri);
        }

        return redirects.Distinct().ToArray();
    }
}
