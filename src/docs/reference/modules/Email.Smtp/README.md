# Email (`OrchardCore.Email.Smtp`)

This module provides an Email provider for sending emails through the Simple Mail Transfer Protocol (SMTP).

# Configuring SMTP Provider

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

## Email Settings Configuration

The `OrchardCore.Email` module in Orchard Core CMS offers users the capability to configure email settings through various sources. By default, users can configure these settings using the admin area. However, the module also allows users to override these default settings by specifying configuration values in alternative sources, such as app settings or environment variables. In this configuration hierarchy, values provided in app settings or environment variables take precedence over those configured in the admin area. This design provides users with flexibility in managing email configurations based on their preferences and specific deployment environments.

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

For more information about configurations, please refer to [Configuration](../../core/Configuration/README.md).

## Credits

### MailKit

<https://github.com/jstedfast/MailKit>

Copyright 2013-2019 Xamarin Inc
Licensed under the MIT License
