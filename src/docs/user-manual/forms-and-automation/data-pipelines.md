---
title: Data pipelines
description: Design pipelines that read data from the site or a file, transform it, and deliver it as files, emails, uploads or content items, then run them and follow their history.
technical_manual:
  - reference/modules/DataPipelines/README.md
  - reference/modules/DataSources/README.md
  - reference/modules/Workflows/README.md
---

# Data pipelines

A data pipeline moves data for you, without code. It reads rows from a source, such as the blog posts of the site, its users, a saved query or a CSV file of the media library, changes them along the way (filters, calculations, sorting, grouping), and delivers the result: an Excel or CSV file saved to the media library, emailed, shared through a download link or uploaded to a server, or rows saved as content items. You draw the pipeline as boxes (steps) joined by arrows (connections) in a designer, publish it, and run it whenever you need, or on a schedule with a workflow.

| | |
| --- | --- |
| **Menu** | **Tools > Data Pipelines > Pipelines**, **Tools > Data Pipelines > Run History**, **Tools > Shared Links** |
| **Permission** | *View data pipelines and their runs*, *Run data pipelines*, *Manage data pipelines*, *Manage shared links* |
| **Feature** | **Data Pipelines** (`OrchardCore.DataPipelines`), **Data Pipelines - FTP** (`OrchardCore.DataPipelines.Ftp`), **Data Pipelines - SFTP** (`OrchardCore.DataPipelines.Sftp`), **Data Pipelines Core** (`OrchardCore.DataPipelines.Core`) |


## How a pipeline works

A pipeline is made of **steps** of four kinds, listed by category in the designer:

1. **Sources** read rows.
2. **Transforms** change the rows: filter, calculate, select, sort, group, join, combine, remove duplicates, keep the first ones.
3. **Files** write the rows to a file (CSV, Excel workbook, JSON or JSON Lines) or zip files together.
4. **Destinations** deliver the files or the rows.

Each step has input and output **ports**. A port carries either **Rows** (data with named, typed fields) or **Files**. You connect an output to an input of the same kind, so a **Create a file** step usually sits between the transforms and the destinations that deliver files. One output can feed several steps, which each get all of its data.

Some steps are available only when another feature is on:

