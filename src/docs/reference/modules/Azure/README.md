# Azure Module (`OrchardCore.Azure`)

The **Azure Module** provides a centralized way to manage all Azure connections and credentials within Orchard Core.
Other modules can reference these credentials by name, allowing for clean configuration management and consistent authentication across tenants or services.

---

## Configuration

You can define Azure credentials in a settings provider such as `appsettings.json`.
Below is an example configuration:

```json
{
  "OrchardCore": {
    "Azure": {
      "Credentials": {
        "my-default": {
          "AuthenticationType": "Default"
        },
        "my-identity": {
          "AuthenticationType": "ManagedIdentity",
          "ClientId": "<your-client-id>" // Optional: If omitted, the system-assigned managed identity will be used.
        },
        "SearchAI": {
          "Host": "<your-host-endpoint>",
          "AuthenticationType": "ApiKey",
          "ApiKey": "<your-api-key>",
          "DeploymentName": "<your-deployment-name>"
        },
        "Redis": {
          "Alias": "my-default" // References another credentials entry (in this case, "my-default").
        }
      }
    }
  }
}
```

---

## Explanation

* **`my-default`**
  Defines a credential that uses Azure CLI authentication, managed identity, or default Azure authentication depending on your environment.
  You can choose any authentication type here: `Default`, `ManagedIdentity`, `AzureCli`, or `ApiKey`.

* **`my-identity`, `SearchAI`, `Redis`**
  Arbitrary names chosen by the user to identify different credentials.
  You can define as many credential entries as needed, each with a unique name.
  Other modules can reference them by name.

* **`Alias` property**
  Allows a credential entry to reference another defined credential by name.
  For example, the `Redis` entry reuses the `my-default` settings.

* **Custom Properties**
  Any credential entry can include additional, user-defined properties beyond the standard fields.
  These can be accessed at runtime using the `GetProperty<T>(propertyName)` method.
  Example:

  ```csharp
  var endpoint = redisOptions.GetProperty<string>("Endpoint");
  ```
---

## Notes

* Credential names are **fully customizable** (e.g., `SearchAI`, `Redis`, `StorageAccount`).
* If a module does not specify which credential to use, it may fall back to a default behavior defined by that module (not enforced by `OrchardCore.Azure`).
* The `Alias` property enables credential reuse without duplicating configuration values.

---

## Obtaining Credentials

To obtain credentials for a specific Azure service, you can use the `AzureOptions` provider and `TokenCredential`:

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
        var redisOptions = _options.Get("Redis");

        // Access a custom property called "Scopes" defined in the configuration
        var scopes = redisOptions.GetProperty<string[]>("Scopes");

        if (scopes is null || scopes.Length == 0)
        {
            throw new InvalidOperationException("Scopes must be defined in the configuration for the Redis credential.");
        }

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
