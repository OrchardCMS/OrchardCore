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

## Adding Custom Providers

The `OrchardCore.Email` module provides you with the capability to integrate additional providers for dispatching email messages. To achieve this, you can easily create an implementation of the `IEmailProvider` interface and then proceed to register it using one of the following approaches:

If your provider does not require any settings like the `LogProvider`, you may register it like this.

```csharp
services.AddEmailProvider<YourCustomImplemenation>("A technical name for your implementation")
```

However, if you have a complex provider like the SMTP provider, you may implement `IConfigureOptions<EmailProviderOptions>` and register it using the following extensions

```csharp
services.AddEmailProviderOptionsConfiguration<YourCustomImplemenation>()
```

Here is and example of how we register Twilio complex provider:

```csharp
public class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly ISiteService _siteService;

    public SmtpProviderOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(SmtpEmailProvider));

        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
        var settings = site.As<SmtpSettings>();

        typeOptions.IsEnabled = settings.IsEnabled;

        options.TryAddProvider(SmtpEmailProvider.TechnicalName, typeOptions);
    }
}
```

## Sending SMS Message

An Email message can be send by injecting `IEmailService` and invoke the `SendAsync` method. For instance

```csharp
public class TestController
{
    private readonly IEmailService _emailService;

    public TestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendSmsMessage()
    {
        var message = new MailMessage
        {
            To = "to-email@test.com",
            Subject = "Subject for the email",
            Body = "Body of the email",
        };

        var result = await _emailService.SendAsync(message);

        if (result.Succeeded) 
        {
            // message was sent!

            return Ok(result);
        }

        return BadRequest(result);
    }
}
```

## Testing Provider

After configuring a provider, you may test it by visiting `Configuration` >> `Settings` >> `Email Test`.


## Credits

### MailKit

<https://github.com/jstedfast/MailKit>

Copyright 2013-2019 Xamarin Inc
Licensed under the MIT License
