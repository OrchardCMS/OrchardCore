using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface IEncryptionService
    {
        Task<string> EncryptAsync(string secretName, string plainText);
        string GetKey(string secretName);
    }
}
