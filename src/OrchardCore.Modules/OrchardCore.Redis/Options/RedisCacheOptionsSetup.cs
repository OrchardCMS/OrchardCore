using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly IRedisService _redis;
        private readonly string _tenant;
        private readonly RedisOptions _redisOptions;

        public RedisCacheOptionsSetup(IRedisService redis, ShellSettings shellSettings, IOptions<RedisOptions> redisOptions)
        {
            _redis = redis;
            _tenant = shellSettings.Name;
            _redisOptions = redisOptions.Value;
        }

        public void Configure(RedisCacheOptions options)
        {
            options.InstanceName = _redisOptions.InstancePrefix + _tenant;
            options.ConfigurationOptions = _redisOptions.ConfigurationOptions;

            var redis = _redis;
            options.ConnectionMultiplexerFactory = async () =>
            {
                if (redis.Connection == null)
                {
                    await redis.ConnectAsync();
                }

                return redis.Connection;
            };
        }
    }
}
