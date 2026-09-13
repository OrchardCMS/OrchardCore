using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId;

/// <summary>
/// Configures the Pomi native client and its device authorization flow.
/// </summary>
public sealed class RemoteManagementCliConfigurationService : IRemoteManagementCliConfigurationService
{
    private const string ClientId = "orchardcore-cli";
    private const string ManagementScope = "orchardcore.management";

    private static readonly Uri RedirectUri = new("http://127.0.0.1/callback");
    private static readonly Uri PostLogoutRedirectUri = new("http://127.0.0.1/");

    private static readonly string[] RequiredPermissions =
    [
        OpenIddictConstants.Permissions.Endpoints.Authorization,
        OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization,
        OpenIddictConstants.Permissions.Endpoints.EndSession,
        OpenIddictConstants.Permissions.Endpoints.Revocation,
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.GrantTypes.DeviceCode,
        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
        OpenIddictConstants.Permissions.ResponseTypes.Code,
        OpenIddictConstants.Permissions.Prefixes.Scope + "email",
        OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
        OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
        OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
        OpenIddictConstants.Permissions.Prefixes.Scope + ManagementScope,
    ];

    private readonly IOpenIdApplicationManager _applicationManager;
    private readonly IOpenIdServerService _serverService;

    public RemoteManagementCliConfigurationService(IOpenIdApplicationManager applicationManager, IOpenIdServerService serverService)
    {
        _applicationManager = applicationManager;
        _serverService = serverService;
    }

    /// <inheritdoc />
    public async Task<bool> IsConfiguredAsync()
    {
        var application = await _applicationManager.FindByClientIdAsync(ClientId);
        var applicationConfigured = false;
        if (application is not null)
        {
            var permissions = await _applicationManager.GetPermissionsAsync(application);
            var requirements = await _applicationManager.GetRequirementsAsync(application);
            var redirectUris = await _applicationManager.GetRedirectUrisAsync(application);
            var postLogoutRedirectUris = await _applicationManager.GetPostLogoutRedirectUrisAsync(application);

            applicationConfigured =
                string.Equals(await _applicationManager.GetApplicationTypeAsync(application), OpenIddictConstants.ApplicationTypes.Native, StringComparison.Ordinal) &&
                string.Equals(await _applicationManager.GetClientTypeAsync(application), OpenIddictConstants.ClientTypes.Public, StringComparison.Ordinal) &&
                RequiredPermissions.All(permissions.Contains) &&
                requirements.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange) &&
                redirectUris.Contains(RedirectUri.AbsoluteUri, StringComparer.Ordinal) &&
                postLogoutRedirectUris.Contains(PostLogoutRedirectUri.AbsoluteUri, StringComparer.Ordinal);
        }

        var server = await _serverService.GetSettingsAsync();
        return applicationConfigured && server.AllowDeviceAuthorizationFlow &&
            server.DeviceAuthorizationEndpointPath == "/connect/device" &&
            server.EndUserVerificationEndpointPath == "/connect/verify";
    }

    /// <inheritdoc />
    public async Task ConfigureAsync()
    {
        var server = await _serverService.LoadSettingsAsync();
        server.DeviceAuthorizationEndpointPath = "/connect/device";
        server.EndUserVerificationEndpointPath = "/connect/verify";
        server.AllowDeviceAuthorizationFlow = true;
        var errors = (await _serverService.ValidateSettingsAsync(server))
            .Where(result => result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            .Select(result => result.ErrorMessage).ToArray();
        if (errors.Length > 0)
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException(string.Join(" ", errors));
        }

        await _serverService.UpdateSettingsAsync(server);
        await ConfigureApplicationAsync();
    }

    private async Task ConfigureApplicationAsync()
    {
        var application = await _applicationManager.FindByClientIdAsync(ClientId);
        var descriptor = new OpenIdApplicationDescriptor();

        if (application is not null)
        {
            await _applicationManager.PopulateAsync(descriptor, application);
        }

        descriptor.ClientId = ClientId;
        descriptor.DisplayName = "Pomi CLI";
        descriptor.ApplicationType = OpenIddictConstants.ApplicationTypes.Native;
        descriptor.ClientType = OpenIddictConstants.ClientTypes.Public;
        descriptor.ConsentType = OpenIddictConstants.ConsentTypes.Explicit;
        descriptor.ClientSecret = null;
        descriptor.RedirectUris.Add(RedirectUri);
        descriptor.PostLogoutRedirectUris.Add(PostLogoutRedirectUri);
        descriptor.Permissions.UnionWith(RequiredPermissions);
        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        if (application is null)
        {
            await _applicationManager.CreateAsync(descriptor);
        }
        else
        {
            await _applicationManager.UpdateAsync(application, descriptor);
        }
    }

}
