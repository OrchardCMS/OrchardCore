using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace OrchardCore.Redis.Options
{
    public class RedisKeyManagementOptionsSetup : IConfigureOptions<KeyManagementOptions>
    {
        private readonly IRedisService _redis;

        public RedisKeyManagementOptionsSetup(IRedisService redis) => _redis = redis;

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
            , $"{redis.InstancePrefix}{redis.TenantPrefix}DataProtection-Keys");
        }
    }
}
