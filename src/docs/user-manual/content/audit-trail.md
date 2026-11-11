---
title: Audit trail
description: See who changed what and when, compare two versions of a content item, and restore an earlier version or a deleted item.
technical_manual:
  - reference/modules/AuditTrail/README.md
---

# Audit trail

The **audit trail** is the history of what happens in the admin: who created, saved, published, unpublished, cloned, deleted or restored a content item, and when. For content items it keeps a copy of each version, so you can see what a page looked like, compare two versions line by line, and bring back an earlier version or a deleted item. User events, such as sign-ins and password resets, are recorded too; see [Users](../users/users.md).

| | |
| --- | --- |
| **Menu** | **Tools > Audit Trail**, **Settings > Audit Trail** |
| **Permission** | *View audit trail*, *Manage audit trail settings* |
| **Feature** | **Audit Trail** (`OrchardCore.AuditTrail`) |


## Choose what is recorded

!!! warning "Content events are recorded only for the types you pick"
    Out of the box, no content type is selected, so no content event is recorded. Pick the content types on the **Content** tab of the settings before you rely on the audit trail.

1. Open **Settings > Audit Trail**.
2. On the **Events** tab, under **Events to record**, tick the events to keep. Each category (**Content**, **User**) has a **Select all** box. All content events are ticked by default. An event that must always be recorded has a grayed-out box.
3. Tick **Allow client IP address** to record the IP address of the person who triggered each event.
4. On the **Content** tab, under **Allowed content types**, tick the types whose events are recorded, or **Select all**.
5. On the **Trimming** tab, set how long events are kept (see below).
6. Click **Save**.

The **Content** events are **Created**, **Saved**, **Published**, **Unpublished**, **Removed** (deleted), **Cloned** and **Restored**.

### Keep the audit trail small

Old events are deleted once a day by a background task.

| Setting | What it does |
| --- | --- |
| **Disable trimming task** | Keeps every event forever. The audit trail then grows without limit. |
| **Retention period** | The number of days an event is kept. 10 by default. |
| **Last run** | When old events were last deleted, or **Never**. |

Deleted events, and the versions saved with them, can't be recovered.

## Browse the audit trail

Open **Tools > Audit Trail**. Each row shows the event (for example **Published**), its category, the details, who triggered it, when, and the IP address if it is recorded.

For a content event, the details read like *Version 3 of the Article My first post was published*. Click **Version 3** to see that version (read only), or the item's title to open its current editor.

Narrow the list with:

- **Search**: type a user name to see that person's events.
- The filter button: quick filters for each category (**Content**, **User**), and **Filter syntax**, which lists every search term you can type.
- The drop-downs: a date range (**Any date**, **Last hour**, **Last 24 hours**, **This week**, **Last month**, a custom range, ...), a category (**All categories**, **Content**, **User**), and the order (**Newest**, **Oldest**, **Category**, **Event**, **User**).

The search box understands terms such as `username:alice`, `category:Content`, `date:>@now-1` (the last day) or a date range, and `id:...` for one content item. Click the filter button, then **Filter syntax**, for examples and for the filter you are currently using.

## See the history of one content item

1. Open **Content > Content Items**.
2. On the item's row, open **Actions** and click **Audit Trail**.

The audit trail opens with only that item's events, and an extra drop-down to filter by event (for example only **Published**). The **Audit Trail** action is shown only to people with *View audit trail*.

## View an event

Click **Details** on the event's row. The page shows the **Event Info** (category, event name, the user name and user id of who triggered it, the time, and the client IP address), and for a content event, the **Content Item Info** (its id, type, display text, version, and the **Comment** typed by the editor, if any).

### View a past version

On a content event's row, click **View**. The item's editor opens, read only, with the content as it was in that version. You need the permission to edit the item.

### Compare two versions

1. Click **Details** on a content event.
2. Open the **Diff** tab.

The tab compares this version with the previous one, showing lines removed and added. Switch between **Split** (side by side) and **Unified** (one column). When there is no earlier version, it says *No previous content item available to diff against*.

## Restore an earlier version

1. In the audit trail, find a **Published** event of the version you want back.
2. Click **Restore** on its row and confirm with **Yes**.

**Restore** is shown only on **Published** events of versions that are not the current one. The old version comes back as a new **draft**; the version currently live on the site stays published until you review the draft and click **Publish**. If the old version no longer passes the content type's rules (for example a field that has become required), the site doesn't restore it and says why.

## Restore a deleted content item

1. In the audit trail, find the **Removed** event of the item.
2. Click **Restore** on its row and confirm with **Yes**.

The item comes back as a draft. Open it from **Content > Content Items** and publish it.

Restoring needs the permission to publish the item: *Publish or unpublish content for others*, *Publish or unpublish own content* for your own items, or the same permissions for that content type. Each restore is recorded as a **Restored** event.

## Ask editors for a comment

Add the **AuditTrail** part to a content type (**Design > Content Definition > Content Types**, **Edit**, **Add Parts**) to give its editor a **Comment** box. What the editor types there is saved with the event and shown in its details. The part's setting **Show comment** (ticked by default) shows or hides the box.
