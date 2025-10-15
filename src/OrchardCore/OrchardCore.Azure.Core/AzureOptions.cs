using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Azure.Core;

public class AzureOptions
{
    public AzureAuthenticationType AuthenticationType { get; set; }

    public string TenantId { get; set; }

    public string ClientId { get; set; }

    public string ApiKey { get; set; }

    internal JsonObject Properties { get; set; }

    public T GetProperty<T>(string propertyName)
    {
        if (Properties is not null && Properties.TryGetPropertyValue(propertyName, out var propertyValue) && propertyValue is not null)
        {
            try
            {
                return propertyValue.Deserialize<T>();
            }
            catch { }
        }

        return default;
    }

    public bool ConfigurationExists()
    {
        return AuthenticationType switch
        {
            AzureAuthenticationType.ApiKey => !string.IsNullOrEmpty(ApiKey),
            _ => true,
        };
    }
}
