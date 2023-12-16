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

    public async Task<ISecretProtector> CreateProtectorAsync(string encryptionSecret, string signingSecret)
    {
        var encryptionRsaSecret = await _secretService.GetSecretAsync<RSASecret>(encryptionSecret)
            ?? throw new InvalidOperationException($"Secret '{encryptionSecret}' not found.");

        var signingRsaSecret = await _secretService.GetSecretAsync<RSASecret>(signingSecret)
            ?? throw new InvalidOperationException($"Secret '{signingSecret}' not found.");

        // The private key is needed for the signature.
        if (signingRsaSecret.KeyType != RSAKeyType.PublicPrivate)
        {
            throw new InvalidOperationException("Secret cannot be used for signing.");
        }

        return new SecretHybridProtector(encryptionRsaSecret, signingRsaSecret);
    }

    public async Task<ISecretUnprotector> CreateUnprotectorAsync(string protectedData)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(protectedData));
        var envelope = JsonConvert.DeserializeObject<SecretHybridEnvelope>(decoded);

        var encryptionRsaSecret = await _secretService.GetSecretAsync<RSASecret>(envelope.EncryptionSecret)
            ?? throw new InvalidOperationException($"'{envelope.EncryptionSecret}' secret not found.");

        // The private key is needed for decryption.
        if (encryptionRsaSecret.KeyType != RSAKeyType.PublicPrivate)
        {
            throw new InvalidOperationException("Secret cannot be used for decryption.");
        }

        var signingRsaSecret = await _secretService.GetSecretAsync<RSASecret>(envelope.SigningSecret)
            ?? throw new InvalidOperationException($"'{envelope.SigningSecret}' secret not found.");

        return new SecretHybridUnprotector(envelope, encryptionRsaSecret, signingRsaSecret);
    }
}
