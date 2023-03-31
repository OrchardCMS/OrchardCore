# Email (`OrchardCore.Email`)

This module provides the infrastructure necessary to send emails using `SMTP`.

## SMTP Settings

Enabling the `OrchardCore.Email` module will allow the user to set the following settings:

| Setting | Description |
| --- | --- |
| `DefaultSender` | The email of the sender. |
| `DeliveryMethod` | The method for sending the email, `SmtpDeliveryMethod.Network` (online) or `SmtpDeliveryMethod.SpecifiedPickupDirectory` (offline). |
| `PickupDirectoryLocation` | The directory location for the mailbox (`SmtpDeliveryMethod.SpecifiedPickupDirectory`). |
| `Host` | The SMTP server. |
| `Port` | The SMTP port number. |
| `AutoSelectEncryption` | Whether the SMTP select the encryption automatically. |
| `RequireCredentials` | Whether the SMTP requires the user credentials. |
| `UseDefaultCredentials` | Whether the SMTP will use the default credentials. |
| `EncryptionMethod` | The SMTP encryption method `SmtpEncryptionMethod.None`, `SmtpEncryptionMethod.SSLTLS` or `SmtpEncryptionMethodSTARTTLS`. |
| `UserName` | The username for the sender. |
| `Password` | The password for the sender. |
| `ProxyHost` | The proxy server. |
| `ProxyPort` | The proxy port number. |

!!! note
    You must configure `ProxyHost` and `ProxyPort` if the SMTP server runs through a proxy server.

## Email Settings Configuration

The `OrchardCore.Email` module allows the user to use configuration values to override the settings configured from the admin area by calling the `ConfigureEmailSettings()` extension method on `OrchardCoreBuilder` when initializing the app.

The following configuration values can be customized:

```json
    "OrchardCore_Email": {
      "DefaultSender": "",
      "DefaultSender": "Network",
      "PickupDirectoryLocation": "",
      "Host": "localhost",
      "Port": 25,
      // Uncomment if SMTP server runs through a proxy server
      //"ProxyHost": "proxy.domain.com",
      //"ProxyPort": 5050,
      "EncryptionMethod": "SSLTLS",
      "AutoSelectEncryption": false,
      "UseDefaultCredentials": false,
      "RequireCredentials": true,
      "Username": "",
      "Password": ""
    }
```

For more information please refer to [Configuration](../../core/Configuration/README.md).

## Credits

### MailKit

<https://github.com/jstedfast/MailKit>

Copyright 2013-2019 Xamarin Inc
Licensed under the MIT License