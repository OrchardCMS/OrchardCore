using System.ComponentModel.DataAnnotations;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Configuration;
using StackExchange.Redis;

namespace OrchardCore.Redis;

/// <summary>
/// Binds and validates the <see cref="RedisOptions"/> of a tenant.
/// </summary>
internal static class RedisOptionsExtension
{
    /// <summary>
    /// Returns an error when <see cref="RedisOptions.ConfigurationOptions"/> is <see langword="null"/>, which means that
    /// the configuration is missing.
    /// </summary>
    public static IEnumerable<ValidationResult> Validate(this RedisOptions options)
    {
        if (options.ConfigurationOptions is null)
        {
            yield return new ValidationResult("The 'Configuration' string is missing.");
        }
    }

    /// <summary>
    /// Binds the options from the <c>Redis</c> configuration section. A missing configuration leaves
    /// <see cref="RedisOptions.ConfigurationOptions"/> <see langword="null"/>, which fails the validation.
    /// </summary>
    /// <returns><see langword="false"/> if a configured value can't be bound, after logging the error.</returns>
    public static bool TryBindConfiguration(this RedisOptions options, IShellConfiguration shellConfiguration, ILogger logger)
    {
        // The 'OrchardCore_Redis' section is deprecated and will be removed in a future major version, use 'Redis' instead.
        var section = shellConfiguration.GetSectionCompat("Redis", "OrchardCore_Redis");

        var configuration = section["Configuration"];

        if (string.IsNullOrWhiteSpace(configuration))
        {
            return true;
        }

        try
        {
            var configurationOptions = ConfigurationOptions.Parse(configuration);

            if (section.GetValue("DisableCertificateVerification", false))
            {
                configurationOptions.CertificateValidation += IgnoreCertificateErrors;
            }

            var allowAdmin = section.GetValue<bool?>("AllowAdmin", null);
            if (allowAdmin.HasValue)
            {
                configurationOptions.AllowAdmin = allowAdmin.Value;
            }

            options.Configuration = configuration;
            options.ConfigurationOptions = configurationOptions;
            options.InstancePrefix = section["InstancePrefix"];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to configure the Redis options.");

            return false;
        }

        return true;
    }

    // Callback for accepting any certificate as long as it exists, while ignoring other SSL policy errors.
    // This allows the use of self-signed certificates on the Redis server.
    private static bool IgnoreCertificateErrors(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        => (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == 0;
}
