---
title: Workflows
description: Automate what happens when something occurs on the site, such as emailing someone when a form is sent or a page is published, by drawing a workflow of events and tasks.
technical_manual:
  - reference/modules/Workflows/README.md
  - topics/workflows/README.md
  - reference/modules/Scripting/README.md
---

# Workflows

A workflow does work for you when something happens. *When* a visitor sends the contact form, *send* an email to the office and *show* a thank-you page. *When* an article is published, *notify* the editors. *Every night*, *run* a data pipeline. You draw a workflow in a designer as boxes (activities) joined by arrows (connections): one **event** that starts it, then **tasks** that run one after the other.

| | |
| --- | --- |
| **Menu** | **Design > Workflows**; **Settings > Workflow Trimming** |
| **Permission** | *Manage workflows*; *Execute workflows* to run them from the designer and retry instances; *Manage workflow settings* for trimming |
| **Feature** | **Workflows** (`OrchardCore.Workflows`), plus **HTTP Workflows Activities** (`OrchardCore.Workflows.Http`), **Timer Workflows Activities** (`OrchardCore.Workflows.Timers`) and **Session Workflows Activities** (`OrchardCore.Workflows.Session`) for their activities |


## Words you'll meet

| Word | Meaning |
| --- | --- |
| **Workflow** | The design: its activities, connections and settings. The list calls it a workflow type. |
| **Activity** | One box. An **event** waits for something to happen (a form is sent, content is published, a time is reached). A **task** does something (send an email, create content, make a decision). |
| **Start activity** | The event that starts the workflow. It shows a green **Start** badge. |
| **Outcome** | An exit of an activity, such as **Done**, **Failed**, **True** or **Valid**. You connect each outcome to the activity that comes next. |
| **Instance** | One run of a workflow. Each time the start event happens, a new instance runs. An instance that reaches an event waiting for something else (a timer, a user's action) stays **Halted** until it happens. |
| **Draft** and **version** | You edit a draft. **Publish** makes it the version that new instances use. Each publish creates a numbered version. |
| **Expression** | A value computed when the workflow runs, written in **Liquid** (`{{ Request.Form.Email }}`) or **JavaScript**. Fields that accept one say *With Liquid support*, or have a syntax list. |

## The workflows list

Open **Design > Workflows**. Each row shows the workflow's name and badges: **Disabled** when it's turned off, **No startup task** when no activity is marked as the start, and **Instances** when it has runs.

- **Edit** opens the designer. The **Instances** badge opens the runs of that workflow. **Delete** deletes it.
- The row's **Actions** menu has **Clone** (copy it under another name, with its settings) and **Export** (download it as a file, to import on another site; see [Import and export](../administration/import-and-export.md)).
- **Search** finds workflows by name. Tick rows and use **Actions > Delete** to delete several.
- **Instances** at the top lists the runs of every workflow.

## Create a workflow

1. Open **Design > Workflows** and click **Create Workflow**.
2. Type a **Name** that says what it does, such as *Email the office when the contact form is sent*.
3. Optionally open **More settings** (the defaults suit most workflows, and you can change them later on the designer's **Workflow** tab).
4. Click **Create and open the designer**.

| Field | What it does |
| --- | --- |
| **Name** | Required. The name in the list. |
| **Enabled** | Checked by default. Uncheck to stop the workflow from running, without deleting it. |
| **Instances at a time** | **Any number** (default). **One at a time**: the event doesn't start a new instance while another one waits. **One per correlated item**: the same, but per content item or user, for example one approval per article. |
| **Delete successfully completed workflows** | Deletes each instance once it finishes without error. Leave it unchecked to keep a history of runs. |
| **Usable as an activity** | Lets other workflows run this one with the **Execute Workflow Task**. Its variables marked **Input** and **Output** are what they pass in and get back. |
| **Outcomes with several transitions** | **Follow the first transition only** (default), or **Follow every transition** to run several branches from one outcome without a **Fork Task**. |
| **Fault the workflow on script errors** | Stops an instance at an activity whose script fails, so you can retry it. Unchecked, the workflow goes on with the script's fallback value and shows the error on the instance's page. |
| **Record activity data** | Checked for a workflow you create. Records, for each run, what every activity evaluated and changed, so you can see what happened. Uncheck it if the values are large or sensitive (personal data, keys). |
| **Lock timeout (ms)**, **Lock expiration (ms)** | Technical settings for sites running on several servers. Leave them at 0 unless your developer says otherwise. |

## The designer

The designer has a toolbar, the **Activities** list on the left, the canvas in the middle and a panel on the right.

| Toolbar item | What it does |
| --- | --- |
| **Workflows /** *name* | Back to the list. |
| **Version** *n* badge | The published version, the one new instances start on. |
| **All changes saved** / **Unsaved changes** | The designer saves your changes to the draft as you work. |
| Undo, Redo | Undo (Ctrl+Z) or redo (Ctrl+Y) your last changes. |
| **Versions** | Lists every published version: **View** it, **Compare** it with another, or **Restore** it into the draft. |
| **Instances** | The runs of this workflow. |
| **Export** | Downloads the published workflow (not the draft) as a file. |
| **Run…** | Runs the published version now, for workflows that start with an **Http Request Event** or a **Started By Workflow Event**. See [Run a workflow to test it](#run-a-workflow-to-test-it). |
| **Discard draft** | Throws away every change since the last publish. |
| **Publish** | Makes the draft the version new instances use. |

The right panel has three tabs:

- **Variables**: values the workflow keeps while it runs. **Add a variable**, then set its **Name**, **Type** (**Text**, **Number**, **Yes or no**, **Date and time**, **Object**, **List** or **Any**), **Default value** and **Description**. Mark it **Input** to receive a value when another workflow (or the **Run…** dialog) starts this one, or **Output** to return it. Read a variable in Liquid with `{{ Workflow.Variables.name }}`.
- **Workflow**: the settings described in [Create a workflow](#create-a-workflow). Click **Apply** to save them.
- **Issues**: problems found in the workflow. **Errors** must be fixed before you can publish; **Warnings** don't block publishing. Click an issue to go to its activity. *No issues found.* means all is well.

### Add the start event

A new workflow shows **Start with an event**, with common events: **Content Published Event**, **Content Created Event**, **Http Request Event**, **Timer Event**, **User Created Event** and **Signal Event**.

1. Click the event that starts your workflow, or drag any event from the **Activities** list onto the canvas.
2. Its settings open in the activity panel below the canvas. Fill them in and click **Apply**.

The event you pick there becomes the start activity. To change it, right-click another event and choose **Set as start activity** (or **Unset start activity** on the current one).

### Add the next activity

1. On an activity, click the small circle next to an outcome, such as **Done**.
2. In **Add the next activity, after Done**, type to search, and click the activity.

The activity is added, connected to that outcome, and its settings open. You can also click or drag an activity from the **Activities** list (use **Search activities** and the **All**, **Events** and **Tasks** buttons to filter it), then connect it.

### Connect, edit and delete activities

- **Connect**: drag from an outcome's circle to another activity, or right-click the activity and choose **Connect an outcome to…**, then pick the **Outcome** and the **Target activity** and click **Connect**. Each outcome has one connection, unless the workflow follows every transition; connecting it again replaces the old one.
- **Edit**: double-click an activity, or click its settings icon. Change the fields and click **Apply**; *Saved!* confirms. Every activity has an optional **Title** to rename the box.
- **Available data**: next to **Settings** in the activity panel, lists the values the activity can use (the variables, the last result, the request, the content item...). Click one to insert its expression in the field you were editing.
- **Retries**: tasks have a **When it fails** section. Set **Retries** (up to 10), a **Delay (seconds)**, whether the **Delay between retries** stays **The same each time** or is **Twice as long each time**, and what happens **Once the retries are spent**: **Fault the workflow** or **Follow the Failed outcome**.
- **Delete**: select an activity (or a connection) and press Delete, use the trash icon in the activity panel, or right-click and choose **Delete**. The message that confirms it has an **Undo** button.
- **Move around**: drag activities to arrange them. The zoom buttons at the bottom right zoom in and out, reset to 100% and **Fit to content**. Right-click an activity to collapse or show the activities after it.

### Publish

Click **Publish**. If the workflow has warnings, the designer lists them and asks you to **Publish anyway**; errors must be fixed first. Instances already running keep the version they started on; new instances use the new one. *The workflow was published as version n.* confirms it.

## Example: email the site owner when a form is sent

This workflow receives a contact form (see [Forms](forms.md#build-a-contact-form)), emails what the visitor typed, and shows a thank-you page. Email sending must be set up first; see [Email](../settings/email.md).

1. Open **Design > Workflows**, click **Create Workflow**, name it *Contact form*, and click **Create and open the designer**.
2. Under **Start with an event**, click **Http Request Event**.
3. In its settings, set **HTTP Method** to **POST**. Keep **Validate Antiforgery Token** checked.
4. Click **Regenerate** next to **URL**, then **Copy**. Keep the URL (the part that starts with `/workflows/Invoke?token=`).
5. Click **Apply**.
6. Click the circle next to **Done** on the event, and choose **Email Task** in the list.
7. In its settings, fill in:
    - **Recipients**: your address, such as `office@example.com`.
    - **Subject**: `Contact form: {{ Request.Form.Name }}`
    - **Text Body**: the values you want, such as `{{ Request.Form.Name }} ({{ Request.Form.Email }}) wrote: {{ Request.Form.Message }}`
8. Click **Apply**.
9. Click the circle next to **Done** on the **Send Email** activity, and choose **Http Redirect Task**.
10. Set **Location** to the address of your thank-you page, such as `/thank-you`, and click **Apply**.
11. Click **Publish**.
12. Edit the page that holds the form, paste the URL from step 4 in the Form widget's **Action**, and publish the page.

When a visitor sends the form, you get the email and they see the thank-you page. `{{ Request.Form.Name }}` is the value of the field whose **HTML Name** is `Name`. To make fields required and show errors, add the validation tasks described in [Forms](forms.md#handle-what-visitors-send) between the event and the email.

!!! note
    The **Email Task** also has a **Failed** outcome, followed when the email can't be sent (for example when no email provider is set up). Connect it to a redirect too, or the visitor gets an empty page when sending fails.

## Run a workflow to test it

**Run…** in the toolbar runs the published version, without waiting for the real event.

- For a workflow that starts with **Started By Workflow Event**, fill in the values of its input variables.
- For a workflow that starts with an **Http Request Event**, the dialog sends a request as a browser would. It lists the **Query string parameters** and **Form fields** the workflow reads (such as `Name`), each with a value box. **Add a value** sends another one. For POST, choose **Form fields** or **JSON or text** for the body.

Click **Run**. The **Result** shows the **Response** and the **Instance** status; **Open the instance** shows what it did.

!!! warning "Antiforgery tokens"
    The **Run…** dialog doesn't send an antiforgery token. A workflow whose **Http Request Event** has **Validate Antiforgery Token** checked answers **400** and doesn't start. To test it from the dialog, uncheck the option, publish, test, then check it again and publish.

Other events can't be run from the dialog: they start the workflow when what they wait for happens.

## Follow the runs of a workflow

Open **Instances** from the designer's toolbar or the workflow's row, or the **Instances** button at the top of the list for all workflows.

- The **Instances by status** counts at the top filter the list: **All**, **Halted** (waiting for an event), **Running**, **Faulted** (stopped by an error), **Finished** and **Aborted** (cancelled).
- The filters narrow the list by workflow, status and creation date (**Any time**, **Last 24 hours**, **Last 7 days**, **Last 30 days**), and sort it by **Recently created** or **Least recently created**.
- Each row shows the instance's id, its status, the version it runs on, and when it was created. **Restart** runs it again from its start with the same input; **Delete** deletes it.
- Tick rows and use **Actions**: **Retry** runs faulted instances again from the activity that faulted, **Cancel** aborts instances that haven't finished, **Delete** deletes them.

Click an instance to open its page. The **Workflow** tab shows the **Workflow ID**, **Created** date, **Status**, **Correlation ID** (the content item or user it's about, if any) and, for a faulted one, the **Fault Message**. Below, the designer shows the workflow read-only: the activities and connections the instance ran are highlighted, and the activities it waits on are marked **Blocking**. Its **Journal** tab lists the activities it ran, in order, with their status (**Completed**, **Waiting**, **Faulted**, **Failed**, **Retrying**); select an activity to see, on its **Runs** tab, what it evaluated and changed (when **Record activity data** is on). A faulted activity offers **Retry from here**. The **State** tab shows the raw data of the instance.

## Choose an activity

The **Activities** list groups activities by category. A category appears only when the feature that provides it is on. Events start or resume a workflow; tasks do the work. For what each activity does and the feature it needs, see [Workflow activities](workflow-activities.md).

## Trim old instances

Instances pile up over time. A background task deletes old ones once a day.

1. Open **Settings > Workflow Trimming**.
2. Set the **Retention period**: the number of days an instance is kept (90 by default).
3. Under **Statuses**, check the statuses of the instances to delete: **Idle**, **Starting**, **Resuming**, **Executing**, **Halted**, **Finished**, **Faulted**, **Aborted**.
4. Click **Save**.

**Last run** shows when the task last ran. Check **Disable** to stop trimming.

!!! warning
    Until you save this page once, trimming deletes instances of **every** status older than 90 days, including halted instances still waiting for an event, even though no box is checked on the screen. Once you save, only the checked statuses are trimmed, and saving with no status checked trims nothing. Leave **Halted** unchecked if some workflows wait longer than the retention period (for example for an approval).

## Permissions

| Permission | What it allows | Given by default to |
| --- | --- | --- |
| *Manage workflows* | Create, edit, publish, clone, export and delete workflows; see, restart, cancel and delete instances. | Administrator |
| *Execute workflows* | Run workflows from the designer's **Run…** dialog, and retry faulted instances. | Administrator, Editor |
| *Manage workflow settings* | Change **Settings > Workflow Trimming**. Anyone with *Manage workflows* has it too. | Administrator |

*Manage workflows* is a powerful permission: a workflow can create users, give roles, create tenants and call other servers. Give it only to people you trust with the whole site.

!!! example "See it in practice"
    [Add a contact form](../use-cases/contact-form.md) walks through this step by step.
