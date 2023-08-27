using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services;

public class DefaultDecryptionProvider : IDecryptionProvider
{
    private readonly ISecretService _secretCoordinator;

    public DefaultDecryptionProvider(ISecretService secretCoordinator) => _secretCoordinator = secretCoordinator;

    public async Task<IDecryptor> CreateAsync(string protectedData)
    {
        var bytes = Convert.FromBase64String(protectedData);
        var decoded = Encoding.UTF8.GetString(bytes);

        var descriptor = JsonConvert.DeserializeObject<HybridKeyDescriptor>(decoded);

        var encryptionSecret = await _secretCoordinator.GetSecretAsync<RsaSecret>(descriptor.EncryptionSecretName)
            ?? throw new InvalidOperationException($"{descriptor.EncryptionSecretName} secret not found");

        var signingSecret = await _secretCoordinator.GetSecretAsync<RsaSecret>(descriptor.SigningSecretName)
            ?? throw new InvalidOperationException($"Secret not found {descriptor.SigningSecretName}");

        return new DefaultDecryptor(encryptionSecret.PrivateKeyAsBytes(), signingSecret.PublicKeyAsBytes());
    }
}
