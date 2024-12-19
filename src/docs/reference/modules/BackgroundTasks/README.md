# Background Tasks (`OrchardCore.BackgroundTasks`)

This module provides tools to manage background tasks. This includes an admin UI to show which background tasks are registered with the ability to enable and disable them.

After enabling the feature, you'll be able to manage background tasks under Configuration → Tasks → Background Tasks. This includes the following:

- Enable or disable a task.
- Change the schedule of the task, i.e. how frequently it runs, with [cron expressions](https://en.wikipedia.org/wiki/Cron#Cron_expression).
- Whether to build the tenant routing pipeline for the task, allowing it to generate correct routes.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Rx11bdawew0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
