using OrchardCore.Indexing;

namespace OrchardCore.Search.OpenSearch.Core.Models;

public class OpenSearchConnectionOptions : ISearchProviderOptions
{
    /// <summary>
    /// The server url.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The server connection port.
    /// </summary>
    public int[] Ports { get; set; }

    /// <summary>
    /// The server connection type.
    /// </summary>
    public OpenSearchConnectionType ConnectionType { get; set; }

    /// <summary>
    /// The authentication type for the OpenSearch server.
    /// </summary>
    public OpenSearchAuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// The server ApiKey id when using ApiKey authentication type.
    /// </summary>
    public string ApiKeyId { get; set; }

    /// <summary>
    /// The server ApiKey when using ApiKey authentication type.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// The server Base64 encoded ApiKey (id:key) when using Base64ApiKey authentication type.
    /// </summary>
    public string Base64ApiKey { get; set; }

    /// <summary>
    /// The server Username when using Basic authentication type.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The server Password when using Basic authentication type.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Enable the Http Compression.
    /// </summary>
    public bool EnableHttpCompression { get; set; } = true;

    /// <summary>
    /// Enable the debug mode.
    /// </summary>
    public bool EnableDebugMode { get; set; }

    /// <summary>
    /// Whether to allow all SSL certificates (for development/self-signed certs).
    /// </summary>
    public bool AllowSslCertificateValidation { get; set; }

    /// <summary>
    /// Whether the configuration section exists.
    /// </summary>
    private bool _fileConfigurationExists { get; set; }

    public void SetFileConfigurationExists(bool fileConfigurationExists)
        => _fileConfigurationExists = fileConfigurationExists;

    private bool? _isConfigured;

    public bool ConfigurationExists()
    {
        if (!_isConfigured.HasValue)
        {
            _isConfigured = !string.IsNullOrEmpty(Url) &&
                Uri.TryCreate(Url, UriKind.Absolute, out var _);
        }

        return _isConfigured.Value;
    }

    public bool FileConfigurationExists()
        => _fileConfigurationExists;
}
