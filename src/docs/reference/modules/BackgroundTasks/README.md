# Background Tasks (`OrchardCore.BackgroundTasks`)

This module provides tools to manage background tasks. This includes an admin UI to show which background tasks are registered with the ability to enable and disable them.

After enabling the feature, you'll be able to manage background tasks under Tools → Background Tasks. This includes the following:

- Enable or disable a task.
- Change the schedule of the task, i.e. how frequently it runs, with [cron expressions](https://en.wikipedia.org/wiki/Cron#Cron_expression).
- Whether to build the tenant routing pipeline for the task, allowing it to generate correct routes.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Rx11bdawew0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Remote administration

The enabled feature exposes registered tenant tasks at `api/background-tasks`.
Every operation requires API bearer authentication, `AccessRemoteManagement`, and
`ManageBackgroundTasks`. Pomi and MCP use the same existing tenant credentials;
enabling Background Tasks does not configure remote-management authentication.

```bash
pomi background-tasks list --take 200
pomi background-tasks list --enabled false
pomi background-tasks show <registered-task-name>
pomi background-tasks validate --body-file task.json
pomi background-tasks update <registered-task-name> --body-file task.json
pomi background-tasks disable <registered-task-name>
pomi background-tasks enable <registered-task-name>
```

List supports `search`, `enabled`, nonnegative `skip`, and `take` from 1 to 200
(default 50). Task names come from the registered implementation; copy the returned
`name` rather than inferring it from the display title. Responses contain `name`,
`title`, `enabled`, and `configuration`. Effective configuration combines task
defaults with tenant overrides; stale settings for unregistered tasks are not listed.

Read `configuration` before updating. Updates replace that complete configuration
while preserving enabled status and the task's registered identity/title:

```json
{
  "schedule": "*/15 * * * *",
  "description": "Run every fifteen minutes",
  "lockTimeout": 3000,
  "lockExpiration": 30000,
  "usePipeline": false
}
```

`validate` reports `isValid` and field-keyed `errors` without saving or running a
task. The cron expression uses the same five-field parser as the existing
scheduler, with the scheduler's existing tenant time-zone behavior. Lock timeout
and expiration are nonnegative milliseconds; locking is atomic only when both are
positive and a distributed lock is registered. Omitted numeric/Boolean properties
reset to zero/false, and an omitted description becomes null. Schedule is required.

The existing administration editor now uses this shared validation and rejects
invalid cron or negative lock settings before saving. Invalid persisted settings
can be repaired through update; disabling remains available, but enabling requires
valid settings. Identical updates and status changes do not rewrite the document.
Successful changes use the existing deferred settings signal so the scheduler
reloads its configuration.

Disabling affects future scheduling and does not cancel running work. These
operations do not offer immediate execution, cancellation, or execution history.
