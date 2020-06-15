# Azure Key Vault (`OrchardCore.Azure.KeyVault`)
The Azure Key Vault configuration provider adds app configuration values from the Azure Key Vault in order to safeguard your cryptographic keys and secrets used by your app. It also contains custom override of the DefaultKeyVaultManger class that retrieves secrets from Azure Key Vault and translates ---
to an underscore (_)  and -- to a colon (:). Both underscores and colons are illegal characters in Azure KeyVault.

Example:
Key Vault Input: "OrchardCore--OrchardCore---Shells---Database--ConnectionString".
Output: "OrchardCore:OrchardCore_Shells_Database:ConnectionString".
See https://github.com/OrchardCMS/OrchardCore/issues/6359.

## Configuration
You'll need to specify the name of your Azure Key Vault and [register a service principle](https://docs.microsoft.com/en-us/azure/key-vault/general/group-permissions-for-apps) in Active Directory for accessing your key vault using an access control policy.

```json
"OrchardCore_Azure_KeyVault": {
    "KeyVaultName": "", // Set the name of your Azure Key Vault.
    "AzureADApplicationId": "", // Set the Azure AD Application Id
    "AzureADApplicationSecret": "" //Set the Azure AD Application Secret
}
```

!!! note
    You should **never check in your client secret into source control** as this defeats the purpose of using a key vault in the first place. Instead set your client secret as an environment variable on your machine, create a seperate azurekeyvault.json file and add it to your `.gitignore`, or use user secrets.

In the `Program.cs`, add `UseOrchardCoreAzureKeyVault()` to the Generic Host in `CreateHostBuilder()`.

```csharp
using OrchardCore.KeyVault.Azure;
public class Program
{
    public static Task Main(string[] args)
        => BuildHost(args).RunAsync();

    public static IHost BuildHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseOrchardCoreAzureKeyVault()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .Build();
}
```

