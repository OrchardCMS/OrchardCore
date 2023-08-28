using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class DefaultDecryptionProvider : IDecryptionProvider
{
    private readonly ISecretService _secretService;

    public DefaultDecryptionProvider(ISecretService secretService) => _secretService = secretService;

    public async Task<IDecryptor> CreateAsync(string protectedData)
    {
        var bytes = Convert.FromBase64String(protectedData);
        var decoded = Encoding.UTF8.GetString(bytes);

        var descriptor = JsonConvert.DeserializeObject<HybridKeyDescriptor>(decoded);

        var encryptionSecret = await _secretService.GetSecretAsync<RsaSecret>(descriptor.EncryptionSecretName)
            ?? throw new InvalidOperationException($"'{descriptor.EncryptionSecretName}' secret not found.");

        var signingSecret = await _secretService.GetSecretAsync<RsaSecret>(descriptor.SigningSecretName)
            ?? throw new InvalidOperationException($"'{descriptor.SigningSecretName}' secret not found.");

        return new DefaultDecryptor(encryptionSecret.PrivateKeyAsBytes(), signingSecret.PublicKeyAsBytes());
    }
}
