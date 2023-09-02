using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretProtectionProvider : ISecretProtectionProvider
{
    private readonly ISecretService _secretService;

    public SecretProtectionProvider(ISecretService secretService) => _secretService = secretService;

    public async Task<ISecretEncryptor> CreateEncryptorAsync(string encryptionSecret, string signingSecret)
    {
        var encryptionRsaSecret = await _secretService.GetSecretAsync<RSASecret>(encryptionSecret)
            ?? throw new InvalidOperationException($"Secret '{encryptionSecret}' not found.");

        var signingRsaSecret = await _secretService.GetSecretAsync<RSASecret>(signingSecret)
            ?? throw new InvalidOperationException($"Secret '{signingSecret}' not found.");

        // The private key is needed for the signature.
        if (signingRsaSecret.KeyType != RSAKeyType.PublicPrivatePair)
        {
            throw new InvalidOperationException("Secret cannot be used for signing.");
        }

        return new SecretHybridEncryptor(encryptionRsaSecret, signingRsaSecret);
    }

    public async Task<ISecretDecryptor> CreateDecryptorAsync(string protectedData)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(protectedData));
        var envelope = JsonConvert.DeserializeObject<SecretHybridEnvelope>(decoded);

        var encryptionRsaSecret = await _secretService.GetSecretAsync<RSASecret>(envelope.EncryptionSecret)
            ?? throw new InvalidOperationException($"'{envelope.EncryptionSecret}' secret not found.");

        var signingRsaSecret = await _secretService.GetSecretAsync<RSASecret>(envelope.SigningSecret)
            ?? throw new InvalidOperationException($"'{envelope.SigningSecret}' secret not found.");

        return new SecretHybridDecryptor(envelope, encryptionRsaSecret, signingRsaSecret);
    }
}
