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
    private readonly ISecretProtectionProvider _protectionProvider;

    public SecretsRecipeStep(ISecretService secretService, ISecretProtectionProvider protectionProvider)
    {
        _secretService = secretService;
        _protectionProvider = protectionProvider;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "Secrets", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var secrets = (JObject)context.Step["Secrets"];
        foreach (var kvp in secrets)
        {
            var secretInfo = kvp.Value["SecretInfo"].ToObject<SecretInfo>();
            var secret = _secretService.CreateSecret(secretInfo.Type);

            var protectedData = kvp.Value["SecretData"]?.ToString();
            if (!string.IsNullOrEmpty(protectedData))
            {
                var protector = await _protectionProvider.CreateDecryptorAsync(protectedData);
                secret = JsonConvert.DeserializeObject(protector.Decrypt(), secret.GetType()) as SecretBase;
            }

            // Secret names are deduced from their key.
            secretInfo.Name = kvp.Key;

            await _secretService.UpdateSecretAsync(secretInfo, secret);
        }
    }
}
