# Azure Module (`OrchardCore.Azure`)

The **Azure Module** provides a centralized way to manage all Azure connections and credentials within Orchard Core.
Other modules can reference these credentials **by name**, allowing for consistent authentication and clean configuration management across tenants or services.

!!! note 
    The credential named `Default` is a special case:
     - It is automatically used whenever a service does not explicitly specify a credential name.
     - It is also returned if you resolve `IOptions<AzureOptions>` without a specific key.

---

## Configuration

You can define Azure credentials in a settings provider such as `appsettings.json`.
Below is an example configuration:

```json
{
  "OrchardCore": {
    "Azure": {
      "Credentials": {
        "Default": {
          "AuthenticationType": "ManagedIdentity",
          "ClientId": "<your-client-id>" // Optional: If omitted, the system-assigned managed identity will be used.
        },
        "AnyAuthentication": {
          "AuthenticationType": "Default" // Uses the special Default credential
        },
        "SearchAI": {
          "Host": "<your-host-endpoint>",
          "AuthenticationType": "ApiKey",
          "ApiKey": "<your-api-key>",
          "DeploymentName": "<your-deployment-name>"
        },
        "Redis": {
          "Alias": "my-default" // References another credentials entry (here, "my-default")
        }
      }
    }
  }
}
```

---

## Notes

* **Credential names are fully customizable** (e.g., `SearchAI`, `Redis`, `StorageAccount`).
* If a module does not specify which credential to use, it **may fall back to `"Default"`**.
* The `Alias` property allows credential reuse without duplicating configuration values.
* You can define **custom properties** within any credential, which can be retrieved programmatically using `GetProperty<T>(propertyName)`.

---

## Obtaining Credentials

You can obtain credentials for a specific Azure service using the `AzureOptions` provider and `TokenCredential`:

```csharp
public sealed class Example
{
    private readonly IOptionsMonitor<AzureOptions> _options;

    public Example(IOptionsMonitor<AzureOptions> options)
    {
        _options = options;
    }

    public async Task<string> GetTokenAsync()
    {
        // Get the named credential (falls back to "Default" if not specified)
        var redisOptions = _options.Get("Redis");

        // Access a custom property called "Scopes" defined in the configuration
        var scopes = redisOptions.GetProperty<string[]>("Scopes");

        if (scopes is null || scopes.Length == 0)
            throw new InvalidOperationException("Scopes must be defined in the configuration for the Redis credential.");

        // Create the appropriate credential based on the authentication type
        TokenCredential credential = redisOptions.AuthenticationType switch
        {
            AzureAuthenticationType.Default => new DefaultAzureCredential(),
            AzureAuthenticationType.ManagedIdentity => new ManagedIdentityCredential(),
            AzureAuthenticationType.AzureCli => new AzureCliCredential(),
            _ => throw new NotSupportedException($"Authentication type {redisOptions.AuthenticationType} is not supported")
        };

        var result = await credential.GetTokenAsync(new TokenRequestContext(scopes), CancellationToken.None);

        return result.Token;
    }
}
```
