using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Secrets;

public static class SecretConfigurationExtensions
{
    public const string LoggerCategory = "OrchardCore.Secrets.Configuration";
    public static readonly EventId LegacySecretConfigurationEvent = new(8100, "LegacySecretConfiguration");

    /// <summary>
    /// Warns about configured legacy credentials without logging their values.
    /// </summary>
    public static void WarnIfLegacySecretConfigured(
        this IConfigurationSection section,
        ILogger logger,
        string tenantName,
        string integration,
        params string[] credentialKeys)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(credentialKeys);

        if (!logger.IsEnabled(LogLevel.Warning))
        {
            return;
        }

        foreach (var key in credentialKeys)
        {
            if (!string.IsNullOrWhiteSpace(section[key]))
            {
                logger.LogWarning(LegacySecretConfigurationEvent,
                    "LegacySecretConfiguration: Tenant '{TenantName}' loads a legacy credential for '{Integration}' from configuration key '{ConfigurationKey}'. Move it to the Secrets module and remove the configuration value. Direct credential configuration may be removed in a future version.",
                    tenantName, integration, section.GetSection(key).Path);
            }
        }
    }
}
