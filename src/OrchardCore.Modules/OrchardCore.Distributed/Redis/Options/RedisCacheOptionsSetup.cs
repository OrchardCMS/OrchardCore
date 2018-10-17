using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Distributed.Redis.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly ShellSettings _shellSettings;
        private readonly IOptions<RedisOptions> _redisOptions;

        public RedisCacheOptionsSetup(ShellSettings shellSettings, IOptions<RedisOptions> redisOptions)
        {
            _shellSettings = shellSettings;
            _redisOptions = redisOptions;
        }

        public void Configure(RedisCacheOptions options)
        {
            // Used to prefix all keys with the tenant name.
            options.InstanceName = _shellSettings.Name;

            // Right now we can only pass a string representing the redis configuration.
            options.Configuration = _redisOptions.Value.ConfigurationOptions?.ToString();
        }
    }
}
