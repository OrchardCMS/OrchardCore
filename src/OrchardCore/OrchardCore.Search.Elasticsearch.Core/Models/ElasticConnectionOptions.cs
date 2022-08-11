namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    public class ElasticConnectionOptions
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
        /// The server connection type
        /// </summary>
        public string ConnectionType { get; set; }

        /// <summary>
        /// The Elasticsearch cloud service CloudId
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
        /// Enables compatibility mode for Elasticsearch 8.x
        /// </summary>
        public bool EnableApiVersioningHeader { get; set; } = false;

        /// <summary>
        /// Whether the configuration section exists.
        /// </summary>
        public bool ConfigurationExists { get; set; }
    }
}
