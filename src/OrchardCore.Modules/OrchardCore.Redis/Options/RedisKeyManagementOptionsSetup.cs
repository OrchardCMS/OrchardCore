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
        _logger = logger;
        _tenant = shellSettings.Name;
        _allowAdmin = options.Value.ConfigurationOptions?.AllowAdmin ?? false;
    }

    public void Configure(KeyManagementOptions options)
    {
        var redis = _redis;
        var redisKey = $"{redis.InstancePrefix}{_tenant}:DataProtection-Keys";

        if (redis.Database == null)
        {
            redis.ConnectAsync().GetAwaiter().GetResult();
        }

        ValidateRedisServer();
        ChangeXmlRepositoryRedisKey(redisKey);

        options.XmlRepository = new RedisXmlRepository(() =>
            {
                if (redis.Database == null)
                {
                    redis.ConnectAsync().GetAwaiter().GetResult();
                    ChangeXmlRepositoryRedisKey(redisKey);
                }

                return redis.Database;
            },
            redisKey);
    }

    // In Orchard Core version 3, the Redis key format for each tenant has been updated.
    // To avoid breaking data protection during upgrade and also to not impact existing nodes, we
    // copy the old key to the new one if it doesn't exist.
    private void ChangeXmlRepositoryRedisKey(string redisKey)
    {
        var database = _redis.Database;
        if (database == null)
        {
            return;
        }

        var oldRedisKey = $"({_redis.InstancePrefix}{_tenant}:DataProtection-Keys";
        try
        {
            if (!database.KeyExists(redisKey))
            {
                if (database.KeyCopy(oldRedisKey, redisKey) && _logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("The data protection Redis key for tenant '{Tenant}' was updated from '{OldRedisKey}' to '{NewRedisKey}'.", _tenant, oldRedisKey, redisKey);
                }
            }
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Unable to copy the old Redis data protection key '{OldRedisKey}' to the new one '{NewRedisKey}'.", oldRedisKey, redisKey);
            }
        }
    }

    private void ValidateRedisServer()
    {
        if (_logger.IsEnabled(LogLevel.Warning) && !CheckRedisPersistenceEnabled())
        {
            _logger.LogWarning("Redis data protection is enabled. Ensure your Redis server has a backup strategy in place to prevent data loss. " +
                "Use either AOF (Append-Only File) or RDB (Redis Database) persistence. " +
                "For more details, visit: https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/");
        }
    }

    private bool CheckRedisPersistenceEnabled()
    {
        if (!_allowAdmin)
        {
            // Redis server admin commands are not allowed, we cannot check persistence.
            return false;
        }

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
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to check Redis server persistence configuration.");
            }
        }

        return false;
    }
}