| Feature that must be on | Steps it adds |
| --- | --- |
| **Media** | **Read a media file**, **Save to the media library** |
| **Email** | **Send by email**, **Share a download link** |
| **Contents** | **Save content items** |
| **Data Pipelines - FTP** | **Upload to an FTP server** |
| **Data Pipelines - SFTP** | **Upload to an SFTP server** |
| **Workflows** | The **Run Data Pipeline Task** and **Data Pipeline Run Completed Event** activities, see [Run a pipeline from a workflow](#run-a-pipeline-from-a-workflow) |

What a pipeline can read depends on the data sources of the site; see [Data sources](data-sources.md).

!!! warning "Publishing vouches for a pipeline"
    A published pipeline runs with the access of the user who published it: it reads and writes what that user may read and write. That is why *Manage data pipelines* is a security critical permission. Give it only to people you trust with that access.

## Find a pipeline

Open **Tools > Data Pipelines > Pipelines**. Type in the **Search** box to find pipelines by name. Each row shows:

- **Draft**, when the pipeline was never published, or **Version 1**, **Version 2** and so on, the published version that runs.
- **Unpublished changes**, when the draft has changes that are not published yet.
- **Disabled**, when the pipeline is turned off.
- The status of its last run, such as **Succeeded**, and how long ago it was queued. Click it to open that run.
- When it was last changed, and its description, if it has one.

Click **Edit** (or **View**, if you can't manage pipelines) to open the designer. The row's **Actions** menu has:

| Action | What it does |
| --- | --- |
| **Run now** | Runs the published version. Shown only when the pipeline is published and enabled, and you have *Run data pipelines*. You confirm with **Run**. |
| **Run history** | Lists the runs of this pipeline. |
| **Start over** | Forgets what past runs remembered, such as how far a source read new rows, so the next run reads every row again and may deliver data again. Not possible while a run is queued or running. |
| **Delete** | Deletes the pipeline, all its versions and its run history, after you confirm. |

## Add a pipeline

1. Open **Tools > Data Pipelines > Pipelines** and click **Add Pipeline**.
2. Type a **Name**. It is required.
3. Click **Create**.

The designer opens on the empty pipeline. Add a description and the steps there.

## Find your way in the designer

The designer fills the page:

- **Toolbar** (top): a **Data Pipelines** link back to the list, the pipeline name, the published version, **Add a data source**, the save status (**Saving…**, **Draft saved**, **No unpublished changes**, **Not saved**), **Undo** and **Redo**, **Versions**, **Run**, **Discard draft** and **Publish**.
- **Steps** toolbox (left): every available step by category (**Sources**, **Transforms**, **Files**, **Destinations**), each with what it does and its ports (for example *In: Rows · Out: Files*). Use **Search the steps** to filter it. The double arrow collapses it.
- **Canvas** (middle): your steps and their connections. The buttons at the bottom right zoom in and out, reset the zoom and fit the steps on the screen.
- **Step panel**: opens over the canvas when you open a step, with three tabs, **Settings**, **Data** and **Fields**. The pin button keeps it below the canvas instead, the trash button deletes the step, and the cross closes it.
- **Pipeline panel** (right): three tabs, **Pipeline**, **Issues** and **History**.

You don't need to save. The designer saves your draft a moment after each change. If another person changed the same draft after you opened it, the status shows **Changed elsewhere**; click **Resolve** and choose **Reload** (show their changes and lose your unsaved ones) or **Overwrite** (save yours over theirs).

## Name, describe or disable a pipeline

1. In the designer, open the **Pipeline** tab on the right.
2. Change the fields and click **Apply** (changes are also applied shortly after you type).

| Field | What it does |
| --- | --- |
| **Name** | Required. The name shown in the list and the run history. |
| **Description** | What the pipeline does, shown under its name in the list. |
| **Enabled** | Clear it to stop the pipeline from running, whether a person or a workflow starts it. |

## Add the first step

An empty pipeline shows **Start with a source** with two buttons, **Read a data source** and **Read a media file**.

To read site data:

1. Click **Add a data source** in the toolbar (or **Read a data source**).
2. In the **Add a data source** dialog, pick a data source on the left (**All**, **Content items**, **Users**, **Queries**), or type in **Filter** to search by name or description.
3. Click **Add** on the data set you want, for example **Blog Post**.

A **Read a data source** step that reads this data set appears on the canvas, and its settings open. A pipeline can read several data sets, for example to join them.

## Add the next steps and connect them

You can add a step in three ways:

- **From the output of a step**: click the round port on the right of a step. A list of the steps that can receive its rows or files opens, with **Search the steps**. Pick one: it is added after the step, already connected.
- **From the toolbox**: drag a step onto the canvas, or click it to add it. Then connect it.
- **From the step's menu**: right-click a step and choose **Add a step after it…**.

To connect two steps, drag from an output port (right side) to an input port (left side) of another step, or right-click the step and choose **Connect an output to…**. The designer refuses a connection that would send rows to a files input (or the reverse), connect a step to itself, or make the pipeline loop. An input that accepts one connection replaces its previous connection.

To remove a connection, right-click it and choose **Delete the connection**, or right-click the step and choose **Disconnect from ...**.

## Edit a step

1. Double-click the step, or click its settings button (top right of the step), or right-click it and choose **Edit**.
2. On the **Settings** tab, change the settings. Every step has an optional **Title**, shown on the canvas instead of the name of the step. Changes are applied as you make them; you can also click **Apply**. If the form has errors, they are shown and the changes aren't applied until you fix them.
3. Use the **Data** tab and click **Preview** to see the first rows the step produces (see [Preview the data](#preview-the-data)).
4. Use the **Fields** tab to see the fields the step **Received** and **Produced**, with their types. Click a field's reference, such as `[TitlePart.Title]`, to copy it for a formula.

## Delete steps

Select a step and press **Delete**, click the trash button of the step panel, or right-click it and choose **Delete**. Select several steps to delete them together. A message offers **Undo**, and the toolbar **Undo** (Ctrl+Z) and **Redo** (Ctrl+Y) buttons undo and redo any change.

## Preview the data

The **Data** tab of a step runs the pipeline up to that step, with the draft's settings, on a sample of the rows, and shows what the step produces: a table of rows, or the files it creates with their size. Click **Preview again** after a change. A preview:

- reads the data with your own access, not the publisher's;
- reads only a sample of each source (100 rows unless IT changed it);
- never runs destinations: previewing a destination shows what it would receive;
- never changes what runs remember for **Read only new rows**.

## Check the issues

The **Issues** tab lists what must be fixed before the pipeline can be published (errors, such as a missing setting, a field that doesn't exist or a required input that isn't connected) and what deserves a look (warnings, such as a step whose output nothing uses). Steps with issues show a badge with their number. Click an issue to open its step. A step shown as **Not available** belongs to a feature that is turned off.

## Publish a pipeline

Runs never use the draft. They use the published version.

1. Click **Publish** in the toolbar.
2. If the pipeline has warnings, the dialog lists them: click **Publish anyway** or **Cancel**. If it has errors, it can't be published: fix the errors listed in the **Issues** tab first.

The toolbar now shows **Version 1** (then 2, 3...), and the list shows the pipeline with that version.

- **Discard draft** throws away the changes made since the pipeline was last published, after you confirm.
- **Versions** lists the published versions with who published them and when. **View** opens a version read-only; **Restore** copies it into the draft, which runs once you publish again.

## Run a pipeline now

You need *Run data pipelines*, and the pipeline must be published and enabled.

1. Click **Run** in the designer toolbar, or **Run now** on the **History** tab, or choose **Run now** in the pipeline's **Actions** menu on the list and confirm with **Run**.
2. The run is queued and runs in the background, so you can keep working. It always uses the published version, not unpublished changes in the draft.

From the designer, the **History** tab follows the run: it shows its status, how long it took, each step with its status and what it produced, the deliveries and the log. Select a run there to see on the canvas what each step did. **Open the run** opens its page, **Refresh** updates the list, and **Run history** opens all the runs of the pipeline.

A few things to know about runs:

- One run of a pipeline runs at a time; other runs of the same pipeline wait in the queue.
- A step that fails stops the run.
- If the user who published the pipeline is deleted or disabled, its runs fail until someone publishes it again.
- Files a run creates are kept only while it runs. To keep a file, deliver it with a destination step.

## Schedule a pipeline

Pipelines have no schedule setting of their own. To run one on a schedule, use a workflow (see [Workflows](workflows.md)):

1. Make sure **Timer Workflows Activities** (`OrchardCore.Workflows.Timers`) is on.
2. Create a workflow that starts with a **Timer Event** and set its **CRON Expression**, for example every night.
3. Add a **Run Data Pipeline Task** and choose the pipeline.
4. If the timer may fire while a run is still going, check **Skip when a run is already queued or running** on the task.

## Read the run history

Open **Tools > Data Pipelines > Run History** to see the runs of every pipeline, the most recent first, or choose **Run history** in a pipeline's **Actions** menu to see only its runs. Use **Search by pipeline** to find runs by pipeline name, and the **Status** menu to show **Every status** or only **Queued**, **Running**, **Succeeded**, **Failed** or **Cancelled** runs.

Each row shows the pipeline, the status, the version that ran, when it was queued, how long it took, and what started it (**Manual** or **Workflow**) and who. A failed run shows its error under the row. Click **Details** to open the run. The **Actions** menu has **Open the pipeline**, **Run history of the pipeline** and, for a queued or running run, **Cancel the run**.

| Status | Meaning |
| --- | --- |
| **Queued** | Waiting to start. |
| **Running** | Running now. |
| **Succeeded** | Every step completed. |
| **Failed** | A step failed, or the pipeline had errors. |
| **Cancelled** | Someone cancelled the run. |

## View one run

The page of a run (**Run of** *pipeline name*) refreshes on its own while the run is going. It shows:

- the status and version, and when the run was **Queued**, **Started** and **Ended** (in UTC), with how long it took, what started it and who, and the **Error** that made it fail;
- **Steps**: each step with its status (**Pending**, **Running**, **Succeeded**, **Failed**, **Cancelled** or **Skipped**), **Rows in**, **Rows out**, **Files** and **Warnings**;
- **Delivered**: what the destinations delivered, such as a file saved to the media library or an email sent. When a delivery has an address, it is a link you can open;
- **Log**: the messages of the run, with warnings and errors highlighted. For example, an SFTP step logs the fingerprint of the server here.

**Open the pipeline** opens the designer. **Cancel** (with *Run data pipelines*) cancels a queued or running run: a queued run stops at once, a running one at its next batch of rows.

Runs are deleted 30 days after they end, unless IT changed this.

## Read only new rows

To deliver only what changed since the last successful run, open the **Read a data source** step and set **Read only new rows** to a field that grows whenever a row is added or changed, such as **By Modified** for content items. Each successful run remembers the largest value it read, and the next run reads only rows with a larger value. The first run reads every row. A failed or cancelled run doesn't move this mark, so the next run reads the same rows again.

To read everything again, for example after the destination was emptied, choose **Start over** in the pipeline's **Actions** menu.

## Choose the steps

Every step, its settings and the placeholders you can use in file names, folders and email texts are described in [Data pipeline steps](data-pipeline-steps.md), along with how passwords and keys are stored and how to keep FTP and SFTP uploads secure.

## Run a pipeline from a workflow

When **Workflows** is on, the workflow editor offers two activities in the **Data** category. See [Workflows](workflows.md) for building workflows.

**Run Data Pipeline Task** queues a run of the published version of a pipeline:

| Field | What it does |
| --- | --- |
| **Data pipeline** | The pipeline to run. |
| **Parameters** | Values passed to the run, one `name=value` per line, with Liquid. Steps use them with `{Parameter:name}`, such as in the filters of **Read a data source**. |
| **Wait for the run to end** | Checked: the workflow waits, then continues with **Succeeded** or **Failed**. Cleared: it continues at once with **Queued**. |
| **Skip when a run is already queued or running** | The workflow continues with **Skipped** instead of queuing another run. |

The task ends with **Failed** at once when the pipeline doesn't exist, isn't published or is disabled.

**Data Pipeline Run Completed Event** starts or resumes a workflow when a run ends, for example to notify someone when a run fails:

| Field | What it does |
| --- | --- |
| **Data pipeline** | One pipeline, or **Any data pipeline**. |
| **Runs** | **Every run**, **Runs that succeeded**, or **Runs that failed or were cancelled**. |

Runs started by a workflow show **Workflow** as what started them in the run history.

## Manage shared links

The **Share a download link** step keeps the file on the site and emails each recipient a link to it. **Tools > Shared Links** lists these links, so you can see who received what and stop a link before it expires. See [Shared links](shared-links.md).

## Copy pipelines to another site

When **Deployment** is on, deployment plans have a **Data Pipelines** step that exports every pipeline (**Include all the data pipelines**) or the ones you check. Runs, history, shared links and passwords are not exported. An imported pipeline arrives as a draft: enter its passwords again and publish it. See [Import and export](../administration/import-and-export.md).

## Permissions

| Permission | What it allows | Default roles |
| --- | --- | --- |
| *View data pipelines and their runs* | See the list, open pipelines and their versions read-only, see the run history. | Administrator |
| *Run data pipelines* | Run published pipelines and cancel runs. Includes viewing. | Administrator |
| *Manage data pipelines* | Create, design, preview, publish, start over and delete pipelines. Includes running and viewing. Security critical. | Administrator |
| *Manage shared links* | See, revoke and delete shared links on **Tools > Shared Links**. | Administrator |

To give these to other roles, see [Roles](../users/roles.md).

## Ask IT to change the limits

The size of previews, how many runs execute at once, how long runs are kept, and the memory a step may use are set in the site configuration, and the list of file extensions the media library accepts too. Your developer or IT team sets these up; see the Technical Manual.
