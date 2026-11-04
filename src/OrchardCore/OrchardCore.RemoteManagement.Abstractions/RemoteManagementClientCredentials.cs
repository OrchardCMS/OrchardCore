using System.Security.Cryptography;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Credentials returned once to the caller provisioning an administrative application.
/// </summary>
public sealed class RemoteManagementClientCredentials
{
    /// <summary>
    /// Gets or sets the unique application identifier.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the application secret. Do not log or persist it in ordinary configuration.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Generates credentials for a new application, independent of any administrator user.
    /// </summary>
    public static RemoteManagementClientCredentials Generate() => new()
    {
        ClientId = "pomi-" + Guid.NewGuid().ToString("N"),
        ClientSecret = GenerateSecret(),
    };
    /// <summary>
    /// Generates a 256-bit random shared secret for provisioning or credential replacement.
    /// </summary>
    public static string GenerateSecret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
