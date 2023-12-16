using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Secrets.Deployment;

public class AllSecretsDeploymentSource : IDeploymentSource
{
    private readonly ISecretService _secretService;
    private readonly ISecretProtectionProvider _protectionProvider;

    public AllSecretsDeploymentSource(ISecretService secretService, ISecretProtectionProvider protectionProvider)
    {
        _secretService = secretService;
        _protectionProvider = protectionProvider;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
    {
        if (deploymentStep is not AllSecretsDeploymentStep allSecretsDeploymentStep)
        {
            return;
        }

        if (string.IsNullOrEmpty(result.EncryptionSecret))
        {
            throw new InvalidOperationException("You must set an encryption rsa secret for the deployment target before exporting secrets.");
        }

        if (string.IsNullOrEmpty(result.SigningSecret))
        {
            throw new InvalidOperationException("You must set a signing rsa secret for the deployment target before exporting secrets.");
        }

        // Secrets used for the deployment itself should already exist on both sides.
        var secretInfos = (await _secretService.GetSecretInfosAsync()).Where(secret =>
            !string.Equals(secret.Value.Name, result.EncryptionSecret, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(secret.Value.Name, result.SigningSecret, StringComparison.OrdinalIgnoreCase));

        if (!secretInfos.Any())
        {
            return;
        }

        var protector = await _protectionProvider.CreateProtectorAsync(result.EncryptionSecret, result.SigningSecret);

        var secrets = new Dictionary<string, JObject>();
        foreach (var secretInfo in secretInfos)
        {
            var store = _secretService.GetSecretStoreInfos().FirstOrDefault(store =>
                string.Equals(store.Name, secretInfo.Value.Store, StringComparison.OrdinalIgnoreCase));

            if (store is null)
            {
                continue;
            }

            var jsonSecretInfo = JObject.FromObject(secretInfo.Value);

            // Cleanup secret names that will be deduced from their keys.
            jsonSecretInfo.Remove("Name");

            var jObject = new JObject(new JProperty("SecretInfo", jsonSecretInfo));

            // When the store is readonly we ship the secret info without the secret value.
            if (!store.IsReadOnly)
            {
                var secret = await _secretService.GetSecretAsync(secretInfo.Value.Name);
                if (secret is not null)
                {
                    var plaintext = JsonConvert.SerializeObject(secret);
                    var encrypted = await protector.ProtectAsync(plaintext);
                    jObject.Add("SecretData", encrypted);
                }
            }

            secrets.Add(secretInfo.Key, jObject);
        }

        result.Steps.Add(new JObject(
            new JProperty("name", "Secrets"),
            new JProperty("Secrets", JObject.FromObject(secrets))
        ));
    }
}
