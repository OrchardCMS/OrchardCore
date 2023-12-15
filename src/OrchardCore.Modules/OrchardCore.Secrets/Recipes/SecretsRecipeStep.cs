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
                var unprotector = await _protectionProvider.CreateUnprotectorAsync(protectedData);
                secret = JsonConvert.DeserializeObject(unprotector.Unprotect(), secret.GetType()) as SecretBase;
            }

            secretInfo.Name = kvp.Key;

            await _secretService.UpdateSecretAsync(secretInfo, secret);
        }
    }
}
