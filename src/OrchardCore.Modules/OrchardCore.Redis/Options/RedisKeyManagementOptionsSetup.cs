using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options
{
    public class RedisKeyManagementOptionsSetup : IConfigureOptions<KeyManagementOptions>
    {
        private readonly IRedisService _redis;
        private readonly string _tenant;

        public RedisKeyManagementOptionsSetup(IRedisService redis, ShellSettings shellSettings)
        {
            _redis = redis;
            _tenant = shellSettings.Name;
        }

        public void Configure(KeyManagementOptions options)
        {
            var redis = _redis;

            options.XmlRepository = new RedisXmlRepository(() =>
            {
                if (redis.Database == null)
                {
                    redis.ConnectAsync().GetAwaiter().GetResult();
                }

                return redis.Database;
            }
            , redis.InstancePrefix + _tenant + ":DataProtection-Keys");
        }
    }
}
