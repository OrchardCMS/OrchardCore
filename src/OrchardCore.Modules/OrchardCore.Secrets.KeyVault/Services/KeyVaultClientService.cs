using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets.KeyVault.Models;

namespace OrchardCore.Secrets.KeyVault.Services;

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

        // TODO test this. I think we delete secrets on set, so we would need to wait for delete to complete,
        // before updating it again, perhaps not, we can check this.

        // You only need to wait for completion if you want to purge or recover the secret.
        await operation.WaitForCompletionAsync();
    }
}
