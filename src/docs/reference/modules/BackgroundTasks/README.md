# Background Tasks (`OrchardCore.BackgroundTasks`)

Background tasks run scheduled work inside an Orchard Core tenant scope. The `OrchardCore.BackgroundTasks` module adds per-tenant settings and an admin UI for registered implementations of `IBackgroundTask`.

## Enable and manage background tasks

Enable the **Background Tasks** feature (`OrchardCore.BackgroundTasks`) from **Tools** > **Features** for each tenant where administrators need to manage task settings. The Orchard Core host can execute registered tasks from their code-defined defaults without this feature, but the feature is required for persisted overrides and the admin UI.

Users with the `ManageBackgroundTasks` permission can open **Tools** > **Background Tasks**. Administrators receive this permission by default. The page supports:

- Searching and filtering registered tasks.
- Enabling or disabling a task.
- Changing its cron schedule and description.
- Enabling the tenant routing pipeline.
- Configuring distributed lock acquisition and expiration times.

Settings are stored separately for each tenant. Values saved in the admin override the defaults declared by the task.

## Implement a background task

Implement `IBackgroundTask` and decorate the class with `BackgroundTaskAttribute` to define its admin title, description, schedule, and optional reliability settings.

The following example delegates tenant-specific work to a scoped service:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;

namespace MyModule;

public interface ITenantMaintenance
{
    Task RunAsync(CancellationToken cancellationToken);
}

public sealed class TenantMaintenance : ITenantMaintenance
{
    private readonly ILogger _logger;

    public TenantMaintenance(ILogger<TenantMaintenance> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Tenant maintenance completed.");

        return Task.CompletedTask;
    }
}

[BackgroundTask(
    Title = "Tenant Maintenance",
    Schedule = "*/15 * * * *",
    Description = "Performs periodic tenant maintenance.",
    LockTimeout = 3_000,
    LockExpiration = 30_000)]
public sealed class TenantMaintenanceBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var maintenance = serviceProvider.GetRequiredService<ITenantMaintenance>();

        return maintenance.RunAsync(cancellationToken);
    }
}
```

Register the task and its dependencies from the module startup:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace MyModule;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITenantMaintenance, TenantMaintenance>();
        services.AddSingleton<IBackgroundTask, TenantMaintenanceBackgroundTask>();
    }
}
```

Register each task as an `IBackgroundTask`; registering only its concrete type does not make it discoverable. Orchard Core commonly registers task implementations as singletons. Resolve tenant-scoped dependencies from the `IServiceProvider` passed to `DoWorkAsync` rather than injecting them into the task constructor.

The technical task name is the implementation type's full name. Orchard Core uses this name to associate persisted settings with the task, so renaming or moving the class creates a new task identity.

### Default settings

`BackgroundTaskAttribute` supports the following properties:

| Property | Default | Purpose |
| --- | --- | --- |
| `Title` | Full task type name | Display name in the admin UI. |
| `Description` | Empty | Description shown to administrators. |
| `Enable` | `true` | Initial enabled state. |
| `Schedule` | `*/5 * * * *` | Initial five-field cron schedule. |
| `UsePipeline` | `false` | Builds and invokes the tenant pipeline before the task. |
| `LockTimeout` | `0` | Maximum time in milliseconds to acquire a distributed lock. |
| `LockExpiration` | `0` | Distributed lock lifetime in milliseconds. |

The attribute is optional. Without it, the task is enabled, uses its full type name as its title, and runs on the `* * * * *` schedule by default.

## Scheduling behavior

Schedules use the NCrontab five-field format:

```text
minute hour day-of-month month day-of-week
```

For example:

| Expression | Meaning |
| --- | --- |
| `* * * * *` | Every minute. |
| `*/15 * * * *` | Every 15 minutes. |
| `0 * * * *` | At the start of every hour. |
| `0 0 * * *` | Daily at midnight. |

Do not include a seconds field. The schedule is evaluated in the tenant's configured time zone when site settings are available.

