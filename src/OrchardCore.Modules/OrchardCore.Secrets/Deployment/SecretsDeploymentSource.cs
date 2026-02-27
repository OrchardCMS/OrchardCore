using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Secrets.Deployment;

public sealed class SecretsDeploymentSource : DeploymentSourceBase<SecretsDeploymentStep>
{
    private readonly ISecretManager _secretManager;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly ILogger _logger;

    public SecretsDeploymentSource(
        ISecretManager secretManager,
        ISecretEncryptionService encryptionService,
        ILogger<SecretsDeploymentSource> logger)
    {
        _secretManager = secretManager;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    protected override async Task ProcessAsync(SecretsDeploymentStep step, DeploymentPlanResult result)
    {
        var secretInfos = await _secretManager.GetSecretInfosAsync();
        var hasEncryptionKey = !string.IsNullOrEmpty(step.EncryptionKeyName);

        var secrets = new JsonObject();

        foreach (var info in secretInfos)
        {
            // Skip the encryption key itself to avoid circular dependency
            if (hasEncryptionKey && info.Name == step.EncryptionKeyName)
            {
                continue;
            }

            var secretEntry = new JsonObject
            {
                ["SecretInfo"] = new JsonObject
                {
                    ["Store"] = info.Store,
                    ["Type"] = info.Type,
                },
            };

            // If encryption key is specified, export encrypted values
            if (hasEncryptionKey)
            {
                try
                {
                    var secret = await GetSecretAsync(info.Name, info.Type);
                    if (secret != null)
                    {
                        var encrypted = await _encryptionService.EncryptAsync(secret, step.EncryptionKeyName);
                        secretEntry["EncryptedKey"] = encrypted.EncryptedKey;
                        secretEntry["EncryptedData"] = encrypted.EncryptedData;
                        secretEntry["IV"] = encrypted.IV;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to encrypt secret '{SecretName}'. It will be exported without a value.", info.Name);
                }
            }

            secrets[info.Name] = secretEntry;
        }

        var stepJson = new JsonObject
        {
            ["name"] = "Secrets",
            ["Secrets"] = secrets,
        };

        if (hasEncryptionKey)
        {
            stepJson["EncryptionKeyName"] = step.EncryptionKeyName;
        }

        result.Steps.Add(stepJson);
    }

    private async Task<ISecret> GetSecretAsync(string name, string type)
    {
        return type switch
        {
            nameof(TextSecret) => await _secretManager.GetSecretAsync<TextSecret>(name),
            nameof(RsaKeySecret) => await _secretManager.GetSecretAsync<RsaKeySecret>(name),
            nameof(X509Secret) => await _secretManager.GetSecretAsync<X509Secret>(name),
            _ => null,
        };
    }
}
