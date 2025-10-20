using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;

namespace OrchardCore.Azure.Core;

public class AzureOptions
{
    public const string DefaultName = "Default";

    public AzureAuthenticationType AuthenticationType { get; set; }

    public string TenantId { get; set; }

    public string ClientId { get; set; }

    public string ApiKey { get; set; }

    public JsonObject Properties { get; set; }

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

    public virtual bool ConfigurationExists()
    {
        return AuthenticationType switch
        {
            AzureAuthenticationType.ApiKey => !string.IsNullOrEmpty(ApiKey),
            _ => true,
        };
    }

    public TokenCredential ToTokenCredential()
    {
        return AuthenticationType switch
        {
            AzureAuthenticationType.Default => new DefaultAzureCredential(),
            AzureAuthenticationType.ManagedIdentity => new ManagedIdentityCredential(ClientId),
            AzureAuthenticationType.AzureCli => new AzureCliCredential(),
            AzureAuthenticationType.VisualStudio => new VisualStudioCredential(),
            AzureAuthenticationType.VisualStudioCode => new VisualStudioCodeCredential(),
            AzureAuthenticationType.AzurePowerShell => new AzurePowerShellCredential(),
            AzureAuthenticationType.Environment => new EnvironmentCredential(),
            AzureAuthenticationType.InteractiveBrowser =>
                new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
                {
                    TenantId = TenantId,
                    ClientId = ClientId,
                }),
            AzureAuthenticationType.WorkloadIdentity => new WorkloadIdentityCredential(),
            _ => null, // ApiKey and unsupported types
        };
    }
}
