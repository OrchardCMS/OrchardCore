namespace OrchardCore.Https.Settings;

/// <summary>
/// Determines how HTTP Strict Transport Security (HSTS) is applied for the tenant.
/// </summary>
public enum HttpStrictTransportSecurityMode
{
    /// <summary>
    /// Always disables HSTS regardless of the current environment.
    /// </summary>
    Disabled,

    /// <summary>
    /// Always enables HSTS regardless of the current environment.
    /// </summary>
    Enabled,

    /// <summary>
    /// Enables HSTS in the Production environment and disables it in other environments.
    /// </summary>
    FromConfiguration,
}
