using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Secrets.Recipes;

/// <summary>
/// This recipe step creates a set of secrets.
/// Supports both encrypted (with EncryptionKeyName) and unencrypted (environment variable) imports.
/// </summary>
public sealed class SecretsRecipeStep : NamedRecipeStepHandler
{
    private readonly ISecretManager _secretManager;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public SecretsRecipeStep(
        ISecretManager secretManager,
        ISecretEncryptionService encryptionService,
        IConfiguration configuration,
        ILogger<SecretsRecipeStep> logger,
        IStringLocalizer<SecretsRecipeStep> stringLocalizer)
        : base("Secrets")
    {
        _secretManager = secretManager;
        _encryptionService = encryptionService;
        _configuration = configuration;
        _logger = logger;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var secretsNode = context.Step["Secrets"];
        if (secretsNode == null)
        {
            return;
        }

        var encryptionKeyName = context.Step["EncryptionKeyName"]?.GetValue<string>();
        var hasEncryptionKey = !string.IsNullOrEmpty(encryptionKeyName);

        // Handle both object format (encrypted) and array format (legacy/unencrypted)
        if (secretsNode is JsonObject secretsObject)
        {
            await ImportFromObjectAsync(context, secretsObject, encryptionKeyName, hasEncryptionKey);
        }
        else if (secretsNode is JsonArray secretsArray)
        {
            await ImportFromArrayAsync(context, secretsArray);
        }
    }

    private async Task ImportFromObjectAsync(RecipeExecutionContext context, JsonObject secrets, string encryptionKeyName, bool hasEncryptionKey)
    {
        foreach (var (name, node) in secrets)
        {
            if (string.IsNullOrEmpty(name) || node is not JsonObject secretEntry)
            {
                continue;
            }

            var secretInfo = secretEntry["SecretInfo"]?.AsObject();
            var store = secretInfo?["Store"]?.GetValue<string>();
            var type = secretInfo?["Type"]?.GetValue<string>() ?? nameof(TextSecret);

            // Check if this is an encrypted secret
            var encryptedKey = secretEntry["EncryptedKey"]?.GetValue<string>();
            var encryptedData = secretEntry["EncryptedData"]?.GetValue<string>();
            var iv = secretEntry["IV"]?.GetValue<string>();

            ISecret secret = null;

            if (hasEncryptionKey && !string.IsNullOrEmpty(encryptedData))
            {
                // Decrypt the secret
                try
                {
                    var encrypted = new EncryptedSecretData
                    {
                        EncryptedKey = encryptedKey,
                        EncryptedData = encryptedData,
                        IV = iv,
                    };

                    secret = await _encryptionService.DecryptAsync(encrypted, encryptionKeyName, type);

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Secret '{SecretName}' decrypted successfully.", name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt secret '{SecretName}'.", name);
                    context.Errors.Add(S["Failed to decrypt secret '{0}': {1}", name, ex.Message]);
                    continue;
                }
            }
            else
            {
                // Try to get value from environment variable (fallback for unencrypted)
                var secretValue = GetSecretValueFromConfiguration(name);
                if (string.IsNullOrEmpty(secretValue))
                {
                    _logger.LogWarning("Secret '{SecretName}' has no encrypted data and no environment variable. Skipping.", name);
                    continue;
                }

                secret = new TextSecret { Text = secretValue };
            }

            await SaveSecretAsync(name, secret, store);
        }
    }

    private async Task ImportFromArrayAsync(RecipeExecutionContext context, JsonArray secrets)
    {
        // Legacy format: array of { Name, Store, Type, Value? }
        foreach (var token in secrets.OfType<JsonObject>())
        {
            var name = token["Name"]?.GetValue<string>();

            if (string.IsNullOrEmpty(name))
            {
                context.Errors.Add(S["Secret name is missing or empty. The secret will not be imported."]);
                continue;
            }

            var store = token["Store"]?.GetValue<string>();

            // Try to get the secret value from the recipe step first
            var secretValue = token["Value"]?.GetValue<string>();

            // If not provided in recipe, try to get from configuration (environment variables)
            if (string.IsNullOrEmpty(secretValue))
            {
                secretValue = GetSecretValueFromConfiguration(name);
            }

            if (string.IsNullOrEmpty(secretValue))
            {
                _logger.LogWarning("Secret '{SecretName}' has no value provided. Skipping.", name);
                continue;
            }

            var secret = new TextSecret { Text = secretValue };
            await SaveSecretAsync(name, secret, store);
        }
    }

    private string GetSecretValueFromConfiguration(string name)
    {
        // Look for secret value in configuration using pattern: OrchardCore_Secrets__{SecretName}
        var configKey = $"OrchardCore_Secrets__{name}";
        var value = _configuration[configKey];

        // Also try with colons for nested configuration
        if (string.IsNullOrEmpty(value))
        {
            configKey = $"OrchardCore:Secrets:{name}";
            value = _configuration[configKey];
        }

        return value;
    }

    private async Task SaveSecretAsync(string name, ISecret secret, string store)
    {
        if (!string.IsNullOrEmpty(store))
        {
            await _secretManager.SaveSecretAsync(name, secret, store);
        }
        else
        {
            await _secretManager.SaveSecretAsync(name, secret);
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Secret '{SecretName}' imported successfully.", name);
        }
    }
}

public sealed class SecretsRecipeStepModel
{
    public JsonArray Secrets { get; set; }
}
