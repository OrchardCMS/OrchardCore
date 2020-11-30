using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface IEncryptionService
    {
        Task<string> EncryptAsync(string plainText);
        Task<string> InitializeAsync(string secretName);
    }
}
