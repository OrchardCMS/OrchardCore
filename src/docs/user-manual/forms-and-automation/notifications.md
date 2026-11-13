---
title: Notifications
description: Read the messages the site sends you in the notification center, choose whether they also reach you by email or SMS, and send notifications to people from a workflow.
technical_manual:
  - reference/modules/Notifications/README.md
  - reference/modules/Workflows/README.md
---

# Notifications

Notifications are messages the site sends to people who have an account, such as *A new article is waiting for your review* or *Your post was published*. Each notification lands in the person's **notification center**, behind the bell in the admin's top bar, and can also be sent to them by email or by SMS. Notifications are sent by workflows (or by other features of the site); you choose what triggers them and what they say.

| | |
| --- | --- |
| **Menu** | The bell in the top bar, then **Notification Center** |
| **Permission** | *Manage notifications*: given by default to every signed-in user (Administrator, Editor, Moderator, Author, Contributor and Authenticated roles) |
| **Feature** | **Notifications** (`OrchardCore.Notifications`), **Email Notifications** (`OrchardCore.Notifications.Email`), **SMS Notifications** (`OrchardCore.Notifications.Sms`) |

!!! tip "Don't see the bell?"
    It appears only when the **Notifications** feature is on and your role has *Manage notifications*. See [Roles and permissions](../getting-started/roles-and-permissions.md).

!!! note "Not the same as the messages after you save"
    The green or red bar that says *Your content has been saved* is a confirmation of what you just did. It isn't stored and doesn't appear in the notification center.

## Check your notifications

The bell in the top bar is solid with a red badge counting your unread notifications, or outlined when you have none. Hover it to read *You have 3 unread notifications*.

1. Click the bell.
2. The list shows your latest unread notifications (up to 10), each with its subject, summary and how long ago it arrived. With none, it says *You have no unread notifications.*
3. Click a notification's subject to open the page it points to, if it has one.

A notification is marked as read as soon as you point at it or click it in the list, and the badge goes down. To see all your notifications, read and unread, click **Notification Center** at the bottom of the list.

## Use the notification center

The notification center lists only your own notifications: nobody else, not even an administrator, sees them here.

| Element | What it does |
| --- | --- |
| **Search** | Finds notifications by their text. |
| **Show** filter | **All**, **Read** or **Unread**. |
| **Sort** | **Recently created** or **Previously created**. |
| **Type** filter | Shown only when the site has notification types. Lists one kind of notification, such as approvals. |
| **Mark All as Read** | Marks every notification as read, after you confirm with **Ok**. |
| **Select All** and the checkboxes | Select notifications for the **Actions** menu: **Mark as read**, **Mark as unread** or **Remove**. |

Unread notifications show in bold, with a **Mark as read** button. Each row's **Actions** menu has:

- **View**: opens the notification on its own page, with its full text. From there, **Open** goes to the page it points to (when it has one), **Mark as unread** puts it back in the unread list, and **Delete** removes it.
- **Mark as unread** (for a read notification), confirmed with **Yes**.
- **Delete**, confirmed with **Yes**. A deleted notification can't be brought back.

If a notification is about something you can no longer open (your role changed, for example), it shows *You no longer have access to this notification.*

## Choose how you receive notifications

Every notification goes to your notification center. With **Email Notifications** or **SMS Notifications** on, it can also be sent to you by email or by text message.

1. Open your profile from the user menu at the top right (or, as an administrator, edit the user in **Access Control > Users**).
2. Find **Notification Methods**. It lists the methods the site offers: **Email Notifications** and **SMS Notifications**.
3. Check the methods you want, and uncheck the ones you don't.
4. When there are several, drag them to put the most important one on top.
5. Click **Save**.

| Field | What it does |
| --- | --- |
| **Notification Methods** | Each checked method sends you a copy of every notification. All methods are checked until you uncheck one. Email uses the email address of your account, SMS its **Phone Number** (with the country code); if it's empty, nothing is sent that way. |
| **Notification Types** | Shown only when the site has notification types (added by other modules). A table with one row per type and one column per method: uncheck a box to stop receiving that type that way. A method unchecked above stays off for every type. |

The notification center itself can't be turned off.

!!! note
    **Notification Methods** doesn't appear when neither **Email Notifications** nor **SMS Notifications** is on. Emails and text messages are sent only when the site's email or SMS provider is set up; see [Email](../settings/email.md) and [SMS](../settings/sms.md).

## Turn on email and SMS notifications

1. Make sure email (or SMS) sending works: set up a provider in **Settings > Communication > Email** (or **SMS**) and try **Tools > Testing > Email Test** (or **SMS Test**). See [Email](../settings/email.md) and [SMS](../settings/sms.md).
2. Open **Tools > Features** and enable **Email Notifications** or **SMS Notifications** (category **Notifications**). See [Features](../getting-started/features.md).

From then on, each notification is also sent by the methods each person keeps checked. The email uses the notification's **Subject**, and its HTML body when **Use HTML Body when possible** is checked (otherwise the text body). The text message is the notification's text body.

## Send notifications from a workflow

The **Notifications** category of the workflow designer has two tasks, listed in [Workflow activities](workflow-activities.md#notifications). See [Workflows](workflows.md) to build a workflow.

| Task | Who receives the notification |
| --- | --- |
| **Notify Specific Users Task** | The users you type in **User names** (required), separated by commas. Liquid is allowed, for example to use a value from a form. |
| **Notify Content's Owner Task** | The owner of the content item the workflow is about, for workflows started by a content event such as **Content Published Event**. Needs **Users** and **Contents**. |

Both have the same message fields:

| Field | What it does |
| --- | --- |
| **Subject** | Required. The title of the notification, shown in the bell list and as the email subject. Liquid is allowed, such as `New message from {{ Request.Form.Name }}`. |
| **Summary** | A short text under the subject. Liquid and HTML are allowed. |
| **URL** | Optional. The page the notification opens, such as `~/blog` for a page of this site (start with `~/`) or a full web address. |
| **Type** | Optional. The kind of notification, used by the **Type** filter of the notification center and by people's preferences, such as `Approval`. 64 characters at most. |
| **Text Body** | The plain-text message, used for SMS and for email without HTML. Liquid is allowed. |
| **HTML Body** | The formatted message for email. Liquid is allowed. |
| **Use HTML Body when possible** | Sends the HTML body by email when there is one. |

The task follows **Done** when it sent the notifications, **Failed** when it couldn't, and **Failed: no user found** when nobody matched (an unknown user name, or a content item without an enabled owner).

### Example: tell the author their article was published

1. Create a workflow that starts with **Content Published Event**, and pick **Article** in its **Content Types**.
2. Add a **Notify Content's Owner Task** after it.
3. Set **Subject** to `Your article "{{ Workflow.Input.ContentItem | display_text }}" is live`.
4. Click **Apply**, then **Publish** the workflow.

When someone publishes an article, its owner finds the notification behind their bell, and by email if they kept **Email Notifications** checked.
