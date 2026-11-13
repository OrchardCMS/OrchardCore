---
title: Background tasks
description: See the jobs the site runs on a schedule, run one now, turn tasks on or off, and change their schedule.
technical_manual:
  - reference/modules/BackgroundTasks/README.md
---

# Background tasks

Background tasks are jobs the site runs on its own, on a schedule, without anyone opening a page: publishing content scheduled for later, cleaning up old audit trail events, abandoned uploads and old content versions, refreshing search indexes, running timed workflows. The features you turn on add their own tasks. **Tools > Background Tasks** shows them all, tells you when each one last ran and whether it worked, and lets you run, pause or reschedule them.

| | |
| --- | --- |
| **Menu** | **Tools > Background Tasks** |
| **Permission** | *Manage background tasks* (the Administrator role has it by default) |
| **Feature** | **Background Tasks** (`OrchardCore.BackgroundTasks`) |


## Read the list of tasks

Open **Tools > Background Tasks**. Each row shows:

- The task's title and description.
- **Enabled** or **Disabled**.
- **Last run**: **never**, or the result (**Succeeded**, **Failed** or **Canceled**), how long ago, and how long it took. When the last run failed, the error is shown too.
- **Next run**: when it runs next, **due** (it is about to run), **not scheduled** (the task is disabled), or **never** (the schedule never occurs).
- **Running** or **Queued** while the task runs or waits to run, and the number of **Failed runs** out of all runs.

Type in the search box to find a task by name, and use the status filter to show only the tasks that are **Enabled**, **Disabled**, **Running or queued**, or whose **Last run failed**.

!!! note
    The run history and counters are kept in memory by the server that answers you, since the site last started. They start from zero after a restart, and on a site with several servers each one shows its own history.

## Run a task now

1. In the task's row, open the **Actions** menu and click **Run now**.

The task is queued and runs within a moment: *The task '...' has been queued and will run shortly.* Refresh the page to see the result. If the task is already running or queued, the screen tells you so.

To run several tasks, check them, then pick **Run now** in the **Actions** menu above the list.

## Turn a task off or on

1. In the task's row, click **Disable** (or **Enable** for a disabled task).

You see *The task has been disabled.* (or *enabled*). A disabled task doesn't run on its schedule. To change several tasks, check them, then pick **Enable** or **Disable** in the **Actions** menu above the list.

!!! warning
    Many features depend on their task. For example, while **Content Items Archiver** is disabled, content items scheduled to be archived stay online. Turn a task off only when you know what it does.

## See the details of a task

1. In the task's row, open the **Actions** menu and click **Details**.

The details screen has three sections:

| Section | What it shows |
| --- | --- |
| **Execution** | The **Status** (**Idle**, **Running** or **Queued**), the **Last run** and its result, duration, error and who triggered it, the **Next run**, and the numbers of **Completed runs** and **Failed runs**. |
| **Settings** | The **Technical name**, whether it is enabled, the **Schedule** (and the default schedule if you changed it), **Use tenant pipeline**, and **Atomic**, which says whether the task takes a lock so only one server runs it at a time. |
| **Recent runs** | The latest runs with when they **Started**, their **Duration**, **Result**, **Triggered by**, and **Error**. |

From the details screen you can also click **Edit**, or use **Actions** to **Run now**, **Enable** or **Disable** the task.

## Change the schedule of a task

1. In the task's row, click **Edit**.
2. On the **Properties** tab, change the **Schedule**.
3. Click **Save**.

You see *The task has been updated.* The **Next run** in the list follows the new schedule.

| Field | What it does |
| --- | --- |
| **Title** | The name shown in the list. |
| **Description** | The description shown in the list. |
| **Schedule** | When the task runs, as a cron expression (see below). It must be valid, otherwise you see *The schedule must be a valid cron expression.* Click **Reset Schedule** to go back to the task's default schedule. |
| **Use Tenant Pipeline** | For developers. Prepares the site's routes before the task runs, so the task can build page addresses. Leave it as it is. |

On the **Advanced** tab:

| Field | What it does |
| --- | --- |
| **Lock timeout (ms)** | How long, in milliseconds, the task waits to get its lock before giving up. 0 means no lock. |
| **Lock expiration (ms)** | How long, in milliseconds, the lock lasts. 0 means no lock. |

The lock matters only on a site that runs on several servers with a shared lock service set up by IT; otherwise these values are ignored. Leave them unless your IT team asks.

### Write a cron schedule

A cron expression has five parts, separated by spaces: minute, hour, day of the month, month, and day of the week. `*` means "every". The times follow the site's **Default Time Zone** in [General settings](general.md).

| Schedule | Runs |
| --- | --- |
| `* * * * *` | Every minute. |
| `*/5 * * * *` | Every 5 minutes. |
| `0 * * * *` | Every hour, on the hour. |
| `0 0 * * *` | Every day at midnight. |
| `30 2 * * 1` | Every Monday at 2:30 in the morning. |
| `0 6 1 * *` | On the first day of every month at 6:00. |
