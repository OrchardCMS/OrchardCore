using System;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class DefaultEncryptionProvider : IEncryptionProvider
{
    private readonly ISecretService _secretService;

    public DefaultEncryptionProvider(ISecretService secretService) => _secretService = secretService;

    public async Task<IEncryptor> CreateAsync(string encryptionSecretName, string signingSecretName)
    {
        var encryptionSecret = await _secretService.GetSecretAsync<RsaSecret>(encryptionSecretName)
            ?? throw new InvalidOperationException($"Secret '{encryptionSecretName}' not found.");

        var signingSecret = await _secretService.GetSecretAsync<RsaSecret>(signingSecretName)
            ?? throw new InvalidOperationException($"Secret '{signingSecretName}' not found.");

        // This becomes irrelevent as we now need the private key for the signature.
        if (signingSecret.KeyType != RsaKeyType.PublicPrivatePair)
        {
            throw new InvalidOperationException("Secret cannot be used for signing.");
        }

        // When encrypting, you use their public key to write a message and they use their private key to read it.
        // When signing, you use your private key to write message's signature,
        // and they use your public key to check if it's really yours.

        return new DefaultEncryptor(
            encryptionSecret.PublicKeyAsBytes(),
            signingSecret.PrivateKeyAsBytes(),
            encryptionSecretName, signingSecretName);
    }
}
