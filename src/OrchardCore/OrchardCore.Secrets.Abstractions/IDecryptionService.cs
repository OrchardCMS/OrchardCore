using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface IDecryptionService
    {
        Task<string> DecryptAsync(string encryptionKey, string protectedData);
    }
}
