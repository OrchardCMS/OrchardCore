using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Secrets;
using OrchardCore.OpenId.Settings;
using OrchardCore.Secrets;

namespace OrchardCore.OpenId.Services;

/// <summary>
/// Creates the OpenIddict client registrations from the Orchard Core client settings.
/// </summary>
public sealed class OpenIdClientRegistrationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public OpenIdClientRegistrationFactory(
        IServiceProvider serviceProvider,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<OpenIdClientRegistrationFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    /// <summary>
    /// Creates a client registration from client settings.
    /// </summary>
    /// <param name="settings">The validated settings of the registration.</param>
    /// <param name="providerName">The provider name, also the authentication scheme, of the registration.</param>
    /// <param name="defaultCallbackPath">The path of the redirection endpoint when the settings don't set one.</param>
    /// <param name="defaultSignedOutCallbackPath">The path of the post-logout redirection endpoint when the settings don't set one.</param>
    public async Task<OpenIddictClientRegistration> CreateAsync(
        OpenIdClientSettings settings,
        string providerName,
        string defaultCallbackPath,
        string defaultSignedOutCallbackPath)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        var registration = new OpenIddictClientRegistration
        {
            Issuer = settings.Authority,
            ClientId = settings.ClientId,
            RedirectUri = CreateRelativeUri(settings.CallbackPath, defaultCallbackPath),
            PostLogoutRedirectUri = CreateRelativeUri(settings.SignedOutCallbackPath, defaultSignedOutCallbackPath),
            ProviderName = providerName,
            ProviderDisplayName = string.IsNullOrWhiteSpace(settings.DisplayName) ? providerName : settings.DisplayName,
            Properties =
            {
                [OpenIdConstants.Client.SettingsProperty] = settings,
            },
        };

        if (!string.IsNullOrEmpty(settings.ResponseMode))
        {
            registration.ResponseModes.Add(settings.ResponseMode);
        }

        if (!string.IsNullOrEmpty(settings.ResponseType))
        {
            registration.ResponseTypes.Add(settings.ResponseType);
        }

        if (settings.Scopes is not null)
        {
            registration.Scopes.UnionWith(settings.Scopes.Where(scope => !string.IsNullOrWhiteSpace(scope)));
        }

        // Note: the protector purpose is the one used by the settings editors to protect the client secret.
        registration.ClientSecret = await _serviceProvider.GetSecretValueAsync(
            settings.ClientSecretSecretName,
            settings.ClientSecret,
            _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration)),
            _logger);

        // When a signing key is stored in secrets, the client authenticates with signed client assertions
        // (private_key_jwt) when the identity provider supports them, and with the client secret otherwise.
        if (!string.IsNullOrWhiteSpace(settings.SigningKeySecretName))
        {
            var secretManager = _serviceProvider.GetService<ISecretManager>();

            if (secretManager is null)
            {
                _logger.LogWarning("The secret '{SecretName}' is referenced, but the Secrets feature is not enabled.", settings.SigningKeySecretName);
            }
            else if (await secretManager.GetSecurityKeyAsync(settings.SigningKeySecretName, _logger) is SecurityKey key)
            {
                registration.SigningCredentials.Add(new SigningCredentials(key, SecurityAlgorithms.RsaSha256));
            }
        }

        return registration;
    }

    // Note: endpoint paths are stored with a leading '/', like ASP.NET Core paths, but a root-relative URI would be
    // resolved against the host and break path-based tenants, so the paths are made relative to the tenant.
    internal static Uri CreateRelativeUri(string path, string defaultPath)
        => new((string.IsNullOrWhiteSpace(path) ? defaultPath : path).TrimStart('/'), UriKind.Relative);
}
