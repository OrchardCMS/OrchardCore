using System.Threading.Tasks;

namespace OrchardCore.Secrets;

public interface ISecretProtectionProvider
{
    Task<ISecretProtector> CreateProtectorAsync(string encryptionSecret, string signingSecret);
    Task<ISecretUnprotector> CreateUnprotectorAsync(string protectedData);
}
