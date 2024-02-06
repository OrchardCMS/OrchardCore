# Azure Email (`OrchardCore.Email.Azure`)

This module provides an Email provider for sending emails through the [Azure Communication Services Email](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview).

## Azure Email Settings

Enabling this module will introduce a new tab labeled 'Azure' within the email settings, allowing you to configure the service. To access these settings, navigate to `Configuration` >> `Settings` >> `Email` and click on the 'Azure' tab. The following are the available settings

| Setting | Description |
| --- | --- |
| `ConnectionString` | The ACS connection string that will be used to deliver the email.
| `DefaultSender` | The email of the sender. This will overrides the `DefaultSender` setting in [`OrchardCore.Email`](../Email/README.md). |

## Azure Email Settings Configuration

You may configure the Azure Email Service using `appsettings.json` file by adding the following settings

```json
"OrchardCore_Email_Azure": {
    "DefaultSender": "",
    "ConnectionString": "",
    "PreventUIConnectionChange": false // This option restricts users from inputting a custom connection string through site settings or the UI.
}
```

For more information about configurations, please refer to [Configuration](../../core/Configuration/README.md).
