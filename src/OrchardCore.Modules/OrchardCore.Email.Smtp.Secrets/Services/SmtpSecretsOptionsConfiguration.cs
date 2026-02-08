using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Secrets.Services;

/// <summary>
/// Configures SmtpOptions to use secrets for the password when configured.
/// </summary>
public sealed class SmtpSecretsOptionsConfiguration : IPostConfigureOptions<SmtpOptions>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public SmtpSecretsOptionsConfiguration(
        ISiteService siteService,
        ISecretManager secretManager,
        ILogger<SmtpSecretsOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void PostConfigure(string name, SmtpOptions options)
    {
        var settings = _siteService.GetSettings<SmtpSecretSettings>();

        if (string.IsNullOrWhiteSpace(settings?.PasswordSecretName))
        {
            return;
        }

        try
        {
            // Resolve password from secrets
            var secret = _secretManager.GetSecretAsync<TextSecret>(settings.PasswordSecretName)
                .GetAwaiter()
                .GetResult();

            if (secret != null && !string.IsNullOrWhiteSpace(secret.Text))
            {
                options.Password = secret.Text;
                _logger.LogDebug("SMTP password loaded from secret '{SecretName}'.", settings.PasswordSecretName);
            }
            else
            {
                _logger.LogWarning("SMTP password secret '{SecretName}' was not found or is empty.", settings.PasswordSecretName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SMTP password from secret '{SecretName}'.", settings.PasswordSecretName);
        }
    }
}
