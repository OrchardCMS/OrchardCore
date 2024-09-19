# Azure Communication Services Email (`OrchardCore.Email.Azure`)

This module adds Email providers for sending emails through [Azure Communication Services](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview).

## **Azure Communication Services** Provider Configuration

Enabling this module will introduce a new tab labeled **Azure Communication Services** within the email settings, allowing you to configure the service. To access these settings, navigate to `Configuration` → `Settings` → `Email` and click on the **Azure Communication Services** tab. The following are the available settings

| Setting | Description |
| --- | --- |
| `DefaultSender` | The email of the sender. This will override the `DefaultSender` setting in [`OrchardCore.Email`](../Email/README.md). |
| `ConnectionString` | The ACS connection string that will be used to deliver the email.

## **Default Azure Communication Services** Provider Configuration

You may configure the Default Azure Email Service provider by the configuration provider using the following settings:

```json
"OrchardCore_Email_AzureCommunicationServices": {
    "DefaultSender": "",
    "ConnectionString": ""
}
```

For more information about configurations, please refer to [Configuration](../../core/Configuration/README.md).

!!! note
    Configuration of the **Default Azure Communication Services** provider is not possible through Admin Settings. Utilize the configuration provider for the necessary setup. The provider will appear only if the configuration exists.
