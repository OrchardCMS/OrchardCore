# Redis (`OrchardCore.Redis`)

Integrates Redis into Orchard Core. Provides the following features:

- Redis: Redis configuration support.
- Redis Cache: Distributed cache using Redis.
- Redis Bus: Makes the `Signal` service distributed.
- Redis Lock: Distributed Lock using Redis.
- Redis DataProtection: [Distributed Data Protection using Redis](#data-protection).

## Health Checks

This module provides a health check to report the status for the Redis server. Refer also to the [Health Checks Section](../HealthChecks/README.md).

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
    "OrchardCore_Redis": {
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
