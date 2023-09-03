using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public static class SecretServiceExtensions
{
    public static TSecret CreateSecret<TSecret>(this ISecretService secretService) where TSecret : SecretBase, new()
        => secretService.CreateSecret(typeof(TSecret).Name) as TSecret;

    public static async Task<TSecret> GetSecretAsync<TSecret>(this ISecretService secretService, string name)
        where TSecret : SecretBase, new()
    {
        var bindings = await secretService.GetSecretBindingsAsync();
        if (!bindings.TryGetValue(name, out var binding))
        {
            return null;
        }

        return await secretService.GetSecretAsync(binding) as TSecret;
    }
}
