# Email (`OrchardCore.Email`)

This module provides the infrastructure necessary to send emails using multiple email providers.


## Settings

Enabling the `Email` feature will add a new settings page under `Configurations` → `Settings` → `Email`. You can utilize these settings to set up the default Email provider. The following are the providers that are readily accessible in OrchardCore:


| Provider | Description |
| --- | --- |
| **Simple Mail Transfer Protocol (SMTP)** | The SMTP provider utilizes the SMTP protocol for sending email messages. This provider is a component of the `OrchardCore.Email.Smtp` feature. To activate it, edit the `SMTP` tab in the email settings. For detailed information on configuring the SMTP provider, refer to the [SMTP provider documentation](../Email.Smtp/README.md). |
| **Simple Mail Transfer Protocol (Default SMTP)** | The Default SMTP provider utilization of the SMTP protocol for sending email messages. This provider is a component of the `OrchardCore.Email.Smtp` feature. To activate it, utilize the configuration provider, such as the `appsettings.json` file or environment variables. For detailed information on configuring the SMTP provider, refer to the [SMTP provider documentation](../Email.Smtp/README.md). |
| **Azure Communication Service** | The Azure Communication Service provider utilizes the Azure Communication Services for sending email messages. This provider is a component of the `OrchardCore.Email.Azure` feature. To activate it, edit the `Azure` tab in the email settings. For detailed information on configuring the Azure Communication provider, refer to the [Azure provider documentation](../Email.Azure/README.md). |
| **Default Azure Communication Service** | The Default Azure Communication Service provider utilizes the Azure Communication Services for sending email messages. This provider is a component of the `OrchardCore.Email.Azure` feature. To activate it, utilize the configuration provider, such as the `appsettings.json` file or environment variables. For detailed information on configuring the Azure Communication provider, refer to the [Azure provider documentation](../Email.Azure/README.md). |


## Adding Custom Providers

The `OrchardCore.Email` module provides you with the capability to integrate additional providers for dispatching email messages. To achieve this, you can easily create an implementation of the `IEmailProvider` interface and then proceed to register it using one of the following approaches:

If your provider does not require any settings, you may register it using the `AddEmailProvider<>` extension. For instance:

```csharp
services.AddEmailProvider<YourCustomImplemenation>("A technical name for your implementation")
```

However, if you have a complex provider like the included SMTP one, you may implement `IConfigureOptions<EmailProviderOptions>` and register it using the `AddEmailProviderOptionsConfiguration<>` extension. For instance:

```csharp
services.AddEmailProviderOptionsConfiguration<YourCustomImplemenation>()
```

Here is and example of how we register the SMTP complex provider:

```csharp
public class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly SmtpOptions _smtpOptions;

    public SmtpProviderOptionsConfigurations(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(SmtpEmailProvider))
        {
            IsEnabled = _smtpOptions.IsEnabled
        };

        options.TryAddProvider(SmtpEmailProvider.TechnicalName, typeOptions);
    }
}
```

## Sending Email Messages

An Email message can be sent by injecting `IEmailService` and invoking the `SendAsync` method. For instance:

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

After configuring a provider, you may test it by visiting `Configuration` → `Settings` → `Email Test`.

## Events

You can easily monitor various events triggered during the message-sending process by either implementing the `IEmailServiceEvents` or inheriting from `EmailServiceEventsBase` base class, then registering your service.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/FmgZHpFHCcg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
