---
title: Workflow activities
description: Every event and task you can add to a workflow, by category, with what it does and the feature it needs.
technical_manual:
  - reference/modules/Workflows/README.md
  - topics/workflows/README.md
  - reference/modules/Scripting/README.md
---

# Workflow activities

This page lists every activity of the workflow designer. The **Activities** list groups activities by category, and a category appears only when the feature that provides it is on. Events start or resume a workflow; tasks do the work. To build a workflow and connect its activities, see [Workflows](workflows.md).

| | |
| --- | --- |
| **Menu** | **Design > Workflows**, then the **Activities** list of the designer |
| **Permission** | *Manage workflows* |
| **Feature** | **Workflows** (`OrchardCore.Workflows`), plus **HTTP Workflows Activities** (`OrchardCore.Workflows.Http`), **Timer Workflows Activities** (`OrchardCore.Workflows.Timers`) and **Session Workflows Activities** (`OrchardCore.Workflows.Session`) for their activities |

## Background

| Activity | What it does |
| --- | --- |
| **Catch Workflow Fault Event** | Starts when another workflow faults, to notify someone or log it. |
| **Timer Event** | Starts (or resumes) on a schedule written as a **CRON Expression**, such as `0 7 * * 1` for every Monday at 7:00. The schedule is in UTC unless you check **Use site time zone**. The timer is checked once a minute. Needs **Timer Workflows Activities**. |

## Content

| Activity | What it does |
| --- | --- |
| **Content Created Event**, **Content Updated Event**, **Content Save Draft Event**, **Content Published Event**, **Content Unpublished Event**, **Content Versioned Event**, **Content Deleted Event** | Start when a content item is created, updated, saved as a draft, published, unpublished, versioned or deleted. **Content Types** limits them to some types; empty means every type. |
| **Create Content Task**, **Retrieve Content Task**, **Update Content Task**, **Publish Content Task**, **Unpublish Content Task**, **Delete Content Task** | Create, load, change, publish, unpublish or delete a content item. |
| **User Task Event** | Pauses the workflow until someone clicks a button on the content item's editor. You list the **Actions** (such as `Approve, Reject`), and optionally the **Roles** allowed to use them; each action becomes an outcome and a button such as **Approve and Save**. |

## Control Flow

| Activity | What it does |
| --- | --- |
| **If Else Task** | Evaluates a **Condition** and goes on with **True** or **False**. |
| **Fork Task** | Splits the workflow into parallel branches, one outcome per name in **Forks**. |
| **Join Task** | Joins branches back together, waiting for all of them (**WaitAll**) or the first one (**WaitAny**); outcome **Joined**. |
| **For Each Task** | Repeats the **Iterate** outcome for each item of a list, then **Done**. |
| **For Loop Task** | Repeats **Iterate** from a start value to an end value, then **Done**. |
| **While Loop Task** | Repeats **Iterate** while a condition is true, then **Done**. |
| **Script Task** | Runs a JavaScript **Script** that picks one of its **Available Outcomes**. |
| **Liquid Task** | Evaluates a Liquid template and keeps the result as the last result. |

## Data

| Activity | What it does |
| --- | --- |
| **Data Pipeline Run Completed Event** | Starts when a run of a data pipeline ends. |
| **Run Data Pipeline Task** | Runs a data pipeline, and can wait for the run to end. See [Data pipelines](data-pipelines.md). |

## HTTP

Most need **HTTP Workflows Activities**.

| Activity | What it does |
| --- | --- |
| **Http Request Event** | Starts when someone calls its secret **URL**, such as a form or another service (a webhook). Settings: **HTTP Method**, **URL** (**Regenerate**, **Copy**), **Validate Antiforgery Token** (keep it checked for forms, uncheck it for other services), **Token Life Span** (days the URL works, 0 for ever; click **Regenerate** after changing it) and **Form Location Key**. Anyone who has the URL can start the workflow, so keep it private. |
| **Http Request Filter Event** | Runs when a request of the site matches its **HTTP Method**, **Controller**, **Action** and **Area** (technical names your developer can give you), before the page runs; outcome **Matched**. |
| **Signal Event** | Resumes a waiting workflow when a signal with its **Name** is triggered, for example from a link the workflow emailed (made with the Liquid `signal_url` filter). |
| **Http Request Task** | Calls a URL on another server, such as a web API. |
| **Http Response Task** | Writes the response sent back to the caller. |
| **Http Redirect Task** | Sends the visitor to another page (**Location**); check **Permanent** for a 301 redirect. |
| **Http Redirect To Form Location Task** | Sends the visitor back to the page of the form they sent. Provided by **Forms**. |

