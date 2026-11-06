using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Secrets.Services;

/// <summary>
/// Post-configures OpenIddict server options to use secrets for signing and encryption keys.
/// </summary>
public sealed class OpenIdSecretsOptionsConfiguration : IPostConfigureOptions<OpenIddictServerOptions>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public OpenIdSecretsOptionsConfiguration(
        ISiteService siteService,
        ISecretManager secretManager,
        ILogger<OpenIdSecretsOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void PostConfigure(string name, OpenIddictServerOptions options)
    {
        var settings = _siteService.GetSettings<OpenIdSecretSettings>();

        // Configure signing key from secrets
        if (!string.IsNullOrWhiteSpace(settings?.SigningKeySecretName))
        {
            var signingKey = _secretManager.GetSecurityKeyAsync(settings.SigningKeySecretName, _logger).GetAwaiter().GetResult();
            if (signingKey != null)
            {
                // Insert at the beginning so it takes precedence
                options.SigningCredentials.Insert(0, new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256));
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("OpenID signing key loaded from secret '{SecretName}'.", settings.SigningKeySecretName);
                }
            }
        }

        // Configure encryption key from secrets
        if (!string.IsNullOrWhiteSpace(settings?.EncryptionKeySecretName))
        {
            var encryptionKey = _secretManager.GetSecurityKeyAsync(settings.EncryptionKeySecretName, _logger).GetAwaiter().GetResult();
            if (encryptionKey != null)
            {
                // Insert at the beginning so it takes precedence
                options.EncryptionCredentials.Insert(0, new EncryptingCredentials(
                    encryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512));
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("OpenID encryption key loaded from secret '{SecretName}'.", settings.EncryptionKeySecretName);
                }
            }
        }
    }
}
