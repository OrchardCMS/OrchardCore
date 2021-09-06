# Azure Key Vault (`OrchardCore.Azure.KeyVault`)

The Azure Key Vault configuration provider adds app configuration values from the Azure Key Vault in order to safeguard your cryptographic keys and secrets used by your app. It also contains custom override of the DefaultKeyVaultManager class that retrieves secrets from Azure Key Vault and translates --- to an underscore (_)  and -- to a colon (:). Both underscores and colons are illegal characters in Azure KeyVault.

Example:
Key Vault Input: "OrchardCore--OrchardCore---Shells---Database--ConnectionString".
Output: "OrchardCore:OrchardCore_Shells_Database:ConnectionString".
See https://github.com/OrchardCMS/OrchardCore/issues/6359.

## Authenticating with Azure Key Vault
By default, the Azure Key Vault configuration provider uses the [Azure Identity library](https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/identity/Azure.Identity/README.md) for Azure Active Directory token authentication support across the Azure SDK. At this time, the OrchardCore.Azure.KeyVault only supports the DefaultAzureCredential setting, which is appropriate for most scenarios where the application is intended to be run in Azure.

When debugging or executing locally, developers have several options for authenticating with Azure Key Vault. To authenticate in Visual Studio select the Tools > Options menu to launch the Options dialog. Then navigate to the Azure Service Authentication options to sign in with your Azure Active Directory account. Developers using Visual Studio Code can use the [Azure Account Extension], to authenticate via the IDE. 

## Configuration
In addition, you will need to specify the name of your Azure Key Vault and optionally a reload interval in seconds.
```json
"OrchardCore_KeyVault_Azure": {
    "KeyVaultName": "", // Set the name of your Azure Key Vault.
    "ReloadInterval": "" // Optional, sets the timespan to wait between attempts at polling the Azure KeyVault for changes. Leave blank to disable reloading.
}
```

In the `Program.cs`, add `AddOrchardCoreAzureKeyVault()` to the Generic Host in `CreateHostBuilder()`.

```csharp
using OrchardCore.Azure.KeyVault.Extensions;;
public class Program
{
    public static Task Main(string[] args)
        => BuildHost(args).RunAsync();

    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddOrchardCoreAzureKeyVault()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .Build();
}
```

