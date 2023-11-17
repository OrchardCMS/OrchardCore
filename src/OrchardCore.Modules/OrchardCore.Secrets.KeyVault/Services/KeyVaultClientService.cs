using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets.Azure.Models;

namespace OrchardCore.Secrets.Azure.Services;

public class KeyVaultClientService
{
    private readonly SecretClient _client;
    private readonly string _prefix;

    public KeyVaultClientService(IOptions<SecretsKeyVaultOptions> options)
    {
        var keyVaultEndpointUri = new Uri($"https://{options.Value.KeyVaultName}.vault.azure.net");

        _client = new SecretClient(
            keyVaultEndpointUri,
            new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeVisualStudioCodeCredential = true }));

        _prefix = options.Value.Prefix;
    }

    public async Task<string> GetSecretAsync(string name)
    {
        if (!string.IsNullOrEmpty(_prefix))
        {
            name = $"{_prefix}{name}";
        }

        var secret = await _client.GetSecretAsync(name);

        return secret.Value.Value;
    }

    public async Task SetSecretAsync(string name, string secretValue)
    {
        if (!string.IsNullOrEmpty(_prefix))
        {
            name = $"{_prefix}{name}";
        }

        await _client.SetSecretAsync(name, secretValue);
    }

    public async Task RemoveSecretAsync(string name)
    {
        var operation = await _client.StartDeleteSecretAsync(name);

        // Purging on deletion is not supported, the retention period should be configured
        // on any key vault, knowing that the 'soft-delete' feature will be mandatory soon.

        // await operation.WaitForCompletionAsync();
        // await _client.PurgeDeletedSecretAsync(operation.Value.Name);
    }
}
