---
title: Shared links
description: See the download links data pipelines emailed to users, who received them and how often they were used, and revoke or delete them.
technical_manual:
  - reference/modules/DataPipelines/README.md
---

# Shared links

The **Share a download link** step of a [data pipeline](data-pipeline-steps.md#destinations) doesn't attach the file to an email: it keeps the file on the site and emails each recipient a link to it. **Tools > Shared Links** lists these links, so you can see who received what and stop a link before it expires.

| | |
| --- | --- |
| **Menu** | **Tools > Shared Links** |
| **Permission** | *Manage shared links* (Administrator by default) |
| **Feature** | **Data Pipelines Core** (`OrchardCore.DataPipelines.Core`), turned on with **Data Pipelines** |

## Find a shared link

1. Open **Tools > Shared Links**.
2. Type part of the file's name, or of the pipeline that shared it, in **Search by name or source**, and press Enter.

Each row shows the name of the shared file and these badges (hover a badge to see its name):

| Badge | What it shows |
| --- | --- |
| Status | **Active**, **Expired** (its days ran out), **Used up** (it reached its maximum downloads) or **Revoked**. Hover **Active** to see the date and time the link works until (UTC). |
| **Kind** | What kind of resource the link opens, such as a pipeline file. |
| **Recipients** | The users the link was sent to. |
| **Shared by** | What shared it, such as the pipeline. |
| **Shared** | How long ago it was shared. |
| **Uses** | How many times it was opened, as *2 of 5 uses* when it has a maximum. |

## Stop a link from working

1. In the row of an active link, open the **Actions** menu and click **Revoke**. Links that already stopped working don't have this action.
2. Confirm with **Yes**.

The message *The link to '...' no longer works.* appears, and the row shows **Revoked**. Anyone who opens the link from their email gets an error page instead of the file.

## Delete a link

1. In the link's row, open the **Actions** menu and click **Delete**.
2. Confirm with **Ok**.

The message *The link to '...' was deleted.* appears, and the link and its file are deleted. You don't need to clean up by hand: a background task deletes every link, and its file, a day after the link stops working (expired, used up or revoked).

**View the source**, in the same menu, opens what shared the link, such as the pipeline.

## What recipients see

Each recipient gets an email, *'file name' was shared with you*, with a link to open it. The link works only for its recipients: they must sign in to the site first, and someone else who signs in and opens it is refused. The email says until when the link works. The link itself appears only in that email; the Shared Links screen never shows it, so it can't be copied from there. If a recipient has no email address, the run logs a warning naming them.
