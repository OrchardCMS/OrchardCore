# Redis (`OrchardCore.Redis`)

Integrates Redis into Orchard Core. Provides the following features:

- Redis: Redis configuration support.
- Redis Cache: Distributed cache using Redis.
- Redis Bus: Makes the `Signal` service distributed.
- Redis Lock: Distributed Lock using Redis.
- Redis DataProtection: [Distributed Data Protection using Redis](#data-protection).

## Health Checks

This module provides a health check to report the status for the Redis server. Refer also to the [Health Checks Section](../HealthChecks/README.md).

## Missing or Invalid Configuration

When the `Configuration` string is missing or invalid, an error is logged and the tenant keeps running without Redis: Redis Cache, Redis Bus, Redis Lock, Redis DataProtection and the [SignalR Redis backplane](../SignalR/README.md) aren't active, so the tenant uses the default implementations, and the health check reports the Redis server as unhealthy.

## Using Redis from a Module

A module whose feature depends on `OrchardCore.Redis` can take `IRedisService` in the constructor of its services, as it's always registered while this feature is enabled. When the `Configuration` string is missing or invalid it resolves to `NullRedisService`, which never connects. Whatever the implementation, `Connection` and `Database` are `null` until a connection is created, which can fail, and a connection doesn't mean that the server is reachable:

```csharp
public sealed class CounterService
{
    private readonly IRedisService _redis;
    private readonly string _prefix;
    private readonly ILogger _logger;

    public CounterService(IRedisService redis, ShellSettings shellSettings, ILogger<CounterService> logger)
    {
        _redis = redis;
        _prefix = redis.InstancePrefix + shellSettings.Name + ':';
        _logger = logger;
    }

    public async Task<long> IncrementAsync(string key)
    {
        if (_redis.Database is null)
        {
            await _redis.ConnectAsync();

            if (_redis.Database is null)
            {
                return 0;
            }
        }

        try
        {
            return await _redis.Database.StringIncrementAsync(_prefix + key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to increment '{KeyName}'.", _prefix + key);

            return 0;
        }
    }
}
```

The startup of a feature that replaces a default implementation with one relying on Redis should only do it when Redis is configured, by calling `IsRedisConfigured()`:

```csharp
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<ICounterStore, RedisCounterStore>();
        }
    }
}
```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/etH6IJOGUe8" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Data Protection

The `OrchardCore.Redis.DataProtection` feature enables storing data protection keys in Redis, providing a high-performance solution for distributed applications.

### Purpose

Redis Data Protection enables sharing data protection keys across multiple application instances using Redis as the centralized storage. This is ideal for load-balanced environments where low-latency key access is required.

Data Protection is a critical security feature in ASP.NET Core that Orchard Core leverages to protect sensitive data such as authentication cookies, anti-forgery tokens, persisted secrets that need to be decrypted (e.g. SMTP passwords but not user passwords), and temporary data. In a multi-tenant or load-balanced environment, it's essential to ensure that data protection keys are shared across all nodes and persisted properly.

### Prerequisites

Before configuring Redis Data Protection, ensure you have:

1. A running Redis instance
2. The `OrchardCore.Redis` module enabled
3. The `OrchardCore.Redis.DataProtection` feature enabled

### Configuration

First, configure the basic Redis connection in your `appsettings.json`:

```json
{
  "OrchardCore": {
    "Redis": {
      "Configuration": "<your-redis-connection-string>",
      "InstancePrefix": "MyApp:",
      "AllowAdmin": true
    }
  }
}
```

#### Configuration Options

- **Configuration**: The Redis connection string (required)
- **InstancePrefix**: A prefix to be added to all Redis keys (optional)
- **AllowAdmin**: Whether to allow admin commands (optional, defaults to false)

### Key Storage Structure

Redis Data Protection stores keys using the following pattern:

```
{InstancePrefix}{TenantName}:DataProtection-Keys
```

For example, with an InstancePrefix of "MyApp:" and a tenant named "Default", the key would be:

```
MyApp:Default:DataProtection-Keys
```

### Persistence Considerations

!!! warning
    Data protection keyrings are not cache files and must be kept in durable storage. Ensure that your Redis server has a backup strategy in place to prevent data loss. Use either AOF (Append-Only File) or RDB (Redis Database) persistence.

For more details on Redis persistence, visit: [Redis documentation](https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/).

The Redis Data Protection module will automatically check if persistence is enabled (when `AllowAdmin` is true) and log a warning if it's not configured.
