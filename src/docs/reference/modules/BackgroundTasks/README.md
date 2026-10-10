# Background Tasks (`OrchardCore.BackgroundTasks`)

This module provides tools to manage background tasks. This includes an admin UI to show which background tasks are registered, how they ran, and to control them.

After enabling the feature, you'll be able to manage background tasks under Tools → Background Tasks. This includes the following:

- Enable or disable a task.
- Change the schedule of the task, i.e. how frequently it runs, with [cron expressions](https://en.wikipedia.org/wiki/Cron#Cron_expression).
- Whether to build the tenant routing pipeline for the task, allowing it to generate correct routes.
- See the execution state of each task: when it last ran, whether that run succeeded or failed and with which error, how long it took, and when it is expected to run next.
- Run a task on demand.

## Execution State

Each task of the list shows whether it is running or queued, when its last run started, its outcome and duration, the error message of a failed run, and when its next scheduled run is expected. The **Status** filter lists the tasks that are running or queued, or whose last run failed.

Clicking the title of a task, or its **Details** button, opens a page with:

- The status of the task, the start time, outcome, duration and trigger of its last run, and the date and time of its next scheduled run.
- The number of completed runs and of failed runs.
- The message and the full details, including the stack trace, of the last error.
- The most recent runs of the task, with their start time, duration, trigger, outcome and error.
- The settings of the task.

While a displayed task is queued or running, the page checks its status in the background and reloads as soon as it changes, so the outcome of the run shows up without refreshing the page.

!!! note
    The execution state is kept in memory by the background service of each server, it is not stored in the database. It reflects the runs of the server handling the request since the application started, so it is reset when the application restarts, and in a web farm each server shows the runs it executed itself.

## Running a Task on Demand

The **Run now** button of a task, available in the list and on the details page, queues the task to run as soon as possible, regardless of its schedule. Select several tasks and use the **Run now** bulk action to queue them all at once.

- A task that is already running or queued is not queued again.
- A disabled task can also be run on demand, which is convenient to check a task before enabling it. Its schedule is not affected.
- The task runs on the server that handled the request, usually within a few seconds, as the background service doesn't wait for the end of its polling time to pick up a requested run. An atomic task still waits to acquire its lock, as for a scheduled run.
- Right after a tenant starts, its tasks are only known to the background service after its next polling, so a task may first be reported as not loaded yet. The request triggers that polling, so trying again a moment later succeeds.

## Configuration

The background service that runs the tasks of all tenants is configured in the `OrchardCore:BackgroundService` section of the application configuration, for example in the `appsettings.json` file:

```json
{
  "OrchardCore": {
    "BackgroundService": {
      "PollingTime": "00:01:00",
      "MinimumIdleTime": "00:00:10",
      "MaxRecentRuns": 10
    }
  }
}
```

| Setting | Description | Default |
| --- | --- | --- |
| `ShellWarmup` | Whether all tenants are loaded when the application starts so that their background tasks run without waiting for a first request. | `false` |
| `PollingTime` | The time to wait between two executions of all the background tasks of a given tenant. | `00:01:00` |
| `MinimumIdleTime` | The minimum idle time before the background tasks of a tenant are triggered, as well as between tasks. | `00:00:10` |
| `MaxRecentRuns` | The number of completed runs kept in memory for each task and shown on its details page. | `10` |

## Using the Execution State from Code

The `IBackgroundTaskMonitor` service gives access to the execution state of the background tasks of a tenant, and lets them be run on demand. It is registered at the host level along with the background service, so it is shared by all tenants, and it is not available in an application that doesn't call `AddBackgroundService()`. Inject it as an `IEnumerable<IBackgroundTaskMonitor>` if your code may run without it.

```csharp
public sealed class MyService
{
    private readonly IBackgroundTaskMonitor _backgroundTaskMonitor;
    private readonly ShellSettings _shellSettings;

    public MyService(IBackgroundTaskMonitor backgroundTaskMonitor, ShellSettings shellSettings)
    {
        _backgroundTaskMonitor = backgroundTaskMonitor;
        _shellSettings = shellSettings;
    }

    public async Task<bool> HasFailedAsync(string taskName)
    {
        var state = await _backgroundTaskMonitor.GetStateAsync(_shellSettings.Name, taskName);

        return state?.LastRun?.Result == BackgroundTaskRunResult.Failed;
    }

    public Task<BackgroundTaskRunRequestResult> RunNowAsync(string taskName)
        => _backgroundTaskMonitor.RequestRunAsync(_shellSettings.Name, taskName);
}
```

The name of a task is the full name of its type, as returned by the `GetTaskName()` extension method of `IBackgroundTask`.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Rx11bdawew0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
