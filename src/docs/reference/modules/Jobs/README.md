# Jobs (`OrchardCore.Jobs`)

A job is work that runs in the background **once, whatever the number of servers** running the application. Each run of a job is recorded with its status, the server that executed it, its progress, its log and its result, and the admin lists the runs, as a CI system lists its builds.

A run is queued at once, at a later date, or by a schedule. Modules provide the jobs: trimming the audit trail, publishing a content item at its scheduled date, running a data pipeline, importing a deployment package, and so on.

## Jobs and Background Tasks

[Background tasks](../BackgroundTasks/README.md) and jobs both run in the background, for different work:

| | Background task | Job |
| --- | --- | --- |
| Runs | On **every server**, each one following its own copy of the schedule | On **a single server**, whichever takes the run first |
| For | What each server has to do: cleaning its local files, its memory, its local cache | What has to be done once: sending emails, changing the database, calling another system |
| State | Kept in memory by each server | Stored in the database, the same for every server |
| Admin pages | Show the server that renders them, and need session affinity | The same from any server |
| Started by | Its schedule, or by hand on one server | A request, a schedule, a workflow, the code of a module, another run |

The Jobs feature itself uses a background task: every minute, each server offers to execute the runs that are due.

## Managing Jobs

After enabling the **Jobs** feature, the admin has three pages under **Tools → Jobs**. Viewing them requires the **View jobs** permission, and changing anything the **Manage jobs** permission.

### Runs

**Tools → Jobs → Runs** lists the runs, the most recent first, with their job, their status, what started them, when they started, how long they took and the server that executed them. The list is filtered by job and by status, and its bulk actions cancel, run again or delete the selected runs.

| Status | Meaning |
| --- | --- |
| Scheduled | The run is queued for a later date. |
| Queued | The run waits for a server to execute it. |
| Running | A server executes the run. |
| Waiting for child runs | The job is done, and the run waits for the runs it queued. |
| Succeeded | The run completed, as did its child runs. |
| Failed | The run failed on its last attempt, its server stopped while executing it, or one of its child runs didn't succeed. |
| Canceled | The run was canceled before it completed. |

The page of a run shows its timeline, its attempts, its server, its parameters, its result, its log, its error, and its child runs. The details of the exception that failed a run, such as its stack trace, are only shown with the **Manage jobs** permission. From there, a run that didn't end can be **canceled**, and a run that ended can be **run again**, which queues a new run with the same parameters and opens it, or **deleted**. From the list of runs, **Run again** stays on the list, where the new run shows up.

The list and the page of a run stay up to date while they are visible, without reloading the page. They show the same from any server, as the runs are read from the database.

### Jobs

**Tools → Jobs → Jobs** lists the jobs the enabled features provide, with their schedules and their last run. A job that can be run from the admin has these actions:

- **Run now** queues a run without parameters.
- **Run with parameters** asks for the title of the run and its parameters, as a JSON object.
- **Add a schedule** creates a schedule for the job.

The other jobs only make sense with the parameters their feature queues them with, such as the content item to publish, so they can't be run from the admin.

### Schedules

