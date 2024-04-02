# Azure Email (`OrchardCore.Email.Azure`)

This module provides an Email provider for sending emails through [Azure Communication Services Email](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview).

## Azure Communication Service Settings

Enabling this module will introduce a new tab labeled 'Azure' within the email settings, allowing you to configure the service. To access these settings, navigate to `Configuration` → `Settings` → `Email` and click on the 'Azure' tab. The following are the available settings

| Setting | Description |
| --- | --- |
| `ConnectionString` | The ACS connection string that will be used to deliver the email.
| `DefaultSender` | The email of the sender. This will override the `DefaultSender` setting in [`OrchardCore.Email`](../Email/README.md). |

## Default Azure Communication Service Configuration

You may configure the Default Azure Email Service provider by the configuration provider using the following settings:

```json
"OrchardCore_Email_Azure": {
    "DefaultSender": "",
    "ConnectionString": ""
}

For more information about configurations, please refer to [Configuration](../../core/Configuration/README.md).

!!! note
    Configuration of the Default Azure Email provider is not possible through Admin Settings. Utilize the configuration provider for the necessary setup. The provider will appear only if the configuration exists.
