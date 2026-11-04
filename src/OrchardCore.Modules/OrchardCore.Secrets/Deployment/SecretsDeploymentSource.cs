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
                    ["Description"] = info.Description,
                    ["ExpiresUtc"] = info.ExpiresUtc,
                },
            };

            // If encryption key is specified, export encrypted values
            if (hasEncryptionKey)
            {
                try
                {
                    var secret = await _secretManager.GetSecretAsync<ISecret>(info.Name, info.Store)
                        ?? throw new InvalidOperationException($"Secret '{info.Name}' could not be read from '{info.Store}'.");
                    var encrypted = await _encryptionService.EncryptAsync(secret, step.EncryptionKeyName, info);
                    secretEntry["Version"] = encrypted.Version;
                    secretEntry["Tag"] = encrypted.Tag;
                    secretEntry["EncryptedKey"] = encrypted.EncryptedKey;
                    secretEntry["EncryptedData"] = encrypted.EncryptedData;
                    secretEntry["IV"] = encrypted.IV;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to encrypt secret '{SecretName}'.", info.Name);
                    throw;
                }
            }

            if (secrets.ContainsKey(info.Name))
            {
                throw new InvalidOperationException($"Secret name '{info.Name}' exists in multiple stores. Resolve the duplicate before exporting.");
            }

            secrets.Add(info.Name, secretEntry);
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
}
