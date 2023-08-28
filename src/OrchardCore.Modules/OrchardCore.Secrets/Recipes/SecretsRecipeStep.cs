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
        if (!String.Equals(context.Name, "Secrets", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var secrets = (JObject)context.Step["Secrets"];
        foreach (var kvp in secrets)
        {
            var secretBinding = kvp.Value["SecretBinding"].ToObject<SecretBinding>();
            var secret = _secretService.CreateSecret(secretBinding.Type);

            // This will always be plaintext as decrypt has already operated on the secret.
            var plaintext = kvp.Value["Secret"]?.ToString();
            if (!String.IsNullOrEmpty(plaintext))
            {
                // Rehyrdate from plaintext to secret type.
                secret = JsonConvert.DeserializeObject(plaintext, secret.GetType()) as Secret;
            }

            await _secretService.RemoveSecretAsync(kvp.Key, secretBinding.Store);
            await _secretService.UpdateSecretAsync(kvp.Key, secretBinding, secret);
        }
    }
}
