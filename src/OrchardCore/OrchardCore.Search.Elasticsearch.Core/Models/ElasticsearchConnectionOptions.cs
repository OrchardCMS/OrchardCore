using OrchardCore.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticsearchConnectionOptions : ISearchProviderOptions
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
    public ElasticsearchConnectionType ConnectionType { get; set; }

    /// <summary>
    /// The authentication type for the Elasticsearch server.
    /// </summary>
    public ElasticsearchAuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// The server ApiKey when using ApiKey authentication type.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// The server Base64 encoded ApiKey when using Base64ApiKey authentication type.
    /// </summary>
    public string Base64ApiKey { get; set; }

    /// <summary>
    /// The Elasticsearch cloud service CloudId. This is only used when the ConnectionType is CloudConnectionPool.
    /// </summary>
    public string CloudId { get; set; }

    /// <summary>
    /// The server Username when using Basic authentication type.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The server Password when using Basic authentication type.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The server's KeyId when using KeyIdAndKey authentication type.
    /// </summary>
    public string KeyId { get; set; }

    /// <summary>
    /// The server's Key when using KeyIdAndKey authentication type.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Enable the Http Compression.
    /// </summary>
    public bool EnableHttpCompression { get; set; } = true;

    /// <summary>
    /// Enable the debug mode.
    /// </summary>
    public bool EnableDebugMode { get; set; }

    /// <summary>
    /// The server Certificate Fingerprint.
    /// </summary>
    public string CertificateFingerprint { get; set; }

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
