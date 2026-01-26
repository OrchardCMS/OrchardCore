using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Secrets.Recipes;

/// <summary>
/// This recipe step creates a set of secrets.
/// </summary>
public sealed class SecretsRecipeStep : NamedRecipeStepHandler
{
    private readonly ISecretManager _secretManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public SecretsRecipeStep(
        ISecretManager secretManager,
        IConfiguration configuration,
        ILogger<SecretsRecipeStep> logger,
        IStringLocalizer<SecretsRecipeStep> stringLocalizer)
        : base("Secrets")
    {
        _secretManager = secretManager;
        _configuration = configuration;
        _logger = logger;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<SecretsRecipeStepModel>();

        if (model?.Secrets == null)
        {
            return;
        }

        foreach (var token in model.Secrets.Cast<JsonObject>())
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
                // Look for secret value in configuration using pattern: OrchardCore_Secrets__{SecretName}
                var configKey = $"OrchardCore_Secrets__{name}";
                secretValue = _configuration[configKey];

                // Also try with colons for nested configuration
                if (string.IsNullOrEmpty(secretValue))
                {
                    configKey = $"OrchardCore:Secrets:{name}";
                    secretValue = _configuration[configKey];
                }
            }

            if (string.IsNullOrEmpty(secretValue))
            {
                _logger.LogWarning("Secret '{SecretName}' has no value provided. Skipping.", name);
                continue;
            }

            var secret = new TextSecret { Text = secretValue };

            if (!string.IsNullOrEmpty(store))
            {
                await _secretManager.SaveSecretAsync(name, secret, store);
            }
            else
            {
                await _secretManager.SaveSecretAsync(name, secret);
            }

            _logger.LogInformation("Secret '{SecretName}' imported successfully.", name);
        }
    }
}

public sealed class SecretsRecipeStepModel
{
    public JsonArray Secrets { get; set; }
}
