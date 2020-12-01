using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface IEncryptionProvider
    {
        Task<IEncryptor> CreateAsync(string secretName);
    }
}