**Tools → Jobs → Schedules** lists the schedules. A schedule queues a run of a job at each occurrence of a [cron expression](https://en.wikipedia.org/wiki/Cron#Cron_expression) with five fields, in the time zone of the schedule, optionally with parameters.

- Whatever the number of servers, an occurrence queues a single run.
- While the run of an occurrence didn't end, the following occurrences don't queue another one.
- The occurrences missed while no server was running result in a single run.
- **Run now** queues a run with the parameters of the schedule, without changing its next occurrence.

A schedule is shown with a description of when it occurs, such as "At 03:00 AM". Its editor shows the description and the next occurrences of the expression as it is typed, offers common schedules and the schedule the job declares, and links to [crontab.guru](https://crontab.guru/).

A job can declare a schedule, shown as **Built-in**. Such a schedule can be changed and disabled, but not deleted.

#### Time Zone of a Schedule

The cron expression of a schedule is stored as it is typed, and applies to the **time zone** of the schedule:

- **Time zone of the site**, the default, which the list of time zones shows with the one it is at the moment. The schedule follows the time zone of the site: when it changes, the schedule keeps its clock time, such as 3:00, in the new time zone, and its next run moves within a minute.
- **A selected time zone**, which can be the same one as the site. The schedule stays in it whatever the time zone of the site, which suits a schedule that follows the hours of another region.

The tooltip of a cron expression tells the time zone it applies to, and the description of a schedule with a selected time zone ends with its abbreviation, such as "At 03:00 AM (CET)".

## How Runs Execute

- **A single server executes a run.** A server takes a run by changing it from queued to running in the database, which only succeeds if no other server changed it in between. No distributed lock is needed.
- **A run starts quickly.** The server that queues a run executes it as soon as its request ends. The other servers find the runs that are due every minute. A tenant only executes runs on the servers where it handled a request, unless the `ShellWarmup` option of the [background service](../BackgroundTasks/README.md#configuration) is enabled.
- **A server reports while it executes a run**, every few seconds, and saves the progress and the log of the run at that time.
- **A run whose server stops is not lost.** A run that stopped reporting for several minutes is considered abandoned. It is queued again when its job allows several attempts, and fails otherwise.
- **A run that fails is tried again** when its job allows several attempts, after a delay that doubles for each attempt.
- **A cancellation reaches any server.** A run that didn't start is canceled at once. A running run stops at the next report of its server, or at once when it is the server that handled the request.

!!! note
    A job runs **at least once** when it allows several attempts: an attempt that fails or whose server stops is followed by another one, so such a job must be able to run again from the start. By default a job makes a single attempt, so a run that didn't complete is reported as failed and is not executed again.

## Configuration

The following configuration values can be customized, for all tenants or per tenant:

```json
{
  "OrchardCore": {
    "Jobs": {
      "MaxConcurrentRuns": 4,
      "MaxConcurrentRunsPerServer": 0,
      "HeartbeatSeconds": 10,
      "AbandonedAfterMinutes": 5,
      "RunRetentionDays": 30
    }
  }
}
```

| Setting | Default | Description |
| --- | --- | --- |
| `MaxConcurrentRuns` | `4` | The number of runs of a tenant a server executes at the same time. The other runs wait, queued, for one to end, or for another server. |
| `MaxConcurrentRunsPerServer` | `0` | The number of runs a server executes at the same time for all its tenants. `0` doesn't limit them. Set it in the application configuration, as the value of the first tenant that executes a run applies. |
| `HeartbeatSeconds` | `10` | How often a server reports that a run is still executing, and saves its progress and its log. A cancellation requested from another server is seen at that time. |
| `AbandonedAfterMinutes` | `5` | The time after which a running run that stopped reporting is considered abandoned by its server. |
| `RunRetentionDays` | `30` | The number of days the runs that ended are kept. `0` keeps them. |

### Server Identity

Each run records the server that executes it. A server is identified by its machine name followed by its process identifier, which can be replaced in the application configuration. The identifier must be unique among the running instances of the application:

```json
{
  "OrchardCore": {
    "Node": {
      "Id": "web-1"
    }
  }
}
```

## Writing a Job

A job implements `IJob`, from the `OrchardCore.Jobs.Abstractions` package. It is created for each run, in the service scope of that run, so its constructor takes any service of the tenant:

```csharp
public sealed class SendNewsletterJob : IJob
{
    private readonly INewsletterService _newsletterService;

    public SendNewsletterJob(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<SendNewsletterParameters>();
        var recipients = await _newsletterService.GetRecipientsAsync(parameters.NewsletterId);

        for (var i = 0; i < recipients.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _newsletterService.SendAsync(parameters.NewsletterId, recipients[i]);
            context.ReportProgress((i + 1) * 100 / recipients.Count, $"{i + 1} of {recipients.Count} emails sent.");
        }

        context.LogInformation($"{recipients.Count} emails were sent.");
        context.SetResult(new { Sent = recipients.Count });
    }
}
```

- What the job saves in its scope, such as with the YesSql session, is committed when it returns, before the run is recorded as succeeded.
- An exception fails the run, or the attempt when the job allows several.
- The cancellation token is cancelled when the run is canceled, or its server stops.

Register the job in the `Startup` of the feature, and make the feature depend on `OrchardCore.Jobs`:

```csharp
services.AddJob<SendNewsletterJob>("SendNewsletter", job =>
{
    job.Title = NewsletterLocalizationSources.SendJobTitle;
    job.Description = NewsletterLocalizationSources.SendJobDescription;
});
```

```csharp
[assembly: Feature(
    Id = "MyModule.Newsletters",
    Name = "Newsletters",
    Dependencies = ["OrchardCore.Jobs"]
)]
```

The job is not added to the service container, and keeps no state between runs. The descriptor of a job has these settings:

| Setting | Default | Description |
| --- | --- | --- |
| `Title`, `Description` | | The localized title and description shown in the admin, as `LocalizationSource` values. |
| `MaxAttempts` | `1` | The number of attempts a run may make. Set it higher only for a job that can run again from the start. |
| `RetryDelay` | 30 seconds | How long a failed run waits before its second attempt. The delay doubles for each following attempt. |
| `Schedule` | | The cron expression of the schedule the job declares. |
| `ScheduleEnabled` | `true` | Whether the declared schedule is created enabled. |
| `AllowManualRun` | `false` | Whether the job can be run and scheduled from the admin, and run by a workflow. A job that declares a schedule always can. |
| `KeepSucceededRuns` | `true` | Whether the runs that succeeded are kept. Turn it off for a job that runs very often, to only keep its failures. |

## Queuing Runs

`IJobManager` queues the runs. A run is saved with the current database session, so it is only queued if the current request completes:

```csharp
var run = await _jobManager.EnqueueAsync<SendNewsletterJob>(
    new SendNewsletterParameters { NewsletterId = newsletter.Id },
    request =>
    {
        request.Title = newsletter.Title;
        request.DueUtc = newsletter.SendUtc;
        request.DeduplicationKey = newsletter.Id;
    });
```

| Option | Description |
| --- | --- |
| `Title` | What the run is about, shown next to the title of the job. |
| `DueUtc` | When the run may start. Without it, the run starts as soon as a server is available. |
| `DeduplicationKey` | Prevents the same work from being queued twice: while a run of the job with this key didn't end, queuing another one returns that run. With `ReplaceQueued`, a run that didn't start gets the parameters and the due date of the request, which reschedules it. |
| `ConcurrencyKey` | Makes the runs with the same key execute one at a time, on any server. |
| `CorrelationId` | Links the run to what queues it, such as a content item. |
| `MaxAttempts` | Replaces the number of attempts of the job for this run. |
| `RequireCurrentNode` | Makes the server that queues the run the only one that may execute it, for a run that needs something only this server has, such as a file it just received. Such a run fails when this server stops before executing it. |

`IJobManager` also finds a run (`GetAsync`, `FindActiveAsync`, `FindLastAsync`), cancels it (`CancelAsync`), queues it again (`RerunAsync`) and deletes it (`DeleteAsync`).

### Splitting Work in Child Runs

A job that has much work to do can split it in batches, each one a child run that any server executes. The run then waits for its child runs, and succeeds when they all succeeded:

```csharp
public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
{
    var parameters = context.GetParameters<SendNewsletterParameters>();

    foreach (var batch in (await _newsletterService.GetRecipientIdsAsync(parameters.NewsletterId)).Chunk(500))
    {
        await context.EnqueueChildAsync("SendNewsletterBatch", new SendNewsletterBatchParameters
        {
            NewsletterId = parameters.NewsletterId,
            RecipientIds = batch,
        });
    }
}
```

The child runs are listed on the page of their parent run. Canceling a run cancels its child runs.

### Finding the Work That Is Due

When the work to do is found by looking at the data, such as the content items whose publish date passed, keep the search in a background task, which only reads, and queue a run for each item found, with the identifier of the item as its `DeduplicationKey` and its `ConcurrencyKey`. The servers that find an item at the same time then queue a single run, and two runs for the same item never execute at the same time. This is how the scheduled publishing and archiving of content items work.

### Handling the End of a Run

Implement `IJobRunHandler` to do something when a run of any job ends, and register it as a scoped service:

```csharp
public sealed class NewsletterJobRunHandler : IJobRunHandler
{
    public Task CompletedAsync(JobRun run)
    {
        // Called once the run succeeded, failed on its last attempt, or was canceled.
        return Task.CompletedTask;
    }
}
```

```csharp
services.AddScoped<IJobRunHandler, NewsletterJobRunHandler>();
```

## Workflows

With the [Workflows](../Workflows/README.md) feature:

- The **Run Job** task queues a run of a job that can be run from the admin, with a title and parameters written with Liquid, and can wait for the run to end. The workflow then continues with the **Succeeded** or **Failed** outcome, and the run as its last result and its `JobRun` output.
- The **Job Run Completed** event starts or resumes a workflow when a run ends, for one job or for all of them, and for the runs that succeeded or failed.
- The **Run Workflow** job starts an instance of a workflow. Its parameters are the `WorkflowTypeId` of the workflow, and optionally the `Inputs` of its input variables and a `CorrelationId`. Schedule it to run a workflow at the occurrences of a cron expression on a single server.

## Jobs of the Built-in Features

| Job | Feature | Queued by |
| --- | --- | --- |
| Audit Trail Trimming | Audit Trail | Its schedule, every day |
| Content Version Pruning | Content Version Pruning | Its schedule, every day, disabled by default |
| Workflow Trimming | Workflows | Its schedule, every day |
| Workflow Retry | Workflows | The retry of a task that faulted, when it is due |
| Run Workflow | Workflows | The admin, a schedule or a workflow |
| OpenID Tokens Pruning | OpenID Connect Authorization Server | Its schedule, every 30 minutes |
| Publish Later | Publish Later | The scheduled publish date of a content item |
| Archive Later | Archive Later | The scheduled archive date of a content item |
| Index Synchronization | Indexing | Resetting, rebuilding or synchronizing an index from the admin |
| Recipe | Recipes | Running a recipe from the admin |
| Deployment Package Import | Deployment | Importing a package from the admin |
| Data Pipeline | Data Pipelines | A run of a data pipeline |
| Data Pipeline Runs Cleanup | Data Pipelines | Its schedule, every hour |
| Shared Links Cleanup | Data Pipelines Core | Its schedule, every 15 minutes |

The Recipes and Deployment features use their job when the Jobs feature is enabled, and run in the request otherwise.