## Messaging

| Activity | What it does |
| --- | --- |
| **Email Task** | Its card on the canvas reads **Send Email**. Sends an email: **From**, **Sender**, **ReplyTo**, **Recipients**, **Cc**, **Bcc**, **Subject**, **Format** (**All**, **Text** or **Html**), **Text Body** and **HTML Body**. Outcomes **Done** and **Failed**. Needs **Email**; see [Email](../settings/email.md). |
| **SMS Task** | Its card on the canvas reads **Send SMS**. Sends a text message. Needs **SMS**; see [SMS](../settings/sms.md). |

## Meta

| Activity | What it does |
| --- | --- |
| **Meta Conversions API Event Task** | Sends a conversion event (such as a purchase or a lead) to Meta from the server. Needs **Meta Pixel**; see [Analytics and social](../settings/analytics-and-social.md). |

## Notifications

| Activity | What it does |
| --- | --- |
| **Notify Specific Users Task** | Sends a notification to the users named in **User names**. |
| **Notify Content's Owner Task** | Sends a notification to the owner of the content item the workflow is about. |

Both are described in [Notifications](notifications.md#send-notifications-from-a-workflow).

## Primitives

| Activity | What it does |
| --- | --- |
| **Started By Workflow Event** | Starts this workflow when another one runs it with **Execute Workflow Task**. |
| **Execute Workflow Task** | Runs a workflow marked **Usable as an activity**, passes it inputs and gets its outputs. |
| **Set Variable Task** | Stores a value in a **Variable**. |
| **Set Property Task** | Stores a value in a property of the workflow. |
| **Set Output Task** | Stores a value in the workflow's output. |
| **Correlate Task** | Ties the instance to a value, such as a content item id, so events about that item resume it. |
| **Log Task** | Writes a line (**Text**) at a **Log Level** in the site's log. |

## Session

| Activity | What it does |
| --- | --- |
| **Commit Transaction Task** | Saves the changes made so far to the database at that point, instead of at the end of the request. Needs **Session Workflows Activities**. |

## Social

| Activity | What it does |
| --- | --- |
| **Update X (Twitter) Status Task** | Posts on X. Needs **X (Twitter) Integration**. |

## Tenant

| Activity | What it does |
| --- | --- |
| **Create Tenant Task**, **Setup Tenant Task**, **Enable Tenant Task**, **Disable Tenant Task** | Create, set up, enable or disable a tenant (another site on the same installation). Needs **Tenants**; see [Tenants](../administration/tenants.md). |

## UI

| Activity | What it does |
| --- | --- |
| **Notify Task** | Shows a message bar (the kind that confirms a save) to the person whose action ran the workflow, with a **Notification Type** and a **Message**. It doesn't send anything; for that, see [Notifications](notifications.md). |

## User

| Activity | What it does |
| --- | --- |
| **User Created Event**, **User Updated Event**, **User Deleted Event**, **User Enabled Event**, **User Disabled Event**, **User Confirmed Event**, **User Loggedin Event**, **User Loggedout Event** | Start when a user account is created, updated, deleted, enabled, disabled, confirms their email, signs in or signs out. |
| **Register User Task** | Creates a user account from form fields. Also needs **Email**. |
| **Validate User Task** | Checks that the current visitor is signed in and has the given roles. |
| **Assign User Role Task** | Adds a role to a user. |
| **Unassign User Role Task** | Removes roles from a user. |
| **Get Users by Role Task** | Gets the users who have the given roles. |

## Validation

| Activity | What it does |
| --- | --- |
| **Validate Antiforgery Token Task**, **Bind Model State Task**, **Validate Form Field Task**, **Add Model Validation Error Task**, **Validate Form Task** | Check what a form sent and show errors on it. See [Forms](forms.md#handle-what-visitors-send). |
| **Validate ReCaptcha Task** | Checks the captcha of a form. Needs **ReCaptcha**. |
