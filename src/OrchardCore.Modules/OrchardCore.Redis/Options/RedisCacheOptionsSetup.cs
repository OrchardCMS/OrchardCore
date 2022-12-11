using System;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly string _tenantPrefix;
        private readonly string _instancePrefix = String.Empty;
        private readonly RedisOptions _redisOptions;

        public RedisCacheOptionsSetup(ShellSettings shellSettings, IOptions<RedisOptions> redisOptions)
        {
            _tenantPrefix = $"{shellSettings.TenantId}_{shellSettings.Name}_";

            _redisOptions = redisOptions.Value;
            if (!String.IsNullOrWhiteSpace(_redisOptions.InstancePrefix))
            {
                _instancePrefix = $"{_redisOptions.InstancePrefix}_";
            }
        }

        public void Configure(RedisCacheOptions options)
        {
            options.InstanceName = $"{_instancePrefix}{_tenantPrefix}";
            options.ConfigurationOptions = _redisOptions.ConfigurationOptions;
        }
    }
}
