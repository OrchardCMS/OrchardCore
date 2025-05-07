using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Redis.Options;

public sealed class RedisKeyManagementOptionsSetup : IConfigureOptions<KeyManagementOptions>
{
    private readonly IRedisService _redis;
    private readonly ILogger _logger;
    private readonly string _tenant;
    private readonly bool _allowAdmin;

    public RedisKeyManagementOptionsSetup(
        IRedisService redis,
        IOptions<RedisOptions> options,
        ShellSettings shellSettings,
        ILogger<RedisKeyManagementOptionsSetup> logger)
    {
        _redis = redis;
        _allowAdmin = options.Value.ConfigurationOptions?.AllowAdmin ?? false;
        _tenant = shellSettings.Name;
        _logger = logger;
    }

    public void Configure(KeyManagementOptions options)
    {
        var redis = _redis;

        if (redis.Database == null)
        {
            redis.ConnectAsync().GetAwaiter().GetResult();
        }

        ValidateRedisServer();

        options.XmlRepository = new RedisXmlRepository(() =>
        {
            if (redis.Database == null)
            {
                redis.ConnectAsync().GetAwaiter().GetResult();
            }

            return redis.Database;
        },
        $"({redis.InstancePrefix}{_tenant}:DataProtection-Keys");
    }

    private void ValidateRedisServer()
    {
        if (!_logger.IsEnabled(LogLevel.Warning))
        {
            return;
        }

        if (_allowAdmin)
        {
            // Redis server admin commands are allowed, try to find out if persistence is enabled. Otherwise we will always warn.
            var server = _redis.Connection?.GetServers().FirstOrDefault();
            if (server != null)
            {
                try
                {
                    var persistenceConfig = server.Info("persistence");

                    if (persistenceConfig.Any(info =>
                        info.Any(value => (value.Key == "aof_enabled" && value.Value == "1") || (value.Key == "rdb_saves" && value.Value != "0"))))
                    {
                        // AOF or RDB persistence is enabled. Note that the check for RDB is not very reliable, because we are only checking if
                        // any save occurred since the last time Redis has started. We are not checking if periodic saves are enabled.
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError(ex, "Unable to check Redis server persistence configuration.");
                    }
                }
            }
        }

        _logger.LogWarning("Redis data protection is enabled. Ensure your Redis server has a backup strategy in place to prevent data loss. " +
            "Use either AOF (Append-Only File) or RDB (Redis Database) persistence. " +
            "For more details, visit: https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/");
    }
}
