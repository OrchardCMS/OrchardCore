using System;
using System.Threading.Tasks;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets;

public static class SecretServiceExtensions
{
    public static TSecret CreateSecret<TSecret>(this ISecretService secretService)
        where TSecret : SecretBase, new() =>
        secretService.CreateSecret(typeof(TSecret).Name) as TSecret;

    public static async Task<TSecret> GetSecretAsync<TSecret>(this ISecretService secretService, string name)
        where TSecret : SecretBase, new() =>
        await secretService.GetSecretAsync(name) as TSecret;

    public static async Task<TSecret> GetOrAddSecretAsync<TSecret>(
        this ISecretService secretService,
        string name,
        Action<TSecret, SecretInfo> configure = null,
        string source = null)
        where TSecret : SecretBase, new()
    {
        var secret = await secretService.GetSecretAsync<TSecret>(name);
        if (secret is not null)
        {
            return secret;
        }

        return await secretService.AddSecretAsync(name, configure, source);
    }

    public static async Task<TSecret> AddSecretAsync<TSecret>(
        this ISecretService secretService,
        string name,
        Action<TSecret, SecretInfo> configure = null,
        string source = null)
        where TSecret : SecretBase, new()
    {
        var info = new SecretInfo
        {
            Name = name,
            Type = typeof(TSecret).Name,
        };

        var secret = secretService.CreateSecret<TSecret>();

        secret.Name = name;

        configure?.Invoke(secret, info);

        await secretService.UpdateSecretAsync(secret, info, source);

        return secret;
    }
}
