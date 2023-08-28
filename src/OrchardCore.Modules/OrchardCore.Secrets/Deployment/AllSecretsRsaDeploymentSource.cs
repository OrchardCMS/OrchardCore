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
    private readonly ISecretService _secretService;
    private readonly IEncryptionProvider _encryptionProvider;

    public AllSecretsRsaDeploymentSource(
        ISecretService secretService,
        IEncryptionProvider encryptionProvider)
    {
        _secretService = secretService;
        _encryptionProvider = encryptionProvider;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
    {
        if (deploymentStep is not AllSecretsRsaDeploymentStep allSecretsDeploymentStep)
        {
            return;
        }

        if (String.IsNullOrEmpty(result.EncryptionSecretName))
        {
            throw new InvalidOperationException("You must set an encryption rsa secret for the deployment target before exporting secrets.");
        }

        if (String.IsNullOrEmpty(result.SigningSecretName))
        {
            throw new InvalidOperationException("You must set a signin rsa secret for the deployment target before exporting secrets.");
        }

        // Deployment secrets should already exist and not with a private key in both sides.
        var secretBindings = (await _secretService.GetSecretBindingsAsync()).Where(binding =>
            !String.Equals(binding.Value.Name, result.EncryptionSecretName, StringComparison.OrdinalIgnoreCase) &&
            !String.Equals(binding.Value.Name, result.SigningSecretName, StringComparison.OrdinalIgnoreCase));

        if (!secretBindings.Any())
        {
            return;
        }

        var encryptor = await _encryptionProvider.CreateAsync(result.EncryptionSecretName, result.SigningSecretName);

        var secrets = new Dictionary<string, JObject>();
        foreach (var secretBinding in secretBindings)
        {
            var storeDescriptor = _secretService.GetSecretStoreDescriptors().FirstOrDefault(store =>
                String.Equals(store.Name, secretBinding.Value.Store, StringComparison.OrdinalIgnoreCase));

            // When descriptor is readonly we ship a binding without the secret value.
            var jObject = new JObject(new JProperty("SecretBinding", JObject.FromObject(secretBinding.Value)));

            if (!storeDescriptor.IsReadOnly)
            {
                var secret = await _secretService.GetSecretAsync(secretBinding.Value);
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
