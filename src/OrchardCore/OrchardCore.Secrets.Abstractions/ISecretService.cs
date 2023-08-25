using System.Threading.Tasks;

namespace OrchardCore.Secrets;

public interface ISecretService<TSecret> where TSecret : Secret, new()
{
    Task<TSecret> GetSecretAsync(string key);
}
