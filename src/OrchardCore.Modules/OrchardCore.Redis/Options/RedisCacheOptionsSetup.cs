using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly string _tenant;
        private const string Separator = "_";
        private readonly IOptions<RedisOptions> _redisOptions;

        public RedisCacheOptionsSetup(ShellSettings shellSettings, IOptions<RedisOptions> redisOptions)
        {
            _tenant = shellSettings.TenantId + Separator + shellSettings.Name;
            _redisOptions = redisOptions;
        }

        public void Configure(RedisCacheOptions options)
        {
            options.InstanceName = _redisOptions.Value.InstancePrefix + _tenant + Separator;
            options.ConfigurationOptions = _redisOptions.Value.ConfigurationOptions;
        }
    }
}
