using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Secrets.Services
{
    public class ConfigurationSecretStore : ISecretStore
    {
        private readonly IShellConfiguration _shellConfiguration;

        private readonly IStringLocalizer S;

        public ConfigurationSecretStore(
            IShellConfiguration shellConfiguration,
            IStringLocalizer<DatabaseSecretStore> stringLocalizer)
        {
            _shellConfiguration = shellConfiguration;
            S = stringLocalizer;
        }

        public string Name => nameof(ConfigurationSecretStore);
        public string DisplayName => S["Configuration Secret Store"];
        public bool IsReadOnly => true;

        public Task<Secret> GetSecretAsync(string key, Type type)
        {
            return Task.FromResult(_shellConfiguration.GetSection($"OrchardCore_Secrets:{key}").Get(type) as Secret);
        }

        public Task<TSecret> GetSecretAsync<TSecret>(string key) where TSecret : Secret, new()
        {
            //TODO we should be able to use GetChildren() here on the parent section.
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#getsection-getchildren-and-exists
            return Task.FromResult(_shellConfiguration.GetSection($"OrchardCore_Secrets:{key}").Get<TSecret>());
        }

        public Task UpdateSecretAsync(string key, Secret secret)
        {
            throw new NotSupportedException("The Configuration Secret Store is ReadOnly");
        }

        public Task RemoveSecretAsync(string key)
        {
            throw new NotSupportedException("The Configuration Secret Store is ReadOnly");
        }

    }
}
