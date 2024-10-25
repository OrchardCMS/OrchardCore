# Email (`OrchardCore.Email.Smtp`)

This module provides an Email provider for sending emails through the Simple Mail Transfer Protocol (SMTP).

## Simple Mail Transfer Protocol (SMTP) Settings

To enable the `SMTP` provider, navigate to `Configurations` → `Settings` → `Email`. Click on the `SMTP` tab, click the Enable checkbox, and provide your SMTP configuration. Then in the `Providers` tab, select SMTP as your default provider.

Here are the available SMTP settings

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

## Default Simple Mail Transfer Protocol (SMTP) Configuration

You may configure the Default SMTP provider by the configuration provider using the following settings:

```json
"OrchardCore_Email_Smtp": {
    "DefaultSender": "",
    "DeliveryMethod": "Network",
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

For more information about configurations, please refer to [Configuration](../../core/Configuration/README.md).

!!! note
    Configuration of the Default SMTP provider is not possible through Admin Settings. Utilize the configuration provider for the necessary setup. The provider will appear only if the configuration exists.


## Credits

### MailKit

<https://github.com/jstedfast/MailKit>

Copyright 2013-2019 Xamarin Inc
Licensed under the MIT License
