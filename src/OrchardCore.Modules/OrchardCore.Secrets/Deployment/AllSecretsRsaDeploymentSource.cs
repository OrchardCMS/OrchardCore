using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Secrets.Deployment;

public class AllSecretsRsaDeploymentSource : IDeploymentSource
{
    private readonly ISecretService _secretCoordinator;
    private readonly IEncryptionProvider _encryptionProvider;

    public AllSecretsRsaDeploymentSource(
        ISecretService secretCoordinator,
        IEncryptionProvider encryptionProvider)
    {
        _secretCoordinator = secretCoordinator;
        _encryptionProvider = encryptionProvider;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
    {
        if (deploymentStep is not AllSecretsRsaDeploymentStep allSecretsDeploymentStep)
        {
            return;
        }

        var secretBindings = await _secretCoordinator.GetSecretBindingsAsync();
        if (secretBindings.Count == 0)
        {
            return;
        }

        if (String.IsNullOrEmpty(result.EncryptionSecretName))
        {
            throw new InvalidOperationException("You must set an rsa secret for the deployment target before exporting secrets.");
        }

        var encryptor = await _encryptionProvider.CreateAsync(result.EncryptionSecretName, result.SigningSecretName);

        var secrets = new Dictionary<string, JObject>();
        foreach (var secretBinding in secretBindings)
        {
            var secretDescriptor = _secretCoordinator.GetSecretStoreDescriptors().FirstOrDefault(store =>
                String.Equals(store.Name, secretBinding.Value.Store, StringComparison.OrdinalIgnoreCase));

            // When descriptor is readonly we ship a binding without the secret value.
            var jObject = new JObject(new JProperty("SecretBinding", JObject.FromObject(secretBinding.Value)));

            if (!secretDescriptor.IsReadOnly)
            {
                var secret = await _secretCoordinator.GetSecretAsync(secretBinding.Value);
                if (secret is not null)
                {
                    var plaintext = JsonConvert.SerializeObject(secret);
                    var encrypted = encryptor.Encrypt(plaintext);

                    // [js: decrypt('theaesencryptionkey', 'theencryptedvalue')]
                    jObject.Add("Secret", $"[js: decrypt('{encrypted}')]");
                }
            }
            secrets.Add(secretBinding.Key, jObject);
        }

        result.Steps.Add(new JObject(
            new JProperty("name", "Secrets"),
            new JProperty("Secrets", JObject.FromObject(secrets))
        ));
    }
}
