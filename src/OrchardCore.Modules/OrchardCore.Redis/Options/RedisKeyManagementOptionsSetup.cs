using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options;

public sealed class RedisKeyManagementOptionsSetup : IConfigureOptions<KeyManagementOptions>
{
    private readonly IRedisService _redis;
    private readonly ShellSettings _tenant;

    public RedisKeyManagementOptionsSetup(IRedisService redis, ShellSettings shellSettings)
    {
        _redis = redis;
        _tenant = shellSettings;
    }

    public void Configure(KeyManagementOptions options)
    {
        var redis = _redis;

        string key;

        if (_tenant["RedisKeyVersion"] == "v1")
        {
            key = $"({redis.InstancePrefix}{_tenant.Name}:DataProtection-Keys";
        }
        else
        {
            key = $"{redis.InstancePrefix}{_tenant.Name}:DataProtection-Keys";
        }

        options.XmlRepository = new RedisXmlRepository(() =>
        {
            if (redis.Database == null)
            {
                redis.ConnectAsync().GetAwaiter().GetResult();
            }

            return redis.Database;
        }, key);
    }
}
