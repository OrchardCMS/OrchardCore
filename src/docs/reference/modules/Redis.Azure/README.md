# Redis Module (`OrchardCore.Redis.Azure`)

The **Redis Azure Module** depends on `OrchardCore.Redis` and enables your application to connect to a Redis cache hosted in Azure.
It supports authentication using Azure credentials, including:

* **Managed Identity**
* **Azure CLI (`AzureCli`)**
* **Default credentials (`Default`)**

---

## Configuration

Add a `Redis` credential entry to your Azure credentials configuration (e.g., `appsettings.json`):

```json
{
  "OrchardCore": {
    "Azure": {
      "Credentials": {
        "Redis": {
          "AuthenticationType": "ManagedIdentity", // Supported types: Default, ManagedIdentity, AzureCli, ApiKey
          "ClientId": "<your-client-id>",          // Optional: Only needed for user-assigned managed identity
          "Scopes": ["https://*.redis.cache.windows.net/.default"]
        }
      }
    }
  }
}
```

---

## Notes

* **Credential Name Requirement:** The Redis module **requires** the Azure credential to be named `Redis`. Other names will not be recognized.
* **AuthenticationType Options:**

  * **Default** – Uses `DefaultAzureCredential` (auto-detects available credentials).
  * **ManagedIdentity** – Uses a system-assigned or user-assigned managed identity.
  * **AzureCli** – Uses the currently logged-in Azure CLI session.
  * **ApiKey** – Uses an explicit API key (less common for Redis).
* **ClientId** is only required for **user-assigned managed identities**. If omitted, the system-assigned managed identity will be used.
* You can also include **custom properties** in the `Redis` credential entry. These can be accessed at runtime using the `GetProperty<T>(propertyName)` method.
