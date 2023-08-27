using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Secrets.Recipes;

public class SecretsRecipeStep : IRecipeStepHandler
{
    private readonly ISecretCoordinator _secretCoordinator;

    public SecretsRecipeStep(ISecretCoordinator secretCoordinator) => _secretCoordinator = secretCoordinator;

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!String.Equals(context.Name, "Secrets", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var secrets = (JObject)context.Step["Secrets"];
        foreach (var kvp in secrets)
        {
            var secretBinding = kvp.Value["SecretBinding"].ToObject<SecretBinding>();
            var secret = _secretCoordinator.CreateSecret(secretBinding.Type);

            // This will always be plaintext as decrypt has already operated on the secret.
            var plaintext = kvp.Value["Secret"]?.ToString();
            if (!String.IsNullOrEmpty(plaintext))
            {
                // Rehyrdate from plaintext to secret type.
                secret = JsonConvert.DeserializeObject(plaintext, secret.GetType()) as Secret;
            }

            await _secretCoordinator.RemoveSecretAsync(kvp.Key, secretBinding.Store);
            await _secretCoordinator.UpdateSecretAsync(kvp.Key, secretBinding, secret);
        }
    }
}
