# Email (`OrchardCore.Email`)

This module provides the infrastructure necessary to send emails using multiple email providers.


## SMTP Settings

Enabling the `Email` feature will add a new settings page under `Configurations` → `Settings` → `Email`. You can utilize these settings to set up the default Email provider. The following are the providers that are readily accessible in OrchardCore:


| Provider | Description |
| --- | --- |
| `SMTP` | Opting for this provider enables the utilization of the SMTP protocol for sending email messages. Edit the `SMTP` tab in the email settings to enable this provider. This provider is only available after enabling `OrchardCore.Email.Smtp` feature. |
| `Azure` | Opting for this provider enables the utilization of [Azure Communication Services Email](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview) for sending email messages. Edit the `Azure` tab in the email settings to enable this provider. This provider is only available after enabling `OrchardCore.Email.Azure` feature. |


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
