using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretProtection : ISecretProtection
{
    private readonly ISecretService _secretService;

    public SecretProtection(ISecretService secretService) => _secretService = secretService;

    public async Task<ISecretEncryptor> CreateEncryptorAsync(string encryptionSecret, string signingSecret)
    {
        var encryptionRsaSecret = await _secretService.GetSecretAsync<RsaSecret>(encryptionSecret)
            ?? throw new InvalidOperationException($"Secret '{encryptionSecret}' not found.");

        var signingRsaSecret = await _secretService.GetSecretAsync<RsaSecret>(signingSecret)
            ?? throw new InvalidOperationException($"Secret '{signingSecret}' not found.");

        // The private key is needed for the signature.
        if (signingRsaSecret.KeyType != RsaKeyType.PublicPrivatePair)
        {
            throw new InvalidOperationException("Secret cannot be used for signing.");
        }

        return new SecretEncryptor(encryptionRsaSecret, signingRsaSecret);
    }

    public async Task<ISecretDecryptor> CreateDecryptorAsync(string protectedData)
    {
        var bytes = Convert.FromBase64String(protectedData);
        var decoded = Encoding.UTF8.GetString(bytes);

        var descriptor = JsonConvert.DeserializeObject<HybridKeyDescriptor>(decoded);

        var encryptionSecret = await _secretService.GetSecretAsync<RsaSecret>(descriptor.EncryptionSecret)
            ?? throw new InvalidOperationException($"'{descriptor.EncryptionSecret}' secret not found.");

        var signingSecret = await _secretService.GetSecretAsync<RsaSecret>(descriptor.SigningSecret)
            ?? throw new InvalidOperationException($"'{descriptor.SigningSecret}' secret not found.");

        return new SecretDecryptor(descriptor, encryptionSecret, signingSecret);
    }
}
