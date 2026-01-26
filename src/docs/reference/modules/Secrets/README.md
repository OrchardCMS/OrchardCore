# Secrets (`OrchardCore.Secrets`)

The Secrets module provides a secure way to store and manage sensitive data such as passwords, API keys, and certificates. It supports multiple secret stores including a built-in database store and Azure Key Vault.

## Features

- Secure storage of sensitive data using ASP.NET Core Data Protection
- Multiple secret store providers (database, Azure Key Vault)
- Admin UI for managing secrets
- Recipe support for importing secrets during setup
- Deployment step for exporting secret metadata

## Configuration

### Database Store (Default)

The database store is enabled by default when the Secrets module is enabled. It uses ASP.NET Core Data Protection to encrypt secrets before storing them in the database.

### Azure Key Vault Store

To use Azure Key Vault as a secret store, enable the `OrchardCore.Secrets.Azure` feature and configure it in your `appsettings.json`:

```json
{
  "OrchardCore": {
    "OrchardCore_Secrets_Azure": {
      "VaultUri": "https://your-vault.vault.azure.net/",
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

If running in Azure with Managed Identity, you can omit the `TenantId`, `ClientId`, and `ClientSecret` - the module will use `DefaultAzureCredential` which supports managed identities automatically.

## Using Secrets Programmatically

### Retrieving a Secret

```csharp
public class MyService
{
    private readonly ISecretManager _secretManager;

    public MyService(ISecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public async Task UseSecretAsync()
    {
        var secret = await _secretManager.GetSecretAsync<TextSecret>("MyApiKey");
        if (secret != null)
        {
            var apiKey = secret.Text;
            // Use the API key
        }
    }
}
```

### Saving a Secret

```csharp
var secret = new TextSecret { Text = "my-secret-value" };
await _secretManager.SaveSecretAsync("MyApiKey", secret);
```

### Saving to a Specific Store

```csharp
// Save to Azure Key Vault specifically
await _secretManager.SaveSecretAsync("MyApiKey", secret, "AzureKeyVault");
```

## Recipe Step

Secrets can be imported using a recipe step. Note that for security reasons, you should provide secret values via environment variables rather than directly in the recipe file.

### Recipe Format

```json
{
  "steps": [
    {
      "name": "Secrets",
      "Secrets": [
        {
          "Name": "SmtpPassword",
          "Store": "Database"
        },
        {
          "Name": "ApiKey",
          "Store": "AzureKeyVault"
        }
      ]
    }
  ]
}
```

### Providing Secret Values

Secret values should be provided via environment variables using one of these patterns:

- `OrchardCore_Secrets__SecretName` (double underscore)
- `OrchardCore:Secrets:SecretName` (colon-separated)

For example:
```bash
export OrchardCore_Secrets__SmtpPassword=mypassword
export OrchardCore_Secrets__ApiKey=myapikey
```

## Secret Types

### TextSecret

The most common secret type for storing string values like passwords and API keys.

```csharp
public class TextSecret : SecretBase
{
    public string Text { get; set; }
}
```

### RsaKeySecret

For storing RSA key material.

```csharp
public class RsaKeySecret : SecretBase
{
    public string KeyAsXml { get; set; }
    public bool IncludesPrivateKey { get; set; }
    public int KeySize { get; set; }
}
```

## Permissions

| Permission | Description |
|------------|-------------|
| `ManageSecrets` | Allows managing (create, edit, delete) secrets |
| `ViewSecrets` | Allows viewing the list of secrets (but not their values) |

## Implementing a Custom Secret Store

You can implement a custom secret store by implementing `ISecretStore`:

```csharp
public class MyCustomSecretStore : ISecretStore
{
    public string Name => "MyCustomStore";
    public bool IsReadOnly => false;

    public Task<T> GetSecretAsync<T>(string name) where T : class, ISecret
    {
        // Implementation
    }

    public Task SaveSecretAsync<T>(string name, T secret) where T : class, ISecret
    {
        // Implementation
    }

    public Task RemoveSecretAsync(string name)
    {
        // Implementation
    }

    public Task<IEnumerable<SecretInfo>> GetSecretInfosAsync()
    {
        // Implementation
    }
}
```

Register your store in `Startup.cs`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddSecretStore<MyCustomSecretStore>();
}
```

## Related Issues

- [#7137](https://github.com/OrchardCMS/OrchardCore/issues/7137) - Keyset does not exist crash with OpenID certificates
- [#13205](https://github.com/OrchardCMS/OrchardCore/issues/13205) - External storage for OpenID certificates
- [#5558](https://github.com/OrchardCMS/OrchardCore/issues/5558) - Missing deployment steps for settings with secrets
- [#3259](https://github.com/OrchardCMS/OrchardCore/issues/3259) - Certificate selection from Azure AppService in OpenID
