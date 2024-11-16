namespace OrchardCore.Search.Elasticsearch.Core.Models;

public class ElasticsearchConnectionOptions
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
    /// The Elasticsearch cloud service CloudId.
    /// </summary>
    public string CloudId { get; set; }

    /// <summary>
    /// The server Username.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The server Password.
    /// </summary>
    public string Password { get; set; }

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
