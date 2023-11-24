# Email (`OrchardCore.Email`)

This module provides email settings configuration.

## Email Settings

Enabling the `OrchardCore.Email` module will allow the user to set the following settings:

| Setting | Description |
| --- | --- |
| `DefaultSender` | The email of the sender. |

## Email Settings Configuration

The `OrchardCore.Email` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureEmailSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_Email": {
      "DefaultSender": "",
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).
