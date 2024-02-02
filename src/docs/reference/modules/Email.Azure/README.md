# Azure Email (`OrchardCore.Email.Azure`)

This module provides the infrastructure necessary to send emails using [Azure Communication Services Email](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview).

## Azure Email Settings

Enabling the `OrchardCore.Email.Azure` module will allow the user to set the following settings:

| Setting | Description |
| --- | --- |
| `ConnectionString` | The ACS connection string that will be used to deliver the email.
| `DefaultSender` | The email of the sender. This will overrides the `DefaultSender` setting in [`OrchardCore.Email`](../Email/README.md). |

## Azure Email Settings Configuration

The `OrchardCore.Email.Azure` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureAzureEmailSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_Email_Azure": {
      "DefaultSender": "",
      "ConnectionString": "",
    }
```

!!! note
    Configuring `DefaultSender` will override the email settings.

For more information please refer to [Configuration](../../core/Configuration/README.md).

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/1PYGKkhJBEA" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
