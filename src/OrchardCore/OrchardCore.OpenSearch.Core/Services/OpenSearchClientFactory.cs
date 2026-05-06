using System.Text;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using OpenSearch.Net;
using OrchardCore.OpenSearch.Core.Models;

namespace OrchardCore.OpenSearch.Core.Services;

public sealed class OpenSearchClientFactory : IOpenSearchClientFactory
{
    private readonly ILogger _logger;

    public OpenSearchClientFactory(ILogger<OpenSearchClientFactory> logger)
    {
        _logger = logger;
    }

    public OpenSearchClient Create(OpenSearchConnectionOptions configuration)
    {
        IConnectionPool connectionPool = configuration.ConnectionType switch
        {
            OpenSearchConnectionType.StaticConnectionPool => new StaticConnectionPool(GetNodeUris(configuration)),
            OpenSearchConnectionType.SniffingConnectionPool => new SniffingConnectionPool(GetNodeUris(configuration)),
            OpenSearchConnectionType.StickyConnectionPool => new StickyConnectionPool(GetNodeUris(configuration)),
            _ => new SingleNodeConnectionPool(GetNodeUris(configuration).FirstOrDefault() ?? new Uri("http://localhost:9200")),
        };

        var settings = new ConnectionSettings(connectionPool);

        switch (configuration.AuthenticationType)
        {
            case OpenSearchAuthenticationType.ApiKey:
                settings = settings.ApiKeyAuthentication(configuration.ApiKeyId, configuration.ApiKey);
                break;

            case OpenSearchAuthenticationType.Base64ApiKey:
                if (!string.IsNullOrEmpty(configuration.Base64ApiKey))
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(configuration.Base64ApiKey));
                    var parts = decoded.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        settings = settings.ApiKeyAuthentication(parts[0], parts[1]);
                    }
                }
                break;

            default:
                settings = settings.BasicAuthentication(configuration.Username, configuration.Password);
                break;
        }

        if (configuration.AllowSslCertificateValidation)
        {
            settings = settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
        }

        if (configuration.EnableDebugMode)
        {
            settings = settings
                .DisableDirectStreaming()
                .OnRequestCompleted(details =>
                {
                    if (!_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    {
                        return;
                    }

                    if (details.RequestBodyInBytes != null)
                    {
                        _logger.LogDebug("OpenSearch request is: {Request}.", Encoding.UTF8.GetString(details.RequestBodyInBytes));
                    }

                    if (details.ResponseBodyInBytes != null)
                    {
                        _logger.LogDebug("OpenSearch response is: {Response}.", Encoding.UTF8.GetString(details.ResponseBodyInBytes));
                    }
                });
        }

        if (configuration.EnableHttpCompression)
        {
            settings = settings.EnableHttpCompression();
        }

        return new OpenSearchClient(settings);
    }

    private static IEnumerable<Uri> GetNodeUris(OpenSearchConnectionOptions configuration)
    {
        if (string.IsNullOrEmpty(configuration.Url))
        {
            return [];
        }

        return configuration.Ports.Select(port => new Uri($"{configuration.Url}:{port}")).Distinct();
    }
}
