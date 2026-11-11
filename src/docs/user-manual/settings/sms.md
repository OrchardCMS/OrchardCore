---
title: SMS
description: Connect the site to a text message service (Twilio or Azure Communication Services), choose the default provider, and send a test SMS.
technical_manual:
  - reference/modules/Sms/README.md
  - reference/modules/Sms.Azure/README.md
---

# SMS

The site can send text messages (SMS), for example two-factor sign-in codes, notifications, and the **Send SMS** task of [workflows](../forms-and-automation/workflows.md). To send them, you connect the site to a text message service, called a **provider**: Twilio, or Azure Communication Services.

| | |
| --- | --- |
| **Menu** | **Settings > Communication > SMS**, **Tools > Testing > SMS Test** |
| **Permission** | *Manage SMS Settings* (the Administrator role has it by default) |
| **Feature** | **SMS** (`OrchardCore.Sms`), which includes Twilio. For Azure, also **Azure Communication Services SMS** (`OrchardCore.Sms.Azure`). |


## Before you start

Get from your text message service:

- The phone number the site sends from. Phone numbers always include the country code, for example `+1` for the United States.
- For Twilio: the **Account SID** and the **Auth Token** of your Twilio account.
- For Azure Communication Services: the connection string of the Azure resource.

## Set up Twilio

1. Open **Settings > Communication > SMS** and click the **Twilio** tab.
2. Check **Enable Twilio Provider**.
3. Fill in the **Twilio Account Info**: **Phone Number**, **Account SID** and **Auth Token**.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Enable Twilio Provider** | Turns this provider on. Clear it and click **Save** to stop using Twilio. If it was the default provider, no default is left. |
| **Phone Number** | Required. The Twilio number messages come from, with the country code, for example `+15555550100`. The site checks that it is a valid phone number. |
| **Account SID** | Required. The account identifier shown in your Twilio console. |
| **Auth Token** | Required. The secret token shown in your Twilio console. It is stored encrypted and never shown again: leave it blank to keep the current one. You can keep it in a [secret](secrets.md) instead. |

If no default provider was chosen yet, Twilio becomes the default provider.

## Set up Azure Communication Services

This tab appears when the **Azure Communication Services SMS** feature is on.

1. Open **Settings > Communication > SMS** and click the **Azure Communication Services** tab.
2. Check **Enable**.
3. Type the **Phone Number** and paste the **Connection String**, or pick a [secret](secrets.md) that holds it.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Enable** | Turns this provider on. |
| **Phone Number** | Required. The default number messages come from, with the country code. It must be a valid phone number. |
| **Connection String** | Required. The connection string from the Azure portal. It is stored encrypted and never shown again: leave it blank to keep the current one. |

If no default provider was chosen yet, Azure Communication Services becomes the default provider.

## Keep a token or connection string in a secret

When the **Secrets** feature is on, **Auth Token** and **Connection String** have a list that says where the value is kept: **User-specified value** (you type it and it is stored, encrypted, with the settings) or **Use a secret** (you pick a secret from the list; use the reload button or **New secret** to add one). See [Secrets](secrets.md).

## Choose the default provider

1. Open **Settings > Communication > SMS**. The **Providers** tab opens first.
2. Pick the **Default SMS provider**.
3. Click **Save**.

The list shows the providers that are turned on, by their short names:

| Provider in the list | What it is |
| --- | --- |
| **Twilio** | The **Twilio** tab of this screen. |
| **Azure** | The **Azure Communication Services** tab of this screen. |
| **DefaultAzure** | An Azure Communication Services account set up by your IT team in the site configuration, if they did so. |
| **Log** | Appears only on a developer's computer. It doesn't send anything: it writes the messages to the site's log, for testing. |

## Send a test SMS

1. Open **Tools > Testing > SMS Test**.
2. Pick the **Provider**.
3. Type the **Phone Number** that receives the test, with the country code.
4. Click **Send**.

The site sends the message *This is a test SMS message.* to that number. When it works, you see *The test SMS message has been successfully sent.* When it fails, you see *The test SMS message failed to send.*: check the provider's settings and your account with the provider. If the number isn't valid, the screen says *Please provide a valid phone number.* If no provider is on, the screen says *No SMS providers are available. Enable at least one SMS provider feature.*

## Where else SMS is used

- The **Two-Factor SMS Method** feature sends sign-in codes by text message. See [Login and registration](../users/login-and-registration.md).
- [Notifications](../forms-and-automation/notifications.md): the **SMS Notifications** feature sends notifications to users by text message.
- [Workflow activities](../forms-and-automation/workflow-activities.md#messaging): the **Send SMS** task sends a message to a **Phone Number**, with an optional sender number and a **Body**, all with Liquid support.
