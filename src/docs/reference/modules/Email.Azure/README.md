# Azure Email (`OrchardCore.Email.Azure`)

This module provides the infrastructure necessary to send emails using `Azure Communication Services`.

## Azure Email Settings

Enabling the `OrchardCore.Email.Azure` module will allow the user to set the following settings:

| Setting | Description |
| --- | --- |
| `DefaultSender` | The email of the sender. |
| `ConnectionString` | The ACS connection string that will be used to deliver the email.

## Azure Email Settings Configuration

The `OrchardCore.Email.Azure` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureEmailSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

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
