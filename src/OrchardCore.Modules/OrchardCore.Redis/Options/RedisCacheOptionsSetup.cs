using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly IRedisService _redis;
        private readonly string _tenant;

        public RedisCacheOptionsSetup(IRedisService redis, ShellSettings shellSettings)
        {
            _redis = redis;
            _tenant = shellSettings.Name;
        }

        public void Configure(RedisCacheOptions options)
        {
            var redis = _redis;
            options.ConnectionMultiplexerFactory = async () =>
            {
                if (redis.Connection == null)
                {
                    await redis.ConnectAsync();
                }

                return redis.Connection;
            };

            options.InstanceName = $"{redis.InstancePrefix}{_tenant}";
        }
    }
}
