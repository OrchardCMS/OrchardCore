using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using StackExchange.Redis;

namespace OrchardCore.Redis.Options
{
    public class RedisOptionsSetup : IConfigureOptions<RedisOptions>
    {
        private readonly IShellConfiguration _shellConfiguration;

        public RedisOptionsSetup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

        public void Configure(RedisOptions options)
        {
            var configuration = _shellConfiguration["OrchardCore.Redis:Configuration"];
            options.ConfigurationOptions = ConfigurationOptions.Parse(configuration);
        }
    }
}
