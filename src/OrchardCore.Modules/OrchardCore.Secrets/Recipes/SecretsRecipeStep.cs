using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Recipes;

public class SecretsRecipeStep : IRecipeStepHandler
{
    private readonly ISecretService _secretService;

    public SecretsRecipeStep(ISecretService secretService) => _secretService = secretService;

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "Secrets", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var secrets = (JObject)context.Step["Secrets"];
        foreach (var kvp in secrets)
        {
            var binding = kvp.Value["SecretBinding"].ToObject<SecretBinding>();
            var secret = _secretService.CreateSecret(binding.Type);

            // This will always be plaintext as decrypt has already operated on the secret.
            var plaintext = kvp.Value["Secret"]?.ToString();
            if (!string.IsNullOrEmpty(plaintext))
            {
                // Rehydrate from plaintext to secret type.
                secret = JsonConvert.DeserializeObject(plaintext, secret.GetType()) as SecretBase;
            }

            // Binding names are deduced from their key.
            binding.Name = kvp.Key;

            await _secretService.RemoveSecretAsync(binding);
            await _secretService.UpdateSecretAsync(binding, secret);
        }
    }
}
