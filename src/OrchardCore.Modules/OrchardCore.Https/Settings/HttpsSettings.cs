using System.Text.Json.Serialization;

namespace OrchardCore.Https.Settings;

/// <summary>
/// Stores the HTTPS site settings for a tenant.
/// </summary>
public class HttpsSettings
{
    /// <summary>
    /// Gets or sets the legacy HSTS toggle retained only so stored settings can be migrated to <see cref="StrictTransportSecurityMode"/>.
    /// </summary>
    [JsonIgnore]
    [Obsolete("This property is obsolete and will be removed in future releases. Use StrictTransportSecurityMode instead.")]
    public bool EnableStrictTransportSecurity { get; set; }

    /// <summary>
    /// Gets or sets how HTTP Strict Transport Security (HSTS) is applied.
    /// </summary>
    public HttpStrictTransportSecurityMode StrictTransportSecurityMode { get; set; } = HttpStrictTransportSecurityMode.Disabled;

    /// <summary>
    /// Gets or sets a value indicating whether all requests should be redirected to HTTPS.
    /// </summary>
    public bool RequireHttps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether HTTPS redirects should use a permanent redirect status code.
    /// </summary>
    public bool RequireHttpsPermanent { get; set; }

    /// <summary>
    /// Gets or sets the HTTPS port to use for redirection when it cannot be inferred automatically.
    /// </summary>
    public int? SslPort { get; set; }
}
