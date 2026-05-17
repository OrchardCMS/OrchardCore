# Email (`OrchardCore.Email.Smtp`)

This module provides an Email provider for sending emails through the Simple Mail Transfer Protocol (SMTP).

## Simple Mail Transfer Protocol (SMTP) Settings

To enable the `SMTP` provider, navigate to `Settings` → `Communication` → `Email`. Click on the `SMTP` tab, click the Enable checkbox, and provide your SMTP configuration. Then in the `Providers` tab, select SMTP as your default provider.

Here are the available SMTP settings

| Setting                   | Description                                                                                                                         |
|---------------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `DefaultSender`           | The email of the sender.                                                                                                            |
| `DeliveryMethod`          | The method for sending the email, `SmtpDeliveryMethod.Network` (online) or `SmtpDeliveryMethod.SpecifiedPickupDirectory` (offline). |
| `PickupDirectoryLocation` | The directory location for the mailbox (`SmtpDeliveryMethod.SpecifiedPickupDirectory`).                                             |
| `Host`                    | The SMTP server.                                                                                                                    |
| `Port`                    | The SMTP port number.                                                                                                               |
| `AutoSelectEncryption`    | Whether the SMTP select the encryption automatically.                                                                               |
| `RequireCredentials`      | Whether the SMTP requires the user credentials.                                                                                     |
| `UseDefaultCredentials`   | Whether the SMTP will use the default credentials.                                                                                  |
| `EncryptionMethod`        | The SMTP encryption method `SmtpEncryptionMethod.None`, `SmtpEncryptionMethod.SSLTLS` or `SmtpEncryptionMethodSTARTTLS`.            |
| `UserName`                | The username for the sender.                                                                                                        |
| `Password`                | The password for the sender.                                                                                                        |
| `ProxyHost`               | The proxy server.                                                                                                                   |
| `ProxyPort`               | The proxy port number.                                                                                                              |

!!! note
    You must configure `ProxyHost` and `ProxyPort` if the SMTP server runs through a proxy server.

## Default Simple Mail Transfer Protocol (SMTP) Configuration

You may configure the Default SMTP provider by the configuration provider using the following settings:

```json
{
  "OrchardCore_Email_Smtp": {
    "DefaultSender": "",
    "DeliveryMethod": "Network",
    "PickupDirectoryLocation": "",
    "Host": "localhost",
    "Port": 25,
    // Uncomment if SMTP server runs through a proxy server
    //"ProxyHost": "proxy.domain.com",
    //"ProxyPort": 5050,
    "EncryptionMethod": "SslTls",
    "AutoSelectEncryption": false,
    "UseDefaultCredentials": false,
    "RequireCredentials": true,
    "Username": "",
    "Password": ""
  }
}
```

For more information about configurations, please refer to [Configuration](../../modules/Configuration/README.md).

!!! note
    Configuration of the Default SMTP provider is not possible through Admin Settings. Utilize the configuration provider for the necessary setup. The provider will appear only if the configuration exists.


## Recipe Configuration

SMTP email settings can be configured using the `Settings` recipe step:

```json
{
  "name": "Settings",
  "SmtpSettings": {
    "IsEnabled": true,
    "DefaultSender": "noreply@example.com",
    "Host": "smtp.example.com",
    "Port": 587,
    "AutoSelectEncryption": true,
    "RequireCredentials": true,
    "UseDefaultCredentials": false,
    "EncryptionMethod": "StartTls",
    "UserName": "smtp-user",
    "Password": "smtp-password",
    "DeliveryMethod": "Network"
  }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `IsEnabled` | Boolean | Whether the SMTP email provider is enabled. |
| `DefaultSender` | String | The default sender email address. |
| `Host` | String | The SMTP server hostname. **Required.** |
| `Port` | Integer | The SMTP server port. Default: `25`. |
| `AutoSelectEncryption` | Boolean | Whether to automatically select the encryption method. |
| `RequireCredentials` | Boolean | Whether the SMTP server requires authentication. |
| `UseDefaultCredentials` | Boolean | Whether to use the default system credentials. |
| `EncryptionMethod` | String | The encryption method. Values: `None`, `SslTls`, `StartTls`. |
| `UserName` | String | The username for SMTP authentication. |
| `Password` | String | The password for SMTP authentication. |
| `ProxyHost` | String | The proxy server hostname. |
| `ProxyPort` | Integer | The proxy server port. |
| `IgnoreInvalidSslCertificate` | Boolean | Whether to ignore invalid SSL certificates. |
| `DeliveryMethod` | String | The delivery method. Values: `Network`, `SpecifiedPickupDirectory`. |
| `PickupDirectoryLocation` | String | The directory path for storing emails when using the pickup directory delivery method. |

## Credits

### MailKit

<https://github.com/jstedfast/MailKit>

Copyright 2013-2019 Xamarin Inc
Licensed under the MIT License
