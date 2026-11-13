---
title: Set up content approval
description: Let contributors write drafts that editors review and publish, find drafts waiting for review, and notify reviewers with a workflow when a draft is saved.
technical_manual:
  - reference/modules/Roles/README.md
  - reference/modules/Contents/README.md
  - reference/modules/Workflows/README.md
  - reference/modules/Notifications/README.md
---

# Set up content approval

Many teams want a second pair of eyes before anything goes live: writers write, an editor checks and publishes. Orchard Core supports this with **drafts** and **permissions**: people who may edit but not publish can only save drafts, and people who may publish decide when a draft goes live. A **workflow** can tell the reviewers when there's something new.

!!! note "What this is, and what it isn't"
    Orchard Core has no built-in approval screen, approval states (such as *In review* or *Rejected*) or approval buttons. The process on this page is built from what exists: drafts, the **Publish** permission, the content list's filters, and an optional notification. Rejecting a draft means telling the writer, for example by email, and leaving it unpublished.

## Who it's for and what you get

For **administrators** who set it up, and the **contributors** and **editors** who use it.

When it's done:

- contributors create and edit their own content, but can only **Save Draft**;
- editors see drafts waiting for review, check them with **Preview**, and **Publish** them;
- reviewers get a notification each time a contributor saves a draft (optional).

## Before you start

| You need | Where or who |
| --- | --- |
| The features **Contents** and **Roles** (always on with the built-in recipes) | |
| For notifications: **Workflows** (`OrchardCore.Workflows`) and **Notifications** (`OrchardCore.Notifications`). **Email Notifications** (`OrchardCore.Notifications.Email`) also sends them by email, once email is set up. | An administrator, in **Tools > Features** |
| The permissions *Manage Roles*, *Manage security settings and all users* and, for the workflow, *Manage workflows* | The **Administrator** role has them |
| Content types that are **Draftable**. **Page**, **Article** and **Blog Post** in the Blog recipe are. | See [Content types](../content/content-types.md) |

!!! warning "Draftable matters"
    The **Save Draft** button exists only on draftable content types. On a type that isn't draftable, someone who can't publish can't save anything at all.

## Set up the roles

Orchard Core comes with roles that fit this process:

| Role | Can do by default | Role in the process |
| --- | --- | --- |
| **Contributor** | *Edit own content*, *Preview own content*, *Clone own content* (and *Localize own content* when **Content Localization** is on) | Writes drafts. Can't publish, and can't edit others' content. |
| **Author** | Edit, preview, clone, delete and **publish** their own content | Publishes their own content without review: don't use it for writers who need review. |
| **Editor** | Edit, preview, clone, delete, list and **publish** everyone's content | Reviews and publishes. |

The **Contributor** role lacks one permission writers need: it can't list content, so the **Content > Content Items** menu is hidden from them. Add it:

1. Open **Access Control > Roles**.
2. Click **Edit** on **Contributor**.
3. Under **Contents**, tick *List content items*.
4. Click **Save**.

To let them see some content types only, tick the per-type permission instead, such as *List Article content items*.

Then give people their roles:

1. Open **Access Control > Users**.
2. Click **Edit** on a writer.
3. Under **Roles**, tick **Contributor**.
4. Clear any role that can publish (**Author**, **Editor**, **Administrator**).
5. Click **Save**.

Give reviewers the **Editor** role the same way. To limit reviewers or writers to some content types, use per-type permissions instead; see [Give an editor limited access](editor-with-limited-access.md). See [Roles](../users/roles.md) and [Users](../users/users.md).

## How a contributor submits content

1. Open **Content > Content Items**.
2. Click **New**.
3. Choose a content type, such as **Article**.
4. Write the content.
5. Click **Preview** to check it.
6. Click **Save Draft**.

There's no **Publish** button for them. To change the item later:

1. In **Content > Content Items**, choose **Owned by me** in the filter to find their own items.
2. Open the item from the list.
3. Edit it.
4. Click **Save Draft** again.

A contributor can also edit their own published items, but their changes stay in a draft until an editor publishes it; visitors keep seeing the published version meanwhile.

## How an editor reviews and publishes

1. Open **Content > Content Items**.
2. In the search box's drop-down, choose **Only draft items**.
3. Click **Edit** on an item.
4. Click **Preview** to see it as visitors will.
5. To approve it, click **Publish**.
6. To send it back instead, leave it as a draft and tell the writer what to change.

**Only draft items** shows items that have a draft: new items never published, and published items with unpublished changes. To approve several items at once, tick them in the list and choose **Publish Now** in the **Actions** menu above the list. The **Audit Trail** of an item, if that feature is on, shows who changed what; see [Audit trail](../content/audit-trail.md).

See [Content items](../content/content-items.md) for the list's filters and bulk actions.

## Notify reviewers when a draft is saved (optional)

A workflow can send reviewers a notification each time someone saves a draft.

1. Open **Design > Workflows**.
2. Click **Create Workflow**.
3. Type a **Name**, such as *Notify reviewers*.
4. Click **Create and open the designer**.
5. Add a **Content Save Draft Event** from the activities list.
6. In **Content Types**, choose the types to watch, such as **Article** and **Page**.
7. Add a **Notify Specific Users Task**.
8. Connect the event's **Done** outcome to **Notify Specific Users Task**.
9. Fill in the task's fields:
    - **User names**: type the reviewers' user names, separated by commas;
    - **Subject**: type `Draft to review: {{ Workflow.Input.ContentItem | display_text }}`;
    - **URL**: type `~/Admin/Contents/ContentItems/{{ Workflow.Input.ContentItem.ContentItemId }}/Edit`;
    - **Summary**: type a sentence such as *A draft was saved and is waiting for review.*
10. Check the **Issues** tab.
11. Click **Publish**.

**Content Save Draft Event** is the **Start** activity. The **URL** makes the notification open the item's editor. Reviewers see the notification in the bell icon of the admin's top bar and in the **Notification Center**. With **Email Notifications** on and email set up, people who chose email in their **Notification Methods** get it by email too. See [Notifications](../forms-and-automation/notifications.md) and [Workflows](../forms-and-automation/workflows.md).

!!! tip "Every draft, or only new items?"
    **Content Save Draft Event** runs on every **Save Draft**, by anyone, including editors. To be told only about new items, use **Content Created Event** instead. To email a list of addresses rather than notify users, use an **Email Task**.

## Check that it works

1. In a private browser window, sign in as a contributor.
2. Create an article.
3. Check there's no **Publish** button.
4. Click **Save Draft**.
5. Sign in as a reviewer (in another browser, or after signing out). The bell icon shows the notification.
6. Click the notification to open the draft.
7. In **Content > Content Items**, choose **Only draft items**: the article is listed.
8. Click **Publish**.
9. Check the article on the site.

## Tips

- Check every role a writer has: one role that can publish (even through a general permission such as *Publish or unpublish own content*) is enough to show the **Publish** button.
- Scheduling isn't approval: a contributor can't use **Publish Later** either, because it needs publish permission. Editors can schedule an approved draft with it; see [Scheduling](../content/scheduling.md).
- Keep the number of reviewers small, so notifications don't become noise.