Cron expressions identify when a task becomes due; they do not provide exact-time execution. The host polls for due tasks, so a task can start after its scheduled minute. A process restart does not replay occurrences missed while the application was stopped.

Within one application instance, due tasks for a tenant run sequentially. Different tenants can be processed in parallel. A task's next occurrence is calculated from its last start time, and the same task does not overlap with itself in the same tenant on the same application instance.

## Multi-tenancy and the tenant pipeline

Each registered task runs once per active tenant whose feature registration includes that task. Orchard Core creates a shell scope for the tenant and passes its scoped service provider to `DoWorkAsync`.

By default, a tenant becomes eligible for background processing after its pipeline has been built by a matching request. Set `ShellWarmup` to `true` in the [host configuration](#host-configuration) to initialize all tenants when the application starts.

Enable **Use Tenant Pipeline** only when a task needs endpoint-aware routing, such as URL generation based on tenant routes. Orchard Core then builds the pipeline if necessary and invokes it with a background `HttpContext`. The context uses the tenant URL host and prefix, and the site `BaseUrl` overrides its scheme, host, and path base when configured.

Pipeline execution adds work to every invocation and is unnecessary for tasks that only use tenant services or data.

## Reliability and failure handling

Background tasks are an in-process scheduler, not a durable job queue:

- The scheduler does not persist an execution history or automatically retry a failed invocation.
- Exceptions from `DoWorkAsync` are logged, passed to registered `IBackgroundTaskEventHandler` implementations, and do not stop later scheduler cycles.
- The application stopping token is passed to the task. Propagate it to asynchronous operations and stop promptly when cancellation is requested.
- Design task work to be idempotent because a process can stop after producing side effects but before the operation is considered complete.

### Multiple application instances

Without a distributed lock, every application instance can execute the same task. To limit a task to one instance:

1. Enable a distributed lock implementation, such as the [Redis Lock feature](../Redis/README.md).
2. Set both `LockTimeout` and `LockExpiration` to values greater than zero on the attribute or in the task's advanced admin settings.

`LockTimeout` controls how long an instance waits to acquire the lock. `LockExpiration` controls how long the acquired lock remains valid. Set the expiration longer than the task's expected maximum duration; the scheduler does not renew it while the task runs.

Lock settings have no effect when only the local lock implementation is active. If lock acquisition times out or fails, the task is skipped for that scheduler cycle and the event is logged.

## Host configuration

The host-level background service is configured under `OrchardCore:OrchardCore_BackgroundService`. These settings apply to all tenants:

```json
{
  "OrchardCore": {
    "OrchardCore_BackgroundService": {
      "ShellWarmup": true,
      "PollingTime": "00:01:00",
      "MinimumIdleTime": "00:00:10"
    }
  }
}
```

| Setting | Default | Description |
| --- | --- | --- |
| `ShellWarmup` | `false` | Initializes tenants at startup so their tasks can run before the first matching request. |
| `PollingTime` | `00:01:00` | Target delay between scheduler polling cycles. |
| `MinimumIdleTime` | `00:00:10` | Minimum delay used before processing begins and between scheduler cycles. |

Lower intervals increase scheduler activity and do not make a cron expression more precise than its one-minute resolution.

## Logging and troubleshooting

The host logs task starts and completions at `Information` level and failures at `Error` level. Messages include the task's full type name and tenant name. A task can also resolve or inject an `ILogger<T>` for its own operational details.

If a task is not listed in the admin UI:

- Confirm that the feature registering the task is enabled for that tenant.
- Confirm that it is registered as `IBackgroundTask`.
- Confirm that the `OrchardCore.BackgroundTasks` feature is enabled.

If a listed task does not run:

- Check that it is enabled and that its five-field cron expression is valid.
- Check the application logs for scheduler, task, pipeline, or distributed lock errors.
- If the tenant has not received a request since startup, enable `ShellWarmup` or request one of its URLs.
- In a multi-node deployment, verify the distributed lock configuration and make `LockExpiration` long enough for the task.

If generated URLs are incorrect, configure the tenant's site `BaseUrl` and enable **Use Tenant Pipeline** for that task.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Rx11bdawew0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
