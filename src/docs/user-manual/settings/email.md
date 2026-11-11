---
title: Email
description: Connect the site to an email service (SMTP or Azure Communication Services), choose the default provider, and send a test email.
technical_manual:
  - reference/modules/Email/README.md
  - reference/modules/Email.Smtp/README.md
  - reference/modules/Email.Azure/README.md
---

# Email

The site sends email for many tasks: password reset links, registration confirmations, two-factor codes, notifications, and the **Send Email** task of [workflows](../forms-and-automation/workflows.md). Before any of this works, you connect the site to an email service, called a **provider**. Orchard Core can send through any mail server that speaks SMTP (such as your company mail server, SendGrid, Mailgun or Microsoft 365), or through Azure Communication Services.

| | |
| --- | --- |
| **Menu** | **Settings > Communication > Email** (providers), **Tools > Testing > Email Test** |
| **Permission** | *Manage Email Settings* (the Administrator role has it by default) |
| **Feature** | **Email** (`OrchardCore.Email`), with at least one provider: **SMTP Email Provider** (`OrchardCore.Email.Smtp`) or **Azure Communication Services Email** (`OrchardCore.Email.Azure`) |


## Before you start

Get the connection details from whoever runs your email service:

- For SMTP: the server name, the port, the kind of encryption, and a user name and password if the server needs them.
- For Azure Communication Services: the connection string of the Azure resource, and a sender address the resource is allowed to use.
- In both cases: the address the site sends from, such as `no-reply@example.com`.

## Set up an SMTP provider

1. Open **Settings > Communication > Email** and click the **SMTP** tab.
2. Check **Enable SMTP Provider**. The rest of the form appears.
3. Type the **Sender email address**.
4. Leave **Delivery method** on **Network**, then fill in the **Network delivery options** (see the table below).
5. If the server needs a sign-in, check **Require credentials**, then type the **User name** and **Password**.
6. Click **Save**.

If no default provider was chosen yet, SMTP becomes the default provider. Send a test email to check the settings (see [Send a test email](#send-a-test-email)).

| Field | What it does |
| --- | --- |
| **Enable SMTP Provider** | Turns this provider on. Clear it and click **Save** to stop using SMTP. If SMTP was the default provider, no default is left. |
| **Sender email address** | Required. The address emails come from, unless the email itself names another sender. It must be a valid email address. |
| **Delivery method** | **Network** sends the mail to a mail server; use it for a real site. **Specified pickup directory** writes each email as a file in a folder on the server instead of sending it, which helps when testing. |
| **Host name** | Required for **Network**. The mail server's name, for example `smtp.mailprovider.com`. |
| **Port number** | The mail server's port. The default is 25. Your provider tells you which port to use (often 587 or 465). |
| **Proxy host name** | Optional. A proxy server to connect through. |
| **Proxy port number** | Optional. The port of that proxy server. |
| **Auto Select Encryption method** | Lets the site pick the encryption from the port number. While it is checked, **Encryption method to use** can't be changed. |
| **Encryption method to use** | **None** (an insecure connection), **SSL/TLS** (a secure connection from the start), or **STARTTLS** (an insecure connection that is then upgraded to a secure one). |
| **Require credentials** | Check it when the mail server needs a sign-in. |
| **Use Windows authentication** | Shown under **Require credentials**. Signs in to the mail server with the identity the site runs under, instead of a user name and password. Your IT team tells you when to use it. |
| **User name** | The user name for the mail server. |
| **Password** | The password for the mail server. It is stored encrypted, and it is never shown again: leave it blank to keep the current one. You can also keep it in a [secret](secrets.md) (see below). |
| **Pickup directory location** | For **Specified pickup directory** only. The folder for the email files: `/` for the site's default folder, or a subfolder such as `/Outbound`. Your IT team sets the base folder; emails always stay inside it. |
| **Ignore Invalid SSL Certificate** | Sends email even when the mail server's security certificate isn't valid. Use it only for testing, never on a live site. |

## Set up Azure Communication Services

1. Open **Settings > Communication > Email** and click the **Azure Communication Services** tab.
2. Check **Enable Azure Provider**.
3. Type the **Sender email address**.
4. Paste the **Connection String** of your Azure resource, or pick a [secret](secrets.md) that holds it.
5. Click **Save**.

| Field | What it does |
| --- | --- |
| **Enable Azure Provider** | Turns this provider on. Clear it and click **Save** to stop using it. |
| **Sender email address** | Required. The address emails come from. It must be a valid email address that your Azure resource is allowed to send from. |
| **Connection String** | Required the first time. The connection string from the Azure portal. It is stored encrypted and never shown again: leave it blank to keep the current one. |

If no default provider was chosen yet, Azure Communication Services becomes the default provider.

## Keep a password or connection string in a secret

When the **Secrets** feature is on, the **Password** of SMTP and the **Connection String** of Azure have a list above the box that says where the value is kept:

- **User-specified value**: you type the value, and it is stored, encrypted, with these settings.
- **Use a secret**: you pick a secret from the list. Click the reload button next to the list after you create one, or click **New secret** to create it in a new browser tab. The value stored with the settings is removed once you save with a secret selected.

Keeping credentials in secrets lets you change a password in one place, and lets your IT team keep them in a vault. See [Secrets](secrets.md).

## Choose the default provider

When more than one provider is on, the site sends with the default one.

1. Open **Settings > Communication > Email**. The **Providers** tab opens first.
2. Pick the **Default provider**.
3. Click **Save**.

The list shows only the providers that are turned on:

| Provider in the list | Where it comes from |
| --- | --- |
| **Simple Mail Transfer Protocol (SMTP)** | The **SMTP** tab of this screen. |
| **Azure Communication Services** | The **Azure Communication Services** tab of this screen. |
| **Simple Mail Transfer Protocol (Default SMTP)**, **Default Azure Communication Services** | Set up by your IT team in the site configuration, if they did so. You can't edit them here. |

If no provider is on, the tab says *No email providers are currently enabled.*

## Send a test email

1. Open **Tools > Testing > Email Test**.
2. Pick the **Provider** to test. The default provider is selected for you.
3. Fill in **To**, **Subject** and **Body**. The other fields are optional.
4. Click **Send**.

| Field | What it does |
| --- | --- |
| **Provider** | Required. The provider to send with. |
| **To** | Required. The address that receives the test. |
| **Sender** | Optional. Useful when the author of the email is different from the address that sends it. |
| **Cc**, **Bcc** | Optional. Copy and blind copy addresses. |
| **Reply To** | Optional. The address replies go to. |
| **Subject** | Required. The subject line. |
| **Body** | Required. The text of the email, sent as plain text. |

When it works, you see *Message sent successfully.* Otherwise the screen shows the provider's error, or *Unable to send the message using the selected provider.* In that case, check the settings with whoever runs your email service. If no provider is turned on, the screen says *No email providers are currently enabled. Please enable at least one email provider feature.*

## Where else email is used

- [Login and registration](../users/login-and-registration.md): password reset, email confirmation and changing an email address.
- The **Two-Factor Email Method** feature sends sign-in codes by email.
- [Notifications](../forms-and-automation/notifications.md): the **Email Notifications** feature sends notifications to users by email.
- [Workflow activities](../forms-and-automation/workflow-activities.md#messaging): the **Send Email** task.

!!! example "See it in practice"
    [Add a contact form](../use-cases/contact-form.md) walks through this step by step.
