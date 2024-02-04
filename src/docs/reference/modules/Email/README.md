# Email (`OrchardCore.Email`)

This module provides the infrastructure necessary to send emails using multiple providers.



## SMTP Settings

Enabling the `Email` feature will add a new settings page under `Configurations` >> `Settings` >> `Email`. You can utilize these settings to set up the default Email provider configuration. The following are the providers that are readily accessible.


| Provider | Description |
| --- | --- |
| `SMTP` | Opting for this provider enables the utilization of SMTP for sending email messages. Edit the `SMTP` tab in the email settings to enable this provider. |
| `Azure` | Opting for this provider enables the utilization of [Azure Communication Services Email](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview) for sending email messages. Edit the `Azure` tab in the email settings to enable this provider. This option is only available after enabling `OrchardCore.Email.Azure` feature. |

# Configuring SMTP Provider

To enable the `SMTP` provider, navigate to `Configurations` >> `Settings` >> `EMAIL`. Click on the `SMTP` tab, click the Enable checkbox and provider your SMTP info. Then in the `Providers` tab, select SMTP as your default provider.

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
