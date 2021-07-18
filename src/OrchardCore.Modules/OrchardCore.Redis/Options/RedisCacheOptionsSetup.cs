using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly string _tenant;
        private readonly IOptions<RedisOptions> _redisOptions;

        public RedisCacheOptionsSetup(ShellSettings shellSettings, IOptions<RedisOptions> redisOptions)
        {
            _tenant = shellSettings.Name;
            _redisOptions = redisOptions;
        }

        public void Configure(RedisCacheOptions options)
        {
            options.InstanceName = _redisOptions.Value.InstancePrefix + _tenant;
            options.ConfigurationOptions = _redisOptions.Value.ConfigurationOptions;
        }
    }
}
