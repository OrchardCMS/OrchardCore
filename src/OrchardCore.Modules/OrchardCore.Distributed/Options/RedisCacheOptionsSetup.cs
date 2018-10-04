using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Distributed.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly ShellSettings _shellSettings;
        private readonly IOptions<RedisOptions> _redisOptionsAccessor;

        public RedisCacheOptionsSetup(ShellSettings shellSettings, IOptions<RedisOptions> redisOptionsAccessor)
        {
            _shellSettings = shellSettings;
            _redisOptionsAccessor = redisOptionsAccessor;
        }

        public void Configure(RedisCacheOptions options)
        {
            options.InstanceName = _shellSettings.Name;

            // Right now we can only pass a string representing the redis configuration.
            options.Configuration = _redisOptionsAccessor.Value.ConfigurationOptions?.ToString();
        }
    }
}
